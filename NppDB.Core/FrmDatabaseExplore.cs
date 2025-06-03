using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NppDB.Comm;
using NppDB.Core.Properties;
using NppDB.PostgreSQL;

namespace NppDB.Core
{

    public partial class FrmDatabaseExplore : Form
    {
        public FrmDatabaseExplore()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            trvDBList.ImageList = new ImageList { ColorDepth = ColorDepth.Depth32Bit };
            trvDBList.ImageList.Images.Add(Resources.bullet);
            trvDBList.ImageList.Images.Add("Group", Resources.Folder);
            trvDBList.ImageList.Images.Add("Database", Resources.Database);
            trvDBList.ImageList.Images.Add("Table", Resources.Table);

            trvDBList.ImageList.Images.Add("Primary_Key", Resources.primaryKey);
            trvDBList.ImageList.Images.Add("Foreign_Key", Resources.foreignKey);
            trvDBList.ImageList.Images.Add("Index", Resources.index);
            trvDBList.ImageList.Images.Add("Unique_Index", Resources.uniqueIndex);
            trvDBList.ImageList.Images.Add("Column_0000", Resources.column0000);
            trvDBList.ImageList.Images.Add("Column_0001", Resources.column0001);
            trvDBList.ImageList.Images.Add("Column_0010", Resources.column0010);
            trvDBList.ImageList.Images.Add("Column_0011", Resources.column0011);
            trvDBList.ImageList.Images.Add("Column_0100", Resources.column0100);
            trvDBList.ImageList.Images.Add("Column_0101", Resources.column0101);
            trvDBList.ImageList.Images.Add("Column_0110", Resources.column0110);
            trvDBList.ImageList.Images.Add("Column_0111", Resources.column0111);
            trvDBList.ImageList.Images.Add("Column_1000", Resources.column1000);
            trvDBList.ImageList.Images.Add("Column_1001", Resources.column1001);
            trvDBList.ImageList.Images.Add("Column_1010", Resources.column1010);
            trvDBList.ImageList.Images.Add("Column_1011", Resources.column1011);
            trvDBList.ImageList.Images.Add("Column_1100", Resources.column1100);
            trvDBList.ImageList.Images.Add("Column_1101", Resources.column1101);
            trvDBList.ImageList.Images.Add("Column_1110", Resources.column1110);
            trvDBList.ImageList.Images.Add("Column_1111", Resources.column1111);

            foreach (var dbcnn in DbServerManager.Instance.Connections)
            {
                var node = dbcnn as TreeNode;
                var id = DbServerManager.Instance.GetDatabaseTypes().First(x => x.ConnectType == dbcnn.GetType()).Id;
                SetTreeNodeImage(node, id);
            
                trvDBList.Nodes.Add((TreeNode)dbcnn);
            }

            btnRegister.Enabled = true;
            btnUnregister.Enabled = false;
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = false;
            btnRefresh.Enabled = false;
            
            if (trvDBList.TopNode != null)
                trvDBList.ItemHeight = trvDBList.TopNode.Bounds.Height + 4;
            
            trvDBList.ShowNodeToolTips = true;
            trvDBList.BeforeExpand += trvDBList_BeforeExpand;
        }

        private readonly List<NotifyHandler> _notifyHandlers = new List<NotifyHandler>();
        public void AddNotifyHandler(NotifyHandler handler)
        {
            _notifyHandlers.Add(handler);
        }

