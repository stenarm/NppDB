using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Kbg.NppPluginNET.PluginInfrastructure;
using Microsoft.Win32.SafeHandles;
using NppDB.Comm;
using NppDB.Core;
using NppDB.Properties;
using NppDB.PostgreSQL;
using NppDB.MSAccess;


namespace NppDB
{
    public static class StringExtension
    {
        public static string RemoveSuffix(this string value, string suffix)
        {
            return value.EndsWith(suffix) ? value.Substring(0, value.Length - suffix.Length) : value;
        }
    }

    public class NppDbPlugin : PluginBase, INppDbCommandHost
    {
        private const string PLUGIN_NAME = "NppDB";
        private string _nppDbPluginDir;
        private string _nppDbConfigDir;
        private string _cfgPath;
        private string _dbConnsPath;
        private string _languageConfigPath;
        private string _translationsConfigPath;
        private FrmDatabaseExplore _frmDbExplorer;
        private int _cmdFrmDbExplorerIdx = -1;
        private readonly Bitmap _imgMan = Resources.DBPPManage16;
        private Icon _tbIcon;
        private readonly Func<IScintillaGateway> _getCurrentEditor = GetGatewayFactory();
        private readonly List<string> _editorErrors = new List<string>();
        private Dictionary<ParserMessageType, string> _warningMessages = new Dictionary<ParserMessageType, string>();
        private Dictionary<ParserMessageType, string> _generalTranslations = new Dictionary<ParserMessageType, string>();
        private Control _currentCtr;
        private const int DEFAULT_SQL_RESULT_HEIGHT = 200;


        static NppDbPlugin()
        {
            AppDomain.CurrentDomain.AssemblyResolve += FindAssembly;
        }
        private static Assembly FindAssembly(object sender, ResolveEventArgs args)
        {
            var logFilePath = Path.Combine(Path.GetTempPath(), "NppDB_ResolveTrace.log");
            try
            {
                File.AppendAllText(logFilePath,
                    $"{DateTime.Now}: Trying to resolve '{args.Name}' requested by '{args.RequestingAssembly?.FullName ?? "Unknown"}'.\r\n");
                var requestedAssemblyName = new AssemblyName(args.Name);
                var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var assemblyPath = Path.Combine(pluginDirectory, requestedAssemblyName.Name + ".dll");
                File.AppendAllText(logFilePath, $"{DateTime.Now}: Looking for '{assemblyPath}'.\r\n");
                if (File.Exists(assemblyPath))
                {
                    File.AppendAllText(logFilePath, $"{DateTime.Now}: Found at '{assemblyPath}'. Loading...\r\n");
                    var loadedAssembly = Assembly.LoadFrom(assemblyPath);
                    File.AppendAllText(logFilePath,
                        $"{DateTime.Now}: Successfully loaded '{loadedAssembly.FullName}'.\r\n");
                    return loadedAssembly;
                }

                if (requestedAssemblyName.Name.Equals("NppDB.Comm", StringComparison.OrdinalIgnoreCase))
                {
                    File.AppendAllText(logFilePath,
                        $"{DateTime.Now}: '{requestedAssemblyName.Name}.dll' not found in plugin dir. Checking N++ base dir...\r\n");
                    var nppBaseDirectory = Path.GetFullPath(Path.Combine(pluginDirectory, "..", ".."));
                    var commPathInBase = Path.Combine(nppBaseDirectory, "NppDB.Comm.dll");
                    if (File.Exists(commPathInBase))
                    {
                        File.AppendAllText(logFilePath,
                            $"{DateTime.Now}: Found NppDB.Comm.dll at '{commPathInBase}'. Loading...\r\n");
                        var loadedAssembly = Assembly.LoadFrom(commPathInBase);
                        File.AppendAllText(logFilePath,
                            $"{DateTime.Now}: Successfully loaded '{loadedAssembly.FullName}'.\r\n");
                        return loadedAssembly;
                    }

                    File.AppendAllText(logFilePath,
                        $"{DateTime.Now}: NppDB.Comm.dll NOT found at '{commPathInBase}'.\r\n");
                }
                else
                {
                    File.AppendAllText(logFilePath, $"{DateTime.Now}: '{assemblyPath}' does not exist.\r\n");
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now}: EXCEPTION in FindAssembly resolving '{args.Name}': {ex}\r\n");
            }
            File.AppendAllText(logFilePath, $"{DateTime.Now}: Failed to resolve '{args.Name}'. Returning null.\r\n");
            return null;
        }

        #region plugin interface

        public static bool IsUnicode() { return true; }
        public void SetInfo(NppData notepadPlusData) { nppData = notepadPlusData; InitPlugin(); }

        public static IntPtr GetFuncsArray(ref int nbF)
        {
            nbF = _funcItems.Items.Count; 
            return _funcItems.NativePointer;
        }
        public bool MessageProc(uint message) {
             switch ((Win32.Wm)message) {
                 case Win32.Wm.MOVE: case Win32.Wm.MOVING: case Win32.Wm.SIZE:
                 case Win32.Wm.ENTER_SIZE_MOVE: case Win32.Wm.EXIT_SIZE_MOVE:
                     UpdateCurrentSqlResult(); break;
                 case Win32.Wm.NOTIFY:
                     break;
                 case Win32.Wm.COMMAND:
                     break;
                 default:
                     throw new ArgumentOutOfRangeException(nameof(message), message, null);
             } 
             return false; 
        }
        private IntPtr _ptrPluginName = IntPtr.Zero;

