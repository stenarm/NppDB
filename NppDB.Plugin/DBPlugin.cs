using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Win32.SafeHandles;

using NppDB.Comm;
using NppDB.Core;
using Kbg.NppPluginNET.PluginInfrastructure;

namespace NppDB
{
    public static class StringExtension
    {
        public static string RemoveSuffix(this string value, string suffix)
        {
            return value.EndsWith(suffix) ? value.Substring(0, value.Length - suffix.Length) : value;
        }
    }
    
    public class NppDBPlugin : PluginBase, INppDBCommandHost
    {
        private const string PluginName = "NppDB";
        private string _nppDbPluginDir;
        private string _nppDbConfigDir;
        private string _cfgPath;
        private string _dbConnsPath;
        private string _languageConfigPath;
        private string _translationsConfigPath;
        private FrmDatabaseExplore _frmDBExplorer;
        private int _cmdFrmDBExplorerIdx = -1;
        private Bitmap _imgMan = Properties.Resources.DBPPManage16;
        private Icon tbIcon;
        private readonly Func<IScintillaGateway> GetCurrentEditor = GetGatewayFactory();
        private IList<string> editorErrors = new List<string>();
        private Dictionary<ParserMessageType, string> _warningMessages = new Dictionary<ParserMessageType, string>();
        private Dictionary<ParserMessageType, string> _generalTranslations = new Dictionary<ParserMessageType, string>();

        #region plugin interface
        public bool isUnicode()
        {
            return true;
        }

        public void setInfo(NppData notepadPlusData)
        {
            nppData = notepadPlusData;
            InitPlugin();
        }

        public IntPtr getFuncsArray(ref int nbF)
        {
            nbF = _funcItems.Items.Count;
            return _funcItems.NativePointer;
        }

        public bool messageProc(uint Message, UIntPtr wParam, IntPtr lParam)
        {
            switch ((Win32.WM)Message)
            {
                case Win32.WM.Move:
                case Win32.WM.Moving:
                case Win32.WM.Size:
                case Win32.WM.EnterSizeMove:
                case Win32.WM.ExitSizeMove:
                    UpdateCurrentSQLResult();
                    break;
            }
            return false;
        }

        //todo implement to free _ptrPluginName in dispose()
        private IntPtr _ptrPluginName = IntPtr.Zero;
        public IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(PluginName);
            return _ptrPluginName;
        }
        
        public void beNotified(ScNotification nc)
        {
            switch (nc.Header.Code)
            {
                case (uint)NppMsg.NPPN_TBMODIFICATION:
                    _funcItems.RefreshItems();
                    SetToolBarIcons();
                    break;
                case (uint)NppMsg.NPPN_SHUTDOWN:
                    FinalizePlugin();
                    break;
                case (uint)NppMsg.NPPN_FILECLOSED:
                    CloseSQLResult(nc.Header.IdFrom);
                    break;
                case (uint)NppMsg.NPPN_BUFFERACTIVATED:
                    break;
                case (uint)SciMsg.SCN_UPDATEUI:
                    ReadTranslations();
                    break;
                case (uint)SciMsg.SCN_PAINTED:
                    UpdateCurrentSQLResult();
                    break;
                /*
                case (uint)SciMsg.SCN_MODIFIED:
                    if ((nc.ModificationType & 1) == 1)
                        HandleTextUpdate();
                    break;
                */
                case (uint)SciMsg.SCN_DWELLSTART:
                    ShowTip(nc.Position);
                    break;
                case (uint)SciMsg.SCN_DWELLEND:
                    CloseTip();
                    break;
            }
        }

        #endregion

        #region initialize and finalize a plugin
        //initialize plugin's command menus  
        private void InitPlugin()
        {
            //plugin configuration
            DBServerManager.Instance.NppCommandHost = this;

            var sbCfgPath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbCfgPath);
            
            _nppDbPluginDir = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
            _nppDbConfigDir = Path.Combine(sbCfgPath.ToString(), PluginName);
            if (!Directory.Exists(_nppDbConfigDir))
            {
                try
                {
                    Directory.CreateDirectory(_nppDbConfigDir);
                } catch (Exception ex) 
                { 
                    MessageBox.Show("plugin dir : " + ex.Message);
                    throw ex;
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
                    MessageBox.Show("config.xml : " + ex.Message); 
                    throw ex;
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
                    MessageBox.Show("dbconnects.xml : "+ ex.Message); 
                    throw ex;
                }
            }
            