        protected override void WndProc(ref Message m)
        {
            if (_notifyHandlers.Count > 0 && m.Msg == 0x4e)
                foreach (var hnd in _notifyHandlers)
                    hnd(ref m);
            
            base.WndProc(ref m);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                RegisterConnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + (ex.InnerException != null ? " : " + ex.InnerException.Message : ""));
            }
        }

        private void RegisterConnect()
        {
            var dlg = new FrmSelectDbType();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            var selDbType = dlg.SelectedDatabaseType;
            var dbConnection = DbServerManager.Instance.CreateConnect(selDbType);

            bool checkLoginResult;
            try
            {
                checkLoginResult = dbConnection.CheckLogin();
            }
            catch (Exception exCheckLogin)
            {
                MessageBox.Show($"RegisterConnect: UNEXPECTED ERROR during CheckLogin call: {exCheckLogin.Message}", @"Debug RegisterConnect Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkLoginResult = false;
            }

            if (!checkLoginResult)
            {
                return;
            }

            var tmpName = dbConnection.GetDefaultTitle();
            var existingCount = DbServerManager.Instance.Connections.Count(x => x.Title.StartsWith(tmpName));
            dbConnection.Title = tmpName + (existingCount == 0 ? "" : "(" + existingCount + ")");

            DbServerManager.Instance.Register(dbConnection);
            var id = selDbType.Id;
            var node = dbConnection as TreeNode;
            if (node != null)
            {
                SetTreeNodeImage(node, id);
                trvDBList.Nodes.Add(node);

                if (trvDBList.TopNode != null && trvDBList.ItemHeight != trvDBList.TopNode.Bounds.Height + 4)
                    trvDBList.ItemHeight = trvDBList.TopNode.Bounds.Height + 4;
            }

            try
            {
                dbConnection.Connect();
                dbConnection.Attach();
                dbConnection.Refresh();
                node?.Expand();
            }
            catch (Exception exConnect)
            {
                MessageBox.Show($"RegisterConnect: ERROR during Connect/Attach/Refresh: {exConnect.Message}" + (exConnect.InnerException != null ? " Inner: " + exConnect.InnerException.Message : ""), @"Debug RegisterConnect Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetTreeNodeImage(TreeNode node, string id)
        {
            var iconProvider = node as IIconProvider;
            if(!trvDBList.ImageList.Images.ContainsKey(id)) 
                trvDBList.ImageList.Images.Add(id, iconProvider.GetIcon());
            node.SelectedImageKey = node.ImageKey = id;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!(trvDBList.SelectedNode is IDbConnect connector)) return;
            try
            {
                var result = connector.ConnectAndAttach();
                if (result != "CONTINUE") { return; }
                trvDBList.SelectedNode.Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + (ex.InnerException != null ? " : " + ex.InnerException.Message: ""));
            }
        }
        
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (!(trvDBList.SelectedNode is IDbConnect connection) || trvDBList.SelectedNode.Level > 0) return;

            DisconnectHandler?.Invoke(connection);
            trvDBList.SelectedNode?.Nodes.Clear();
        }

        private void btnUnregister_Click(object sender, EventArgs e)
        {
            if (!(trvDBList.SelectedNode is IDbConnect connection) || trvDBList.SelectedNode.Level > 0) return;

            trvDBList.Nodes.Remove(trvDBList.SelectedNode);
            btnRegister.Enabled = true;
            btnUnregister.Enabled = false;
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = false;
            btnRefresh.Enabled = false;

            UnregisterHandler?.Invoke(connection);
        }
        
        private void trvDBList_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count != 1 || !string.IsNullOrEmpty(e.Node.Nodes[0].Text)) return;
            if (e.Node is IRefreshable refreshableNode)
            {
                Cursor = Cursors.WaitCursor;
                trvDBList.Cursor = Cursors.WaitCursor;
                try
                {
                    refreshableNode.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error expanding node '{e.Node.Text}':\n{ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
                finally
                {
                    Cursor = Cursors.Default;
                    trvDBList.Cursor = Cursors.Default;
                }
            }
            else
            {
                e.Node.Nodes.Clear();
            }
        }

        public delegate void DatabaseEventHandler(IDbConnect connection);

        public DatabaseEventHandler DisconnectHandler { get; set; }
        public DatabaseEventHandler UnregisterHandler { get; set; }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!(trvDBList.SelectedNode is IRefreshable refreshable)) return;
            refreshable.Refresh();
        }

        private void trvDBList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnUnregister.Enabled = e.Node is IDbConnect;
            if (!(GetRootParent(e.Node) is IDbConnect dbConnection)) return;
            btnConnect.Enabled = e.Node is IDbConnect && !dbConnection.IsOpened;
            btnDisconnect.Enabled = e.Node is IDbConnect && dbConnection.IsOpened;
            btnRefresh.Enabled = e.Node is IRefreshable && dbConnection.IsOpened;
        }

        private void trvDBList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            trvDBList.SelectedNode = e.Node;
            e.Node.ContextMenuStrip = CreateMenu(e.Node);
        }

        private static TreeNode GetRootParent(TreeNode node)
        {
            while (node.Parent != null) node = node.Parent;
            return node;
        }

        private void trvDBList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            IRefreshable r;
            if (e.Button != MouseButtons.Left || (r = e.Node as IRefreshable) == null) return;
            var dbConnection = GetRootParent(e.Node) as IDbConnect;
            if ( e.Node.Nodes.Count == 0)
            {
                if (dbConnection != null)
                {
                    var result = dbConnection.ConnectAndAttach();
                    if (result != "CONTINUE") { return;  }
                }

                r.Refresh();
            }
            e.Node.Expand();
        }

        private ContextMenuStrip CreateMenu(TreeNode node)
        {
            var menuCreator = node as IMenuProvider;
            var menu = menuCreator?.GetMenu() ?? new ContextMenuStrip { ShowImageMargin = false };

            var insertIndex = 0;

            if (node is PostgreSqlConnect)
            {
                menu.Items.Insert(insertIndex++, new ToolStripButton("Edit database connection details", null, EditConnection_Click));
                menu.Items.Insert(insertIndex++, new ToolStripSeparator());
            }

            if (node is IDbConnect connection)
            {
                menu.Items.Insert(insertIndex++, new ToolStripButton("Remove this database connection", null, btnUnregister_Click));
                menu.Items.Insert(insertIndex++, new ToolStripSeparator());
                menu.Items.Insert(insertIndex++, new ToolStripButton("Connect to this database", null, btnConnect_Click) { Enabled = !connection.IsOpened });
                menu.Items.Insert(insertIndex++, new ToolStripButton("Disconnect from this database", null, btnDisconnect_Click) { Enabled = connection.IsOpened });
                menu.Items.Insert(insertIndex++, new ToolStripSeparator());
            }

            if (menu.Items.Count <= 0) return menu;
            if (menu.Items[menu.Items.Count - 1] is ToolStripSeparator && (menuCreator?.GetMenu() == null || menuCreator.GetMenu().Items.Count == 0))
            {
                menu.Items.RemoveAt(menu.Items.Count - 1);
            }
            if (menu.Items.Count > 0 && menu.Items[0] is ToolStripSeparator && insertIndex == menu.Items.Count && menuCreator?.GetMenu() == null)
            {
                menu.Items.RemoveAt(0);
            }


            return menu;
        }
        
        private void EditConnection_Click(object sender, EventArgs e)
        {
            if (trvDBList.SelectedNode is PostgreSqlConnect pgConnection)
            {
                try
                {
                    var changesMade = pgConnection.CheckLogin();

                    if (!changesMade) return;
                    trvDBList.SelectedNode.Text = pgConnection.Title;

                    if (pgConnection.IsOpened)
                    {
                        MessageBox.Show(this, "Connection properties have been updated.\n\nPlease disconnect and reconnect for these changes\nto take effect on the live database connection.",
                            @"Properties Changed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error opening connection properties:\n{ex.Message}",
                                    @"Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(this, @"The selected item is not a PostgreSQL connection.", @"Cannot Edit Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void shortcuts_Click(object sender, EventArgs e)
        {
            var shortcutText = new StringBuilder();
            shortcutText.AppendLine("NppDB Shortcuts:");
            shortcutText.AppendLine("--------------------------------------------");
            shortcutText.AppendLine("Execute SQL:                                 F9");
            shortcutText.AppendLine("");
            shortcutText.AppendLine("Analyze SQL:                       Shift+F9");
            shortcutText.AppendLine("");
            shortcutText.AppendLine("Generate AI prompt:            Ctrl+F9");
            shortcutText.AppendLine("");
            shortcutText.AppendLine("Clear Analysis:           Ctrl+Shift+F9");
            shortcutText.AppendLine("");
            shortcutText.AppendLine("DB Connect Manager:               F10");

            MessageBox.Show(
                this, shortcutText.ToString(), @"NppDB Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnEditAiPromptTemplate_Click(object sender, EventArgs e)
        {
            IDbConnect selectedConnection = null;
            if (trvDBList.SelectedNode is IDbConnect connectNode)
            {
                selectedConnection = connectNode;
            }
            else if (trvDBList.SelectedNode != null && GetRootParent(trvDBList.SelectedNode) is IDbConnect rootConnectNode)
            {
                selectedConnection = rootConnectNode;
            }
            else if (DbServerManager.Instance.Connections.Any())
            {
                selectedConnection = DbServerManager.Instance.Connections.First();
            }

            if (selectedConnection?.CommandHost == null)
            {
                MessageBox.Show("Cannot determine plugin context to open the template file. Please ensure a database connection is registered.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var pluginDirObj = selectedConnection.CommandHost.Execute(NppDbCommandType.GET_PLUGIN_DIRECTORY, null);
            if (!(pluginDirObj is string pluginDirectoryPath) || string.IsNullOrEmpty(pluginDirectoryPath))
            {
                MessageBox.Show("Could not retrieve plugin directory path from the command host.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var templateFilePath = Path.Combine(pluginDirectoryPath, "AIPromptTemplate.txt");


            if (!File.Exists(templateFilePath))
            {
                MessageBox.Show($"AI prompt template file not found at:\n{templateFilePath}\n\nA new empty file will be created if you save.",
                                "Template Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            selectedConnection.CommandHost.Execute(NppDbCommandType.OPEN_FILE_IN_NPP, new object[] { templateFilePath });
        }
    }
}