        public IntPtr GetName()
        {
            if (_ptrPluginName == IntPtr.Zero) _ptrPluginName = Marshal.StringToHGlobalUni(PLUGIN_NAME); 
            return _ptrPluginName;
        }
        public void BeNotified(ScNotification nc) {
             switch (nc.Header.Code) {
                 case (uint)NppMsg.NPPN_TBMODIFICATION: _funcItems.RefreshItems(); SetToolBarIcons(); break;
                 case (uint)NppMsg.NPPN_SHUTDOWN: FinalizePlugin(); break;
                 case (uint)NppMsg.NPPN_FILECLOSED: CloseSqlResult(nc.Header.IdFrom); break;
                 case (uint)NppMsg.NPPN_BUFFERACTIVATED:
                     break;
                 case (uint)SciMsg.SCN_UPDATEUI:
                     ReadTranslations(); break;
                 case (uint)SciMsg.SCN_PAINTED: UpdateCurrentSqlResult(); break;
                 case (uint)SciMsg.SCN_DWELLSTART: ShowTip(nc.Position); break;
                 case (uint)SciMsg.SCN_DWELLEND: CloseTip(); break;
             } }
        #endregion

        #region initialize and finalize a plugin

        private void InitPlugin() {
             DBServerManager.Instance.NppCommandHost = this;
             var sbCfgPath = new StringBuilder(Win32.MaxPath);
             Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MaxPath, sbCfgPath);
             _nppDbPluginDir = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
             _nppDbConfigDir = Path.Combine(sbCfgPath.ToString(), PLUGIN_NAME);
             if (!Directory.Exists(_nppDbConfigDir))
             {
                 try
                 {
                     Directory.CreateDirectory(_nppDbConfigDir);
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show("plugin dir : " + ex.Message); throw;
                 }
             }
             _cfgPath = Path.Combine(_nppDbConfigDir, "config.xml");
             if (File.Exists(_cfgPath))
             {
                 try
                 {
                     Options.Instance.LoadFromXml(_cfgPath);
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show("config.xml : " + ex.Message); throw;
                 }
             }
             _dbConnsPath = Path.Combine(_nppDbConfigDir, "dbconnects.xml");
             if (File.Exists(_dbConnsPath))
             {
                 try
                 {
                     DBServerManager.Instance.LoadFromXml(_dbConnsPath);
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show("dbconnects.xml : "+ ex.Message); throw;
                 }
             }

             var parent = new DirectoryInfo(sbCfgPath.ToString()).Parent;
             var directoryInfo = parent?.Parent;
             if (directoryInfo != null)
                 _languageConfigPath = Path.Combine(directoryInfo.FullName,
                     "nativeLang.xml");