            // \AppData\Roaming\Notepad++\plugins\config\ -> \AppData\Roaming\Notepad++\
            _languageConfigPath = Path.Combine(new DirectoryInfo(sbCfgPath.ToString()).Parent.Parent.FullName, "nativeLang.xml");
            try
            {
                ReadTranslations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }

            SetCommand(0, "Execute SQL", Execute, new ShortcutKey(false, false, false, Keys.F9));
            SetCommand(1, "Analyze SQL", Analyze, new ShortcutKey(false, false, true, Keys.F9));
            SetCommand(2, "Database Connect Manager", ToggleDBManager, new ShortcutKey(false, false, false, Keys.F10));
            SetCommand(3, "Clear analysis", ClearAnalysis, new ShortcutKey(true, false, true, Keys.F9));
            SetCommand(4, "Open console", OpenConsole);
            SetCommand(5, "About", ShowAbout);
            //SetCommand(3, "Options", ShowOptions);
            _cmdFrmDBExplorerIdx = 2;
            OpenConsole(); // TODO remove
        }

        private void ReadTranslations()
        {
            // There doesn't seem to be plugin communication to retrieve selected localization language
            // Therefore read from Notepad++ 'nativeLang.xml' config
            // (although this is a silly way since we keep re-reading it for any changes on SCN_UPDATE_UI notification):
            // <NotepadPlus><Native-Langue name="English" filename="english.xml" version="8.4.6">
            var xd = new XmlDocument();
            xd.Load(_languageConfigPath);
            var selectedLocalizationFile = xd.SelectSingleNode("/NotepadPlus/Native-Langue/@filename").Value;
            var translationsConfigPath = Path.Combine(_nppDbPluginDir, selectedLocalizationFile.RemoveSuffix(".xml") + ".ini");
            if (!string.IsNullOrEmpty(_translationsConfigPath) &&
                _translationsConfigPath.Equals(translationsConfigPath))
                return;
            _translationsConfigPath = translationsConfigPath;
            Console.WriteLine($"Plugin translations: {_translationsConfigPath}");

            var dict = new Dictionary<ParserMessageType, string>
            {
                { ParserMessageType.EQUALS_ALL, 
                    "Probably =ANY was intended" },
                { ParserMessageType.NOT_EQUALS_ANY, 
                    "Probably <>ALL was intended" },
                { ParserMessageType.DOUBLE_QUOTES, 
                    "Use of double quotes either refers to identifier or string literal by mistake" },
                { ParserMessageType.SELECT_ALL_IN_SUB_QUERY,
                    "Reckless use of SELECT * in sub-query" },
                { ParserMessageType.MULTIPLE_COLUMNS_IN_SUB_QUERY, 
                    "Multiple columns selected in sub-query" },
                { ParserMessageType.SELECT_ALL_IN_UNION_STATEMENT, 
                    "Reckless use of SELECT * in UNION select statement" },
                { ParserMessageType.DISTINCT_KEYWORD_WITH_GROUP_BY_CLAUSE,
                    "Both DISTINCT keyword and GROUP BY clause used" },
                { ParserMessageType.AGGREGATE_FUNCTION_WITHOUT_GROUP_BY_CLAUSE,
                    "SELECT clause contains aggregate function, but no GROUP BY clause" },
                { ParserMessageType.SELECT_ALL_WITH_MULTIPLE_JOINS,
                    "Reckless use of SELECT * in a query with multiple joins" },
                { ParserMessageType.COUNT_FUNCTION_WITH_OUTER_JOIN,
                    "count(*) function used with outer join" },
                { ParserMessageType.INSERT_STATEMENT_WITHOUT_COLUMN_NAMES,
                    "No column names were given to INSERT statement" },
                { ParserMessageType.SELECT_ALL_IN_INSERT_STATEMENT,
                    "Reckless use of SELECT * in INSERT statement" },
                { ParserMessageType.ORDERING_BY_ORDINAL,
                    "Ordering term is literal value" },
                { ParserMessageType.HAVING_CLAUSE_WITHOUT_AGGREGATE_FUNCTION,
                    "HAVING clause does not contain aggregate function" },
                { ParserMessageType.AGGREGATE_FUNCTION_IN_GROUP_BY_CLAUSE,
                    "GROUP BY clause contains aggregate function" },
                { ParserMessageType.DUPLICATE_SELECTED_COLUMN_IN_SELECT_CLAUSE,
                    "Same column exists in result set more than once" },
                { ParserMessageType.AND_OR_MISSING_PARENTHESES_IN_WHERE_CLAUSE,
                    "WHERE/HAVING clause has AND and OR expressions without parentheses" },
                { ParserMessageType.AGGREGATE_FUNCTION_IN_WHERE_CLAUSE,
                    "WHERE clause has aggregate function" },
                { ParserMessageType.MISSING_COLUMN_ALIAS_IN_SELECT_CLAUSE,
                    "Missing column alias in SELECT clause" },
                { ParserMessageType.USE_COUNT_FUNCTION,
                    "Use count(*) instead of sum(1)" },
                { ParserMessageType.ORDER_BY_CLAUSE_IN_SUB_QUERY_WITHOUT_LIMIT,
                    "ORDER BY clause in sub-query without row limiting" },
                { ParserMessageType.MISSING_WILDCARDS_IN_LIKE_EXPRESSION,
                    "Missing wildcards in LIKE expression" },
                { ParserMessageType.COLUMN_LIKE_COLUMN,
                    "Column LIKE column" },
                { ParserMessageType.EQUALITY_WITH_NULL,
                    "NULL value used with equality operator" },
                { ParserMessageType.EQUALITY_WITH_TEXT_PATTERN,
                    "Right operand of equality operation is a string literal and contains wildcards" },
                { ParserMessageType.NOT_LOGICAL_OPERAND,
                    "Is not logical operand" },
                { ParserMessageType.USE_AVG_FUNCTION,
                    "Use 'avg' function instead of dividing sum with count" },
                { ParserMessageType.TOP_KEYWORD_WITHOUT_ORDER_BY_CLAUSE,
                    "If you do not include the ORDER BY clause with TOP predicate, the query will return an arbitrary set of records from the table that satisfy the WHERE clause" },
            };
            var dict1 = new Dictionary<ParserMessageType, string>
            {
                { ParserMessageType.WARNING_FORMAT, "Warning at {0}:{1} : {2}" },
            };
            ReadTranslations("Warnings", dict, ref _warningMessages);
            ReadTranslations("General", dict1, ref _generalTranslations);
        }

        private void ReadTranslations(string sectionName, 
            in Dictionary<ParserMessageType, string> inputDictionary, 
            ref Dictionary<ParserMessageType, string> outputDictionary)
        {
            var codePage = Win32.SendMessage(GetCurrentScintilla(), SciMsg.SCI_GETCODEPAGE, 0, 0).ToInt32();
            foreach (var entry in inputDictionary)
            {
                var key = Enum.GetName(typeof(ParserMessageType), entry.Key);
                Console.WriteLine($"Reading translation: [{sectionName}] {key}");
                var bufferSize = 256;
                var bytes = new byte[bufferSize];
                Win32.GetPrivateProfileString(sectionName, key, entry.Value, bytes, bufferSize, _translationsConfigPath);
                var text = Encoding.GetEncoding(codePage).GetString(bytes).TrimEnd('\0');
                outputDictionary[entry.Key] = text;
            }
        }

        private const UInt32 STD_OUTPUT_HANDLE = 0xFFFFFFF5; // -11
        private const int MY_CODE_PAGE = 437;

        private void OpenConsole()
        {
            Win32.AllocConsole();
            var safeFileHandle = new SafeFileHandle(Win32.GetStdHandle(STD_OUTPUT_HANDLE), true);
            var fileStream = new FileStream(safeFileHandle, FileAccess.Write);
            var standardOutput = new StreamWriter(fileStream, Encoding.GetEncoding(MY_CODE_PAGE)) { AutoFlush = true };
            Console.SetOut(standardOutput);
        }

        private void SetToolBarIcons()
        {

            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = _imgMan.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_ADDTOOLBARICON, _funcItems.Items[_cmdFrmDBExplorerIdx]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);

        }
        private void FinalizePlugin()
        {
            Marshal.FreeHGlobal(_ptrPluginName);
            try
            {
                Options.Instance.SaveToXml(_cfgPath);
                DBServerManager.Instance.SaveToXml(_dbConnsPath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("finalize plugin : "+ ex.Message);
            }
        }
        #endregion

        #region plugin commands
        
        private static CaretPosition GetCaretPosition()
        {
            return new CaretPosition
            {
                Line = Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTLINE, 0, 0).ToInt32() + 1,
                Column = Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTCOLUMN, 0, 0).ToInt32(),
                Offset = Win32.SendMessage(nppData._scintillaMainHandle, (uint)SciMsg.SCI_GETCURRENTPOS, 0, 0).ToInt32(), // is byte offset
            };
        }

        private static unsafe string GetScintillaText(IntPtr scintillaHnd, bool selectionOnly)
        {
            byte[] textBuffer;
            var codePage = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETCODEPAGE, 0, 0).ToInt32();
            if (selectionOnly)
            {
                var length = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETSELTEXT, 0, 0).ToInt32();

                textBuffer = new byte[length];
                fixed (byte* textPtr = textBuffer)
                {
                    Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETSELTEXT, 0, (IntPtr)textPtr);
                }
            }
            else
            {
                var length = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETTEXTLENGTH, 0, 0).ToInt32();

                textBuffer = new byte[length];
                fixed (byte* textPtr = textBuffer)
                {
                    Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETTEXT, (IntPtr)length, (IntPtr)textPtr);
                }
            }

            string text;
            try
            {
                text = Encoding.GetEncoding(codePage).GetString(textBuffer).TrimEnd('\0');
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ApplicationException("Invalid codepage: " + codePage);
            }
            catch (ArgumentException)
            {
                throw new ApplicationException("Unsupported codepage: " + codePage);
            }
            catch (NotSupportedException)
            {
                throw new ApplicationException("Unsupported codepage: " + codePage);
            }

            return text;
        }

        private void ShowTip(Position position)
        {
            var editor = GetCurrentEditor();
            var value = editor.IndicatorValueAt(20, Convert.ToInt32(position.Value));
            if (value > 0 && value - 1 < editorErrors.Count)
            {
                editor.CallTipShow(Convert.ToInt32(position.Value), editorErrors[value - 1]);
            }
        }

        private void CloseTip()
        {
            var editor = GetCurrentEditor();
            editor.CallTipCancel();
        }

        private void AnalyzeAndExecute(bool showFeedback, bool onlyAnalyze)
        {
            var bufID = GetCurrentBufferId();
            var result = SQLResultManager.Instance.GetSQLResult(bufID);
            if (result == null)
            {
                MessageBox.Show("No database connection is attached to the current document.\nPlease select \"Attach\" from database context menu.");
                return;
            }
            
            ShowSQLResult(result);
            var selectionOnly = true;
            var text = "";

            var editor = GetCurrentEditor();
            try
            {
                text = GetScintillaText(GetCurrentScintilla(), true);
                if (string.IsNullOrWhiteSpace(text))
                {
                    selectionOnly = false;
                    text = GetScintillaText(GetCurrentScintilla(), false);
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    result.SetError("No text to execute!");
                    return;
                }
            }
            catch (Exception ex)
            {
                result.SetError(ex.Message);
                return;
            }
            
            var textLength = editor.GetTextLength();
            var caretPosition = GetCaretPosition();
            if (caretPosition.Offset == textLength) return; // to fix crash, but should be avoided with code change somewhere

            var parserResult = result.Parse(text.Replace("\t", "    "), caretPosition);
            var commands = selectionOnly 
                ? parserResult.Commands.Select(c => c.Text).ToList()
                : parserResult.Commands.Skip(parserResult.EnclosingCommandIndex).Take(1).Select(c => c.Text).ToList();

            editor.IndicSetFore(21, new Colour(0xff00));
            editor.IndicSetStyle(21, IndicatorStyle.BOX);

            editor.SetIndicatorCurrent(21);
            editor.IndicatorClearRange(0, textLength);

            // green outline
            var baseLine = 0;
            if (selectionOnly)
            {
                var start = editor.GetSelectionStart();
                var stop = editor.GetSelectionEnd();
                editor.IndicatorFillRange(start, stop - start);
                baseLine = editor.LineFromPosition(start);
            }
            else
            {
                var command = parserResult.Commands[parserResult.EnclosingCommandIndex];
                var start = editor.FindColumn(command.StartLine - 1, command.StartColumn);
                var stop = editor.FindColumn(command.StopLine - 1, command.StartColumn + command.StopOffset - command.StartOffset + 1);
                editor.IndicatorFillRange(start, stop - start);
            }

            if (showFeedback)
            {
                editor.StyleSetFont(199, "Calibri");
                editor.StyleSetBack(199, new Colour(250, 250, 225));
                editor.StyleSetFore(199, new Colour(130, 110, 30));

                editor.AnnotationClearAll();
                foreach (var command in parserResult.Commands)
                {
                    var lineWarnings = new Dictionary<int, string>();
                    foreach (var warning in command.Warnings)
                    {
                        var warningText = _warningMessages[warning.Type].Replace("\\n", "\n");
                        warningText = string.Format(
                            _generalTranslations[ParserMessageType.WARNING_FORMAT],
                            baseLine + warning.StartLine,
                            warning.StartColumn + 1,
                            warningText);
                        var line = baseLine + warning.StopLine - 1;
                        if (lineWarnings.TryGetValue(line, out var existingWarningText))
                        {
                            warningText = $"{existingWarningText}\r\n{warningText}";
                        }

                        lineWarnings[line] = warningText;
                    }

                    foreach (var entry in lineWarnings)
                    {
                        editor.AnnotationSetStyle(entry.Key, 199);
                        editor.AnnotationSetText(entry.Key, entry.Value);
                    }
                }

                editor.AnnotationSetVisible(AnnotationVisible.BOXED);

                editor.SetMouseDwellTime(500);
                editor.IndicSetFore(20, new Colour(0xff));
                editor.IndicSetStyle(20, IndicatorStyle.SQUIGGLEPIXMAP);

                editor.SetIndicatorCurrent(20);
                editor.IndicatorClearRange(0, text.Length);
                editorErrors.Clear();

                var errors = parserResult.Errors;
                foreach (var error in errors)
                {
                    var length = error.StopOffset - error.StartOffset + 1;
                    if (length < 1) continue;

                    var errorText = $"{error.Text} (line {error.StartLine}, col {error.StartColumn + 1})";

                    editorErrors.Add(errorText);
                    editor.SetIndicatorValue(editorErrors.Count);
                    var startOffset = editor.FindColumn(error.StartLine - 1, error.StartColumn);
                    var stopOffset = editor.FindColumn(error.StartLine - 1, error.StartColumn + length);
                    editor.IndicatorFillRange(startOffset, stopOffset - startOffset);
                    // var suggestionText = string.Join(" ", parserResult.Suggestions.ToArray());
                    // editor.AutoCShow(length - 1, suggestionText);
                }
            }
            result.SetError("");
            if (!onlyAnalyze) result.Execute(commands);
        }

        private void Analyze()
        {
            AnalyzeAndExecute(true, true);
        }

        private void ClearAnalysis()
        {
            var editor = GetCurrentEditor();
            var textLength = editor.GetTextLength();
            editor.AnnotationClearAll();
            editor.SetIndicatorCurrent(20);
            editor.IndicatorClearRange(0, textLength);
            editorErrors.Clear();
        }

        private void Execute()
        {
            AnalyzeAndExecute(false, false);
        }

        private void ExecuteSQL(IntPtr bufferID, string query)
        {
            var result = SQLResultManager.Instance.GetSQLResult(bufferID);
            
            result.SetError("");
            result.Execute(new List<string> { query });
        }
        
        private void CloseCurrentSQLResult()
        {
            var bufID = GetCurrentBufferId();
            CloseSQLResult(bufID);
        }

        internal void CloseSQLResult(IntPtr BufferID)
        {
            var result = SQLResultManager.Instance.GetSQLResult(BufferID);
            if (result == null) return;
            SQLResultManager.Instance.Remove(BufferID);
            
            _preViewHeights.Remove(result.Handle.ToInt32());
            if (_currentCtr == result) _currentCtr = null;
            hSplitBar.Visible = false;
            ResetViewPos(result);
            Win32.DestroyWindow(result.Handle);
        }
        
        private void Disconnect(IDBConnect connection)
        {
            connection.Disconnect();
            CloseCurrentSQLResult();
            SQLResultManager.Instance.RemoveSQLResults(connection);
        }
        
        private void Unregister(IDBConnect connection)
        {
            DBServerManager.Instance.Unregister(connection);
            connection.Disconnect();
            CloseCurrentSQLResult();
            SQLResultManager.Instance.RemoveSQLResults(connection);
        }

        private void ToggleDBManager()
        {
            if (_frmDBExplorer == null)
            {
               
                _frmDBExplorer = new FrmDatabaseExplore();
                _frmDBExplorer.AddNotifyHandler(
                    // toggle menu item and toolbar button when docking dialog's close button click
                    (ref Message msg) =>
                    {
                        ScNotification nc = (ScNotification)Marshal.PtrToStructure(msg.LParam, typeof(ScNotification));
                        if (nc.Header.Code != (uint)DockMgrMsg.DMN_CLOSE) return;
                        Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[_cmdFrmDBExplorerIdx]._cmdID, 0);
                    });

                _frmDBExplorer.DisconnectHandler = Disconnect;
                _frmDBExplorer.UnregisterHandler = Unregister;

                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.Fuchsia;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(_imgMan, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData nppTbData = new NppTbData();
                nppTbData.hClient = _frmDBExplorer.Handle;
                nppTbData.pszName = _funcItems.Items[_cmdFrmDBExplorerIdx]._itemName;
                nppTbData.dlgID = _cmdFrmDBExplorerIdx;
                //default docking
                nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                nppTbData.hIconTab = (uint)tbIcon.Handle;

                nppTbData.pszModuleName = PluginName;
                IntPtr ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(nppTbData));
                Marshal.StructureToPtr(nppTbData, ptrNppTbData, false);

                Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_DMMREGASDCKDLG, 0, ptrNppTbData);
                //toogle both menu item and toolbar button
                Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[_cmdFrmDBExplorerIdx]._cmdID, 1);

            }
            else
            {
                var nppMsg = NppMsg.NPPM_DMMSHOW;
                var toggleStatus = 1;
                if (_frmDBExplorer.Visible)
                {
                    nppMsg = NppMsg.NPPM_DMMHIDE;
                    toggleStatus = 0;
                }
                Win32.SendMessage(nppData._nppHandle, (uint)nppMsg, 0, _frmDBExplorer.Handle);
                Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_SETMENUITEMCHECK, _funcItems.Items[_cmdFrmDBExplorerIdx]._cmdID, toggleStatus);
            }

        }

        private void ShowOptions()
        {
            var dlg = new frmOption();
            dlg.ShowDialog();
        }

        private void ShowAbout()
        {
            var dlg = new frmAbout();
            dlg.ShowDialog();
        }

        #endregion

        private Control _currentCtr = null;
        internal void UpdateCurrentSQLResult()
        {
            if (SQLResultManager.Instance.Count == 0) return;//don't execute follow when loading 
            var bufID = GetCurrentBufferId();
            if (bufID == IntPtr.Zero) return;
            var result = SQLResultManager.Instance.GetSQLResult(bufID);
            if (result == null)
            {
                if (_currentCtr != null) _currentCtr.Visible = false;
                hSplitBar.Visible = false;
                return;
            }
            if (_currentCtr != null && _currentCtr != result && _currentCtr.Visible ) _currentCtr.Visible = false;
            _currentCtr = result;
            SetResultPos(_currentCtr);   
        }

        private Control AddSQLResult(IntPtr bufID, IDBConnect connect, ISQLExecutor sqlExecutor)
        {
            var ctr = SQLResultManager.Instance.CreateSQLResult(bufID, connect, sqlExecutor);
            ctr.Height = _defaultSQLResultHeight;
            ctr.Visible = false;//prevent Flicker
            var ret = Win32.SetParent(ctr.Handle, nppData._nppHandle);
            if (ret == null || ret == IntPtr.Zero) MessageBox.Show("setparent fail");

            if(hSplitBar== null) hSplitBar = CreateSplitBar();

            return ctr;
        }

        bool isDrag = false;
        Control hSplitBar = null; 
        private Control CreateSplitBar()
        {
            var bar = new PictureBox() { Left = 0, Top = 300, Width = 400, Height = 6, Cursor = Cursors.SizeNS, Visible = false };
            Win32.SetParent(bar.Handle, nppData._nppHandle);
            bar.BringToFront();

            int preBarY = 0,
                preScinH = 0,
                preSqlH = 0;

            bar.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                isDrag = true;
                preBarY = e.Y;
                RECT recScin;
                IntPtr hndScin = GetCurrentScintilla();
                Win32.GetWindowRect(hndScin, out recScin);
                preScinH = recScin.Bottom - recScin.Top;
                preSqlH = _currentCtr.Height;

                bar.BackColor = SystemColors.ActiveBorder;
                bar.BringToFront();

            };
            bar.MouseMove += (s, e) =>
            {

                if (!isDrag) return;

                RECT recScin;
                IntPtr hndScin = GetCurrentScintilla();
                Win32.GetWindowRect(hndScin, out recScin);
                var y = bar.Top + (e.Y - preBarY);
                if (bar.PointToScreen(new Point(0, y)).Y > recScin.Top + 100)
                    bar.Top = y;
                bar.BringToFront();

            };
            bar.MouseUp += (s, e) =>
            {
                if (!isDrag) return;
                bar.BackColor = SystemColors.ButtonFace;
                bar.BringToFront();

                int key = _currentCtr.Handle.ToInt32();

                RECT recScin;
                IntPtr hndScin = GetCurrentScintilla();
                Win32.GetWindowRect(hndScin, out recScin);

                IntPtr parent = Win32.GetParent(hndScin); //actually parent is nppData._scintillaMainHandle
                Point pRecScin = new Point(recScin.Left, recScin.Top);
                Win32.ScreenToClient(parent, ref pRecScin);

                int viewH = bar.Top - pRecScin.Y;
                int sqlH = preSqlH + (preScinH - viewH);
                int width = recScin.Right - recScin.Left;
                Win32.SetWindowPos(
                    hndScin, 
                    IntPtr.Zero, 
                    pRecScin.X, pRecScin.Y, 
                    width, viewH, 
                    Win32.SetWindowPosFlags.NoZOrder | Win32.SetWindowPosFlags.ShowWindow);
                Win32.SetWindowPos(
                    _currentCtr.Handle, 
                    IntPtr.Zero, 
                    pRecScin.X, bar.Top + bar.Height, 
                    width, sqlH, 
                    Win32.SetWindowPosFlags.NoZOrder | Win32.SetWindowPosFlags.ShowWindow);
                _preViewHeights[key] = viewH;

                isDrag = false;

            };
            return bar;
        }

        private void ShowSQLResult(SQLResult control)
        {
            _currentCtr = control;
            if (!control.Visible)
            {
                SetResultPos(control);
            }
            
            if (!control.LinkedDBConnect.IsOpened)
            {
                control.SetError("this database connection closed. open a database connection again.");
            }
        }

        private void NewFile()
        {
            Win32.SendMessage(nppData._nppHandle, (uint)Win32.WM.Command, (int)NppMenuCmd.IDM_FILE_NEW, 0);
        }

        private Dictionary<int, int> _preViewHeights = new Dictionary<int, int>();
        private const int _defaultSQLResultHeight = 200;
        private void SetResultPos(Control control)
        {
            if (isDrag) return;

            int key = control.Handle.ToInt32();
            int preViewH = _preViewHeights.ContainsKey(key) ? _preViewHeights[key] : -1;
            int sqlH = control.Height;

            RECT recScin;
            try
            {
                IntPtr hndScin = GetCurrentScintilla();
                Win32.GetWindowRect(hndScin, out recScin);

                int viewH = recScin.Bottom - recScin.Top;
                if (viewH != preViewH) viewH -= sqlH + hSplitBar.Height;

                IntPtr parent = Win32.GetParent(hndScin); //actually parent is nppData._scintillaMainHandle
                Point p = new Point(recScin.Left, recScin.Top);
                Win32.ScreenToClient(parent, ref p);
                
                Win32.SetWindowPos(hndScin, IntPtr.Zero, p.X, p.Y, recScin.Right - recScin.Left, viewH , Win32.SetWindowPosFlags.NoZOrder | Win32.SetWindowPosFlags.ShowWindow);
                /*
                hSplitBar.Left = p.X; hSplitBar.Top = p.Y + viewH;hSplitBar.Width = recScin.Right - recScin.Left;
                hSplitBar.Visible = true;
                hSplitBar.BringToFront();
                 * */ 
                Win32.SetWindowPos(
                    hSplitBar.Handle, 
                    IntPtr.Zero, 
                    p.X, p.Y + viewH, 
                    recScin.Right - recScin.Left, hSplitBar.Height, 
                    Win32.SetWindowPosFlags.ShowWindow);
                Win32.SetWindowPos(
                    control.Handle, 
                    IntPtr.Zero, p.X, p.Y + viewH + hSplitBar.Height, 
                    recScin.Right - recScin.Left, sqlH, 
                    Win32.SetWindowPosFlags.NoZOrder | Win32.SetWindowPosFlags.ShowWindow);
                control.Visible = true;

                _preViewHeights[key] = viewH;
            }
            catch (Exception ex)
            {
                MessageBox.Show("setpos : " + ex.Message);
            }

        }

        private void ResetViewPos(Control control)
        {

            int sqlH = control.Height;

            RECT recScin;
            try
            {
                IntPtr hndScin = GetCurrentScintilla();
                Win32.GetWindowRect(hndScin, out recScin);

                IntPtr parent = Win32.GetParent(hndScin); //actually parent is nppData._scintillaMainHandle
                Point p = new Point(recScin.Left, recScin.Top);
                Win32.ScreenToClient(parent, ref p);

                Win32.SetWindowPos(
                    hndScin, 
                    IntPtr.Zero, 
                    p.X, p.Y, 
                    recScin.Right - recScin.Left, recScin.Bottom - recScin.Top + sqlH, 
                    Win32.SetWindowPosFlags.NoZOrder | Win32.SetWindowPosFlags.ShowWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("setpos : " + ex.Message);
            }

        }
        
        private static IntPtr? GetCurrentAttachedBufferId()
        {
            var bufferId = GetCurrentBufferId();
            var result = SQLResultManager.Instance.GetSQLResult(bufferId);
            return result == null ? null : bufferId as IntPtr?;
        }

        private static void ActivateBufferId(int bufferId)
        {
            Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_RELOADBUFFERID, bufferId, 0);
        }

        private static IntPtr GetCurrentBufferId()
        {
            return Win32.SendMessage(nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTBUFFERID, 0, 0);
        }
        
        private void AppendToScintillaText(IntPtr scintillaHnd, string text)
        {
            int codePage = Win32.SendMessage(scintillaHnd, SciMsg.SCI_GETCODEPAGE, 0, 0).ToInt32();
            var bytes = Encoding.GetEncoding(codePage).GetBytes(text);
            IntPtr ptrChars = Marshal.AllocHGlobal(bytes.Length);


            try
            {
                Marshal.Copy(bytes, 0, ptrChars, bytes.Length);
                Win32.SendMessage(scintillaHnd, SciMsg.SCI_APPENDTEXT, bytes.Length , ptrChars);
                //todo selection, scroll
            }
            catch (Exception ex)
            {
                throw new ApplicationException("SCI_APPENDTEXT", ex);
            }
            finally
            {
                Marshal.FreeHGlobal(ptrChars);
            }
        }

        public object Execute(NppDBCommandType type, object[] parameters)
        {
            try
            {
                switch (type)
                {
                    case NppDBCommandType.ActivateBuffer://id
                        ActivateBufferId((int)parameters[0]);
                        break;
                    case NppDBCommandType.AppendToCurrentView:// text
                        AppendToScintillaText(GetCurrentScintilla(), (string)parameters[0]);
                        break;
                    case NppDBCommandType.NewFile://null
                        NewFile();
                        break;
                    case NppDBCommandType.CreateResultView://id, IDBConnect 
                        return AddSQLResult((IntPtr)parameters[0], (IDBConnect)parameters[1], (ISQLExecutor)parameters[2]);
                    case NppDBCommandType.DestroyResultView:
                        CloseCurrentSQLResult();
                        break;
                    case NppDBCommandType.ExecuteSQL://id, text 
                        ExecuteSQL((IntPtr)parameters[0], (string)parameters[1]);
                        break;
                    case NppDBCommandType.GetAttachedBufferID:
                        return GetCurrentAttachedBufferId();
                    case NppDBCommandType.GetActivatedBufferID:
                        return GetCurrentBufferId();
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return null;
        }

    }
}