             try
             {
                 ReadTranslations();
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message); throw;
             }
             SetCommand(0, "Execute SQL", Execute, new ShortcutKey(false, false, false, Keys.F9));
             SetCommand(1, "Analyze SQL", Analyze, new ShortcutKey(false, false, true, Keys.F9));
             SetCommand(2, "Database Connect Manager", ToggleDbManager, new ShortcutKey(false, false, false, Keys.F10));
             SetCommand(3, "Clear analysis", ClearAnalysis, new ShortcutKey(true, false, true, Keys.F9));
             SetCommand(4, "Open console", OpenConsole);
             SetCommand(5, "About", ShowAbout);
             _cmdFrmDbExplorerIdx = 2; 
        }

        private void ReadTranslations()
        {
            string localTranslationsConfigPath;
            try
            {
                var xd = new XmlDocument();
                xd.Load(_languageConfigPath);
                var selectedLocalizationFileNode = xd.SelectSingleNode("/NotepadPlus/Native-Langue/@filename");

                if (selectedLocalizationFileNode != null && !string.IsNullOrEmpty(selectedLocalizationFileNode.Value))
                {
                    var selectedLocalizationFileName = selectedLocalizationFileNode.Value;
                    var iniFileName = selectedLocalizationFileName.RemoveSuffix(".xml") + ".ini";
                    localTranslationsConfigPath = Path.Combine(_nppDbPluginDir, iniFileName);
                }
                else
                {
                    Console.WriteLine($"Warning: Could not find selected language filename in '{_languageConfigPath}'. Fallback will be used.");
                    localTranslationsConfigPath = Path.Combine(_nppDbPluginDir, "english.ini");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception reading language config '{_languageConfigPath}': {ex.Message}. Defaulting to english.ini.");
                localTranslationsConfigPath = Path.Combine(_nppDbPluginDir, "english.ini");
            }

            if (!string.IsNullOrEmpty(_translationsConfigPath) && _translationsConfigPath.Equals(localTranslationsConfigPath))
            {
                return;
            }
            _translationsConfigPath = localTranslationsConfigPath;

            if (string.IsNullOrEmpty(_translationsConfigPath) || !File.Exists(_translationsConfigPath))
            {
                MessageBox.Show($"Translation INI file not found at: {_translationsConfigPath ?? "Path not set"}. Warnings will not be translated.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                _warningMessages.Clear();
                _generalTranslations.Clear();
                _generalTranslations[ParserMessageType.WARNING_FORMAT] = "Warning at {0}:{1} : {2}";
                return;
            }

            Console.WriteLine($@"Plugin translations being loaded from: {_translationsConfigPath}");

            var warningTypesToLoad = Enum.GetValues(typeof(ParserMessageType)).Cast<ParserMessageType>().Where(type => type != ParserMessageType.WARNING_FORMAT).ToDictionary(type => type, type => string.Empty);

            var generalTypesToLoad = new Dictionary<ParserMessageType, string>
            {
                { ParserMessageType.WARNING_FORMAT, "Warning at {0}:{1} : {2}" }
            };

            _warningMessages.Clear();
            _generalTranslations.Clear();

            ReadTranslations("Warnings", warningTypesToLoad, ref _warningMessages);
            ReadTranslations("General", generalTypesToLoad, ref _generalTranslations);
        }

        private void ReadTranslations(string sectionName, in Dictionary<ParserMessageType, string> inputDictionary, ref Dictionary<ParserMessageType, string> outputDictionary)
        {
            var codePage = Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_GETCODEPAGE, 0, 0).ToInt32();
            foreach (var entry in inputDictionary)
            {
                var iniKeyName = Enum.GetName(typeof(ParserMessageType), entry.Key);

                if (string.IsNullOrEmpty(iniKeyName))
                {
                    Console.WriteLine($"Warning: Could not get name for enum value {entry.Key}. Skipping translation.");
                    continue;
                }

                const int bufferSize = 256;

                var bufferBytes = new byte[bufferSize];

                try
                {
                    Win32.GetPrivateProfileString(
                        sectionName,
                        iniKeyName,
                        entry.Value, bufferBytes,
                        bufferSize,
                        _translationsConfigPath
                    );

                    var translatedText = Encoding.GetEncoding(codePage).GetString(bufferBytes).TrimEnd('\0');

                    outputDictionary[entry.Key] = translatedText;
                }
                catch (ArgumentException argEx)
                {
                    Console.WriteLine(
                        $"Warning: Encoding error for key '{iniKeyName}' with codepage {codePage}. Using default. Error: {argEx.Message}");
                    outputDictionary[entry.Key] = entry.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Warning: Failed to read translation for key '{iniKeyName}'. Using default. Error: {ex.Message}");
                    outputDictionary[entry.Key] = entry.Value;
                }
            }
        }

        private const uint STD_OUTPUT_HANDLE = 0xFFFFFFF5; private const int MY_CODE_PAGE = 437;
        /// <summary>
        /// Allocates a new console window for the application and redirects
        /// the standard console output (Console.WriteLine, etc.) to it.
        /// Useful for debugging purposes within the Notepad++ plugin.
        /// </summary>
        private static void OpenConsole()
        {
            try
            {
                Win32.AllocConsole();

                var stdOutHandle = Win32.GetStdHandle(STD_OUTPUT_HANDLE);

                var safeFileHandle = new SafeFileHandle(stdOutHandle, true);

                var fileStream = new FileStream(safeFileHandle, FileAccess.Write);

                var standardOutput = new StreamWriter(fileStream, Encoding.GetEncoding(MY_CODE_PAGE))
                {
                    AutoFlush = true
                };

                Console.SetOut(standardOutput);

                Console.WriteLine("Debug console successfully opened.");
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Error opening console (IOException): {ioEx.Message}", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening console: {ex.Message}", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Sets the toolbar icon for the Database Connect Manager menu item.
        /// This method should be called after Notepad++ has initialized its toolbar,
        /// typically in response to the NPPN_TBMODIFICATION notification.
        /// </summary>
        private void SetToolBarIcons()
        {
            if (_cmdFrmDbExplorerIdx < 0 || _cmdFrmDbExplorerIdx >= _funcItems.Items.Count)
            {
                Console.WriteLine($"Error: Invalid command index ({_cmdFrmDbExplorerIdx}) for setting toolbar icon.");
                return;
            }

            if (_imgMan == null)
            {
                Console.WriteLine($"Error: Toolbar icon image resource (_imgMan) is null.");
                return;
            }

            var pTbIcons = IntPtr.Zero;

            try
            {
                var tbIcons = new toolbarIcons
                {
                    hToolbarBmp = _imgMan.GetHbitmap()
                };

                pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));

                Marshal.StructureToPtr(tbIcons, pTbIcons, false);

                Win32.SendMessage(
                    nppData._nppHandle,
                    (uint)NppMsg.NPPM_ADDTOOLBARICON,
                    _funcItems.Items[_cmdFrmDbExplorerIdx]._cmdID, pTbIcons);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting toolbar icon: {ex.Message}",
                                PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (pTbIcons != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pTbIcons);
                }
            }
        }
        private void FinalizePlugin()
        {
            if (_ptrPluginName != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_ptrPluginName);
                _ptrPluginName = IntPtr.Zero;
            }

            try
            {
                Options.Instance.SaveToXml(_cfgPath);

                DBServerManager.Instance.SaveToXml(_dbConnsPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving plugin configuration during finalization: " + ex.Message,
                    PLUGIN_NAME, MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        #endregion

        private void Analyze()
        {
            AnalyzeAndExecute(true, true);
        }

        private void Execute()
        {
            AnalyzeAndExecute(false, false);
        }
        
        

        private void AnalyzeAndExecute(bool showFeedbackOnSuccess, bool onlyAnalyze)
        {
            SqlResult attachedResult = null;
            IScintillaGateway editor = null;
            var baseLine = 0;
            var selectionOnly = true;

            try
            {
                var bufId = GetCurrentBufferId();
                if (bufId == IntPtr.Zero) return;

                editor = _getCurrentEditor();
                if (editor == null)
                {
                    MessageBox.Show("Could not get editor instance.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var currentScintilla = GetCurrentScintilla();
                var textToParse = GetScintillaText(currentScintilla, true);
                if (string.IsNullOrWhiteSpace(textToParse))
                {
                    selectionOnly = false;
                    textToParse = GetScintillaText(currentScintilla, false);
                }
                if (string.IsNullOrWhiteSpace(textToParse))
                {
                    ClearAnalysisIndicators(editor);
                    return;
                }
                textToParse = textToParse.Replace("\t", "    ");

                if (selectionOnly)
                {
                    var start = editor.GetSelectionStart();
                    baseLine = editor.LineFromPosition(start);
                }

                attachedResult = SQLResultManager.Instance.GetSQLResult(bufId);

                ParserResult analysisResult;
                if (attachedResult != null)
                {
                    if (!attachedResult.LinkedDbConnect.IsOpened && !onlyAnalyze)
                    {
                        MessageBox.Show("Database connection is closed. Cannot execute.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var caretPosition = GetCaretPosition();
                    analysisResult = attachedResult.Parse(textToParse, caretPosition);
                    ShowSqlResult(attachedResult);
                }
                else
                {
                    if (!onlyAnalyze)
                    {
                        MessageBox.Show("No database connection attached for execution.\nPlease attach a connection first.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    ISqlExecutor chosenExecutor;
                    using (var dialectDlg = new FrmSelectSqlDialect())
                    {
                        IWin32Window owner = Control.FromHandle(nppData._nppHandle);
                        if (dialectDlg.ShowDialog(owner) != DialogResult.OK)
                        {
                            return;
                        }

                        switch (dialectDlg.SelectedDialect)
                        {
                            case SqlDialect.PostgreSQL:
                                chosenExecutor = new PostgreSQLExecutor(null);
                                break;
                            case SqlDialect.MsAccess:
                                chosenExecutor = new MSAccessExecutor(null);
                                break;
                            case SqlDialect.None:
                            default:
                                MessageBox.Show("No SQL dialect selected for analysis.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                        }
                    }

                    var caretPosition = GetCaretPosition();
                    analysisResult = chosenExecutor.Parse(textToParse, caretPosition);
                }

                var hasErrors = analysisResult?.Errors?.Any() ?? false;
                var hasWarns = analysisResult?.Commands?.Any(c => c != null && ((c.Warnings?.Any() ?? false) || (c.AnalyzeErrors?.Any() ?? false))) ?? false;

                if (showFeedbackOnSuccess || hasErrors || hasWarns)
                {
                    if (analysisResult != null)
                    {
                        DisplayAnalysisFeedback(editor, analysisResult, baseLine);
                    } else {
                         ClearAnalysisIndicators(editor);
                    }
                } else {
                    ClearAnalysisIndicators(editor);
                }

                attachedResult?.SetError(hasErrors ? "Analysis found errors." : (hasWarns ? "Analysis found warnings." : ""));

                if (onlyAnalyze) return;
                {
                    if (hasErrors)
                    {
                        MessageBox.Show("Execution cancelled due to parsing errors. Please fix the errors shown.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (analysisResult?.Commands == null || !analysisResult.Commands.Any())
                    {
                        MessageBox.Show("No valid SQL commands found to execute.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var commandIndex = (analysisResult.EnclosingCommandIndex >= 0 && analysisResult.EnclosingCommandIndex < analysisResult.Commands.Count)
                        ? analysisResult.EnclosingCommandIndex : 0;
                    var commandsToExecute = selectionOnly
                        ? analysisResult.Commands.Where(c => c != null).Select(c => c.Text).ToList()
                        : analysisResult.Commands.Skip(commandIndex).Take(1).Where(c => c != null).Select(c => c.Text).ToList();

                    if (commandsToExecute.Any(cmd => !string.IsNullOrWhiteSpace(cmd)))
                    {
                        attachedResult.SetError("");
                        attachedResult.Execute(commandsToExecute);
                    } else {
                        MessageBox.Show("Could not determine specific command to execute based on selection or caret position.", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during analysis/execution:\n{ex.Message}\n{ex.StackTrace}", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
                attachedResult?.SetError($"Unexpected Error: {ex.Message}");
                if (editor != null) ClearAnalysisIndicators(editor);
            }
        }

        private static CaretPosition GetCaretPosition()
        {
            return new NppDB.Comm.CaretPosition
            {
                Line = Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTLINE, 0, 0).ToInt32() + 1,
                Column = Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTCOLUMN, 0, 0).ToInt32(),
                Offset = Win32.SendMessage(nppData._scintillaMainHandle, (uint)SciMsg.SCI_GETCURRENTPOS, 0, 0).ToInt32(),
            };
        }

        private static unsafe string GetScintillaText(IntPtr scintillaHnd, bool selectionOnly)
        {
            if (scintillaHnd == IntPtr.Zero) return string.Empty;

            byte[] textBuffer;
            int length;

            var codePage = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETCODEPAGE, 0, 0).ToInt32();
            if (codePage == 0) codePage = 65001;

            if (selectionOnly)
            {
                length = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETSELTEXT, 0, 0).ToInt32();
                if (length <= 0) return string.Empty;

                textBuffer = new byte[length + 1];
                fixed (byte* textPtr = textBuffer)
                {
                    Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETSELTEXT, 0, (IntPtr)textPtr);
                }

                length = Array.IndexOf(textBuffer, (byte)0);
                if (length == -1) length = textBuffer.Length -1;
            }
            else
            {
                length = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETTEXTLENGTH, 0, 0).ToInt32();
                if (length <= 0) return string.Empty;

                textBuffer = new byte[length + 1];
                fixed (byte* textPtr = textBuffer)
                {
                    Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETTEXT, (IntPtr)(length + 1), (IntPtr)textPtr);
                }

                length = Array.IndexOf(textBuffer, (byte)0);
                 if (length == -1) length = textBuffer.Length -1;
            }

            string text;
            try
            {
                text = Encoding.GetEncoding(codePage).GetString(textBuffer, 0, length);
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is ArgumentException || ex is NotSupportedException)
            {
                MessageBox.Show($"Scintilla Text Encoding Error (Codepage: {codePage}): {ex.Message}", "Encoding Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { text = Encoding.UTF8.GetString(textBuffer, 0, length); } catch { text = string.Empty; }
            }
            catch(Exception genEx)
            {
                 MessageBox.Show($"Error getting Scintilla text: {genEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 text = string.Empty;
            }

            return text;
        }

        private void DisplayAnalysisFeedback(IScintillaGateway editor, ParserResult parserResult, int baseLine)
        {
            if (editor == null || parserResult == null) return;

            var textLength = editor.GetTextLength();

            editor.AnnotationClearAll();
            editor.SetIndicatorCurrent(20);
            editor.IndicatorClearRange(0, textLength);
            lock (_editorErrors)
            {
                _editorErrors.Clear();
            }


            const int WARN_STYLE = 199;
            const int ERROR_STYLE = 200;
            try {
                editor.StyleSetFont(WARN_STYLE, "Tahoma"); editor.StyleSetSize(WARN_STYLE, 8);
                editor.StyleSetBack(WARN_STYLE, new Colour(250, 250, 200)); editor.StyleSetFore(WARN_STYLE, new Colour(100, 100, 0));
                editor.StyleSetFont(ERROR_STYLE, "Tahoma"); editor.StyleSetSize(ERROR_STYLE, 8);
                editor.StyleSetBack(ERROR_STYLE, new Colour(255, 220, 220)); editor.StyleSetFore(ERROR_STYLE, new Colour(180, 0, 0));
            } catch (Exception styleEx){ Console.WriteLine($"Style Setting Error: {styleEx.Message}"); }

            var lineAnnotations = new Dictionary<int, Tuple<int, StringBuilder>>();
            var allMessages = new List<ParserMessage>();
            if (parserResult.Errors != null) allMessages.AddRange(parserResult.Errors.Where(e => e != null));
            if (parserResult.Commands != null) {
                 foreach(var cmd in parserResult.Commands.Where(c => c != null)) {
                     if (cmd.Warnings != null) allMessages.AddRange(cmd.Warnings.Where(w => w != null));
                     if (cmd.AnalyzeErrors != null) allMessages.AddRange(cmd.AnalyzeErrors.Where(w => w != null));
                 }
            }

            foreach (var msg in allMessages.OrderBy(m => m.StartLine).ThenBy(m => m.StartColumn))
            {
                if (msg.StartLine <= 0 || msg.StartColumn < 0 || msg.StartOffset < 0 || msg.StopOffset < msg.StartOffset) continue;

                var isError = msg is ParserError;
                var style = isError ? ERROR_STYLE : WARN_STYLE;
                var prefix = isError ? "Error:" : "Warning:";
                var line = baseLine + msg.StartLine - 1;
                if (line < 0) line = 0;

                var messageKeyText = _warningMessages.TryGetValue(msg.Type, out var message) ? message : msg.Text;
                var messageText = $"{prefix} (L{msg.StartLine} C{msg.StartColumn + 1}) {messageKeyText?.Replace("\\n", "\n") ?? ""}";

                if (lineAnnotations.TryGetValue(line, out var existingAnnotation)) {
                    existingAnnotation.Item2.AppendLine().Append(messageText);
                    if (isError && existingAnnotation.Item1 != ERROR_STYLE) lineAnnotations[line] = Tuple.Create(style, existingAnnotation.Item2);
                } else {
                    lineAnnotations[line] = Tuple.Create(style, new StringBuilder(messageText));
                }

                if (!isError) continue;
                int length;
                int startBytePos;

                if (baseLine == 0) {
                    startBytePos = msg.StartOffset;
                    length = Math.Max(1, msg.StopOffset - msg.StartOffset + 1);
                } else {
                    startBytePos = msg.StartOffset;
                    length = Math.Max(1, msg.StopOffset - msg.StartOffset + 1);
                }

                lock(_editorErrors) _editorErrors.Add(messageText);

                try {
                    editor.SetIndicatorCurrent(20);
                    editor.IndicSetStyle(20, IndicatorStyle.SQUIGGLE);
                    editor.IndicSetFore(20, new Colour(255, 0, 0));
                    lock (_editorErrors)
                    {
                        editor.SetIndicatorValue(_editorErrors.Count);
                    }
                    editor.IndicatorFillRange(startBytePos, length);
                } catch(Exception indicatorEx) { Console.WriteLine($"Indicator Error: {indicatorEx.Message} Pos:{startBytePos} Len:{length}");}
            }

            foreach (var entry in lineAnnotations.Where(entry => entry.Key >= 0 && entry.Key < editor.GetLineCount()))
            {
                try {
                    editor.AnnotationSetStyle(entry.Key, entry.Value.Item1);
                    editor.AnnotationSetText(entry.Key, entry.Value.Item2.ToString());
                } catch (Exception annEx) { Console.WriteLine($"Annotation Error Line {entry.Key}: {annEx.Message}");}
            }

            if (lineAnnotations.Count > 0) {
                try { editor.AnnotationSetVisible(AnnotationVisible.BOXED); }
                catch
                {
                    // ignored
                }
            }

            try { editor.SetMouseDwellTime(500); }
            catch
            {
                // ignored
            }
        }


        private void ClearAnalysisIndicators(IScintillaGateway editor)
        {
            if (editor == null) editor = _getCurrentEditor();
            if (editor == null) return;
            try
            {
                var textLength = editor.GetTextLength();
                editor.AnnotationClearAll();
                editor.SetIndicatorCurrent(20);
                editor.IndicatorClearRange(0, textLength);
                lock (_editorErrors) _editorErrors.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing analysis indicators: {ex.Message}");
            }
        }
        
        private void ShowTip(Position position)
        {
            var editor = _getCurrentEditor();
            var value = editor.IndicatorValueAt(20, Convert.ToInt32(position.Value));
            lock (_editorErrors)
            {
                if (value <= 0 || value - 1 >= _editorErrors.Count) return;
                try {
                    editor.CallTipShow(Convert.ToInt32(position.Value), _editorErrors[value - 1]);
                } catch (Exception ex) { Console.WriteLine($"CallTipShow Error: {ex.Message}"); }
            }
        }

        private void CloseTip()
        {
            try
            {
                var editor = _getCurrentEditor();
                editor.CallTipCancel();
            }
            catch (Exception ex) { Console.WriteLine($"CallTipCancel Error: {ex.Message}"); }
        }

        private void ClearAnalysis()
        {
             ClearAnalysisIndicators(_getCurrentEditor());
        }

        public void SendNppMessage(uint msg, IntPtr wParam, int lParam)
        {
            Win32.SendMessage(nppData._nppHandle, msg, wParam, lParam);
        }

        public object Execute(NppDbCommandType type, object[] parameters)
        {
            try
            {
                switch (type)
                {
                    case NppDbCommandType.ActivateBuffer:
                        ActivateBufferId((int)parameters[0]);
                        break;
                    case NppDbCommandType.AppendToCurrentView:
                        AppendToScintillaText(GetCurrentScintilla(), (string)parameters[0]);
                        break;
                    case NppDbCommandType.NewFile:
                        NewFile();
                        break;
                    case NppDbCommandType.CreateResultView:
                        if (parameters != null && parameters.Length >= 3 && parameters[0] is IntPtr p0 && parameters[1] is IDbConnect p1 && parameters[2] is ISqlExecutor p2)
                             return AddSqlResult(p0, p1, p2);
                        else return null;
                    case NppDbCommandType.DestroyResultView:
                        CloseCurrentSqlResult();
                        break;
                    case NppDbCommandType.ExecuteSQL:
                         if (parameters != null && parameters.Length >= 2 && parameters[0] is IntPtr pSql0 && parameters[1] is string pSql1)
                             ExecuteSql(pSql0, pSql1);
                         break;
                    case NppDbCommandType.GetAttachedBufferID:
                        return GetCurrentAttachedBufferId();
                    case NppDbCommandType.GetActivatedBufferID:
                        return GetCurrentBufferId();
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command {type}: {ex.Message}", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        private static void SetResultPos(Control control)
        {
            try
            {
                var nppHwnd = nppData._nppHandle;
                var hndScin = GetCurrentScintilla();

                if (hndScin == IntPtr.Zero || nppHwnd == IntPtr.Zero) return;

                if (!Win32.GetClientRect(nppHwnd, out var nppClientRect)) return;
                var nppClientHeight = nppClientRect.Bottom - nppClientRect.Top;

                var hStatusBar = Win32.FindWindowEx(nppHwnd, IntPtr.Zero, "msctls_statusbar32", null);
                var statusBarHeight = 0;
                if (hStatusBar != IntPtr.Zero)
                {
                    if (Win32.GetWindowRect(hStatusBar, out var statusBarRect))
                    {
                        statusBarHeight = statusBarRect.Bottom - statusBarRect.Top;
                    }
                }

                var availableHeight = nppClientHeight - statusBarHeight;

                var resultTop = availableHeight - DEFAULT_SQL_RESULT_HEIGHT;
                if (resultTop < 0) resultTop = 0;

                if (!Win32.GetWindowRect(hndScin, out var scinScreenRect)) return;

                var scinWidth = scinScreenRect.Right - scinScreenRect.Left;
                var scinTopLeftScreen = new Point(scinScreenRect.Left, scinScreenRect.Top);
                Win32.ScreenToClient(nppHwnd, ref scinTopLeftScreen);
                var scinLeft = scinTopLeftScreen.X;
                var scinTop = scinTopLeftScreen.Y;

                if (resultTop < scinTop) resultTop = scinTop;

                var newScinHeight = resultTop - scinTop;
                if (newScinHeight < 50) newScinHeight = 50;

                Win32.SetWindowPos(
                    hndScin, IntPtr.Zero,
                    scinLeft, scinTop,
                    scinWidth, newScinHeight,
                    Win32.SetWindowPosFlags.NO_Z_ORDER | Win32.SetWindowPosFlags.NO_ACTIVATE
                    );

                Win32.SetWindowPos(
                    control.Handle, IntPtr.Zero,
                    scinLeft, resultTop, scinWidth, DEFAULT_SQL_RESULT_HEIGHT,
                    Win32.SetWindowPosFlags.SHOW_WINDOW | Win32.SetWindowPosFlags.NO_ACTIVATE
                    );

                 if (!control.Visible) control.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in SetResultPos: {ex.Message}", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ResetViewPos(Control control)
        {
            try
            {
                var nppHwnd = nppData._nppHandle;
                var hndScin = GetCurrentScintilla();

                if (hndScin == IntPtr.Zero || nppHwnd == IntPtr.Zero) return;

                if (!Win32.GetClientRect(nppHwnd, out var nppClientRect)) return;
                var nppClientHeight = nppClientRect.Bottom - nppClientRect.Top;

                var hStatusBar = Win32.FindWindowEx(nppHwnd, IntPtr.Zero, "msctls_statusbar32", null);
                var statusBarHeight = 0;
                if (hStatusBar != IntPtr.Zero)
                {
                    if (Win32.GetWindowRect(hStatusBar, out var statusBarRect))
                    {
                         statusBarHeight = statusBarRect.Bottom - statusBarRect.Top;
                    }
                }

                var availableHeight = nppClientHeight - statusBarHeight;

                if (!Win32.GetWindowRect(hndScin, out var scinScreenRect)) return;

                var scinWidth = scinScreenRect.Right - scinScreenRect.Left;
                var scinTopLeftScreen = new Point(scinScreenRect.Left, scinScreenRect.Top);
                Win32.ScreenToClient(nppHwnd, ref scinTopLeftScreen);
                var scinLeft = scinTopLeftScreen.X;
                var scinTop = scinTopLeftScreen.Y;

                var restoredScinHeight = availableHeight - scinTop;
                if (restoredScinHeight < 50) restoredScinHeight = 50;

                Win32.SetWindowPos(
                    hndScin, IntPtr.Zero,
                    scinLeft, scinTop,
                    scinWidth, restoredScinHeight,
                    Win32.SetWindowPosFlags.NO_Z_ORDER | Win32.SetWindowPosFlags.NO_ACTIVATE
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ResetViewPos: {ex.Message}", PLUGIN_NAME, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ExecuteSql(IntPtr bufferId, string query)
        {
            var result = SQLResultManager.Instance.GetSQLResult(bufferId);

            if (result == null)
            {
                return;
            }

            result.SetError("");

            result.Execute(new List<string> { query });
        }
        
        private void CloseCurrentSqlResult() { var bufId = GetCurrentBufferId(); CloseSqlResult(bufId); }

         private void CloseSqlResult(IntPtr bufferId)
         {
             var result = SQLResultManager.Instance.GetSQLResult(bufferId);
             if (result == null) return;

             SQLResultManager.Instance.Remove(bufferId);

             var wasCurrentControl = (_currentCtr == result);

             if (wasCurrentControl)
             {
                 _currentCtr = null;
             }

             if (result.IsHandleCreated && !result.IsDisposed)
             {
                 ResetViewPos(result);
             }

             if (result.IsHandleCreated && !result.IsDisposed)
                 Win32.DestroyWindow(result.Handle);

             if (!result.IsDisposed)
                 result.Dispose();
         }

         private void Disconnect(IDbConnect connection)
         {
             connection.Disconnect(); 
             CloseCurrentSqlResult(); 
             SQLResultManager.Instance.RemoveSQLResults(connection);
         }

         private void Unregister(IDbConnect connection)
         {
             DBServerManager.Instance.Unregister(connection); 
             connection.Disconnect(); 
             CloseCurrentSqlResult(); 
             SQLResultManager.Instance.RemoveSQLResults(connection);
         }

         private void ToggleDbManager()
         {
             if (_frmDbExplorer == null)
             {
                 _frmDbExplorer = new FrmDatabaseExplore(); 
                 _frmDbExplorer.AddNotifyHandler((ref Message msg) =>
                 {
                     var nc = (ScNotification)Marshal.PtrToStructure(msg.LParam, typeof(ScNotification)); 
                     if (nc.Header.Code != (uint)DockMgrMsg.DMN_CLOSE) return; 
                     Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[_cmdFrmDbExplorerIdx]._cmdID, 0);
                 }); 
                 _frmDbExplorer.DisconnectHandler = Disconnect; _frmDbExplorer.UnregisterHandler = Unregister;
                 using (var newBmp = new Bitmap(16, 16))
                 {
                     var g = Graphics.FromImage(newBmp); 
                     var colorMap = new ColorMap[1]; 
                     colorMap[0] = new ColorMap
                     {
                         OldColor = Color.Fuchsia,
                         NewColor = Color.FromKnownColor(KnownColor.ButtonFace)
                     };
                     var attr = new ImageAttributes(); 
                     attr.SetRemapTable(colorMap); 
                     g.DrawImage(_imgMan, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr); 
                     _tbIcon = Icon.FromHandle(newBmp.GetHicon());
                 } 
                 var nppTbData = new NppTbData
                 {
                     hClient = _frmDbExplorer.Handle,
                     pszName = _funcItems.Items[_cmdFrmDbExplorerIdx]._itemName,
                     dlgID = _cmdFrmDbExplorerIdx,
                     uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR,
                     hIconTab = (uint)_tbIcon.Handle,
                     pszModuleName = PLUGIN_NAME
                 };
                 var ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData)); 
                 Marshal.StructureToPtr(nppTbData, ptrNppTbData, false); 
                 Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData); 
                 Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[_cmdFrmDbExplorerIdx]._cmdID, 1); 
                 Marshal.FreeHGlobal(ptrNppTbData);
             }
             else
             {
                 var nppMsg = NppMsg.NPPM_DMMSHOW; var toggleStatus = 1;
                 if (_frmDbExplorer.Visible)
                 {
                     nppMsg = NppMsg.NPPM_DMMHIDE; toggleStatus = 0;
                 } 
                 Win32.SendMessage(nppData._nppHandle, (uint)nppMsg, 0, _frmDbExplorer.Handle); 
                 Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[_cmdFrmDbExplorerIdx]._cmdID, toggleStatus);
             }
         }
         private static void ShowAbout() { var dlg = new frmAbout(); dlg.ShowDialog(); }

         private void UpdateCurrentSqlResult()
        {
            if (SQLResultManager.Instance == null) return;

            var bufId = GetCurrentBufferId();
            SqlResult targetResult = null;

            if (bufId != IntPtr.Zero)
            {
                targetResult = SQLResultManager.Instance.GetSQLResult(bufId);
                if (targetResult != null && targetResult.IsDisposed)
                {
                    targetResult = null;
                }
            }

            var controlToHide = _currentCtr;
            Control controlToShow = targetResult;

            if (controlToHide == controlToShow)
            {
                if (controlToShow != null && controlToShow.IsHandleCreated && !controlToShow.IsDisposed && controlToShow.Visible)
                {
                    SetResultPos(controlToShow);
                }
                return;
            }

            if (controlToHide != null && controlToHide.IsHandleCreated && !controlToHide.IsDisposed && controlToHide.Visible)
            {
                ResetViewPos(controlToHide);
                controlToHide.Visible = false;
            }

            _currentCtr = controlToShow;

            if (_currentCtr != null && _currentCtr.IsHandleCreated && !_currentCtr.IsDisposed)
            {
                SetResultPos(_currentCtr);
            }
        }
         private static Control AddSqlResult(IntPtr bufId, IDbConnect connect, ISqlExecutor sqlExecutor)
         {
             var ctr = SQLResultManager.Instance.CreateSQLResult(bufId, connect, sqlExecutor);

             var ret = Win32.SetParent(ctr.Handle, nppData._nppHandle);
             if (ret == IntPtr.Zero) MessageBox.Show(@"setparent fail");

             ctr.Visible = false;

             return ctr;
         }
         private void ShowSqlResult(SqlResult control)
         {
             if (control == null || control.IsDisposed) return;

             _currentCtr = control;

             SetResultPos(control);


             if (!control.LinkedDbConnect.IsOpened)
             {
                 control.SetError("This database connection is closed. Please connect again.");
             }
         }

         private static void NewFile()
         {
             Win32.SendMessage(nppData._nppHandle, (uint)Win32.Wm.COMMAND, (int)NppMenuCmd.IDM_FILE_NEW, 0);
         }
         private static IntPtr? GetCurrentAttachedBufferId()
         {
             var bufferId = GetCurrentBufferId();

             if (bufferId == IntPtr.Zero)
             {
                 return null;
             }

             var result = SQLResultManager.Instance.GetSQLResult(bufferId);

             if (result == null)
             {
                 return null;
             }

             return bufferId;
         }
         private static void ActivateBufferId(int bufferId) { 
             Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_ACTIVATEDOC, 0, bufferId);
         }

         private static IntPtr GetCurrentBufferId()
         {
             return Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0);
         }
         private static void AppendToScintillaText(IntPtr scintillaHnd, string text)
         {
             if (scintillaHnd == IntPtr.Zero || string.IsNullOrEmpty(text))
             {
                 return;
             }

             var codePage = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETCODEPAGE, 0, 0).ToInt32();
             if (codePage == 0) codePage = 65001;

             var ptrChars = IntPtr.Zero;

             try
             {
                 var bytes = Encoding.GetEncoding(codePage).GetBytes(text);

                 ptrChars = Marshal.AllocHGlobal(bytes.Length);

                 Marshal.Copy(bytes, 0, ptrChars, bytes.Length);

                 Win32.SendMessage(scintillaHnd, SciMsg.SCI_APPENDTEXT, (IntPtr)bytes.Length, ptrChars);
             }
             catch (Exception ex)
             {
                 throw new ApplicationException("Appending text to Scintilla (SCI_APPENDTEXT) failed.", ex);
             }
             finally
             {
                 if (ptrChars != IntPtr.Zero)
                 {
                     Marshal.FreeHGlobal(ptrChars);
                 }
             }
         }
    }
}