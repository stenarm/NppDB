using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NppDB;
using NppDB.Comm;


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
            trvDBList.ImageList = new ImageList() { ColorDepth = ColorDepth.Depth32Bit };
            trvDBList.ImageList.Images.Add(NppDB.Core.Properties.Resources.bullet);
            trvDBList.ImageList.Images.Add("Group", NppDB.Core.Properties.Resources.Folder);
            trvDBList.ImageList.Images.Add("Database", NppDB.Core.Properties.Resources.Database);
            trvDBList.ImageList.Images.Add("Table", NppDB.Core.Properties.Resources.Table);

            trvDBList.ImageList.Images.Add("Primary_Key", NppDB.Core.Properties.Resources.primaryKey);
            trvDBList.ImageList.Images.Add("Foreign_Key", NppDB.Core.Properties.Resources.foreignKey);
            trvDBList.ImageList.Images.Add("Index", NppDB.Core.Properties.Resources.index);
            trvDBList.ImageList.Images.Add("Unique_Index", NppDB.Core.Properties.Resources.uniqueIndex);
            trvDBList.ImageList.Images.Add("Column_0000", NppDB.Core.Properties.Resources.column0000);
            trvDBList.ImageList.Images.Add("Column_0001", NppDB.Core.Properties.Resources.column0001);
            trvDBList.ImageList.Images.Add("Column_0010", NppDB.Core.Properties.Resources.column0010);
            trvDBList.ImageList.Images.Add("Column_0011", NppDB.Core.Properties.Resources.column0011);
            trvDBList.ImageList.Images.Add("Column_0100", NppDB.Core.Properties.Resources.column0100);
            trvDBList.ImageList.Images.Add("Column_0101", NppDB.Core.Properties.Resources.column0101);
            trvDBList.ImageList.Images.Add("Column_0110", NppDB.Core.Properties.Resources.column0110);
            trvDBList.ImageList.Images.Add("Column_0111", NppDB.Core.Properties.Resources.column0111);
            trvDBList.ImageList.Images.Add("Column_1000", NppDB.Core.Properties.Resources.column1000);
            trvDBList.ImageList.Images.Add("Column_1001", NppDB.Core.Properties.Resources.column1001);
            trvDBList.ImageList.Images.Add("Column_1010", NppDB.Core.Properties.Resources.column1010);
            trvDBList.ImageList.Images.Add("Column_1011", NppDB.Core.Properties.Resources.column1011);
            trvDBList.ImageList.Images.Add("Column_1100", NppDB.Core.Properties.Resources.column1100);
            trvDBList.ImageList.Images.Add("Column_1101", NppDB.Core.Properties.Resources.column1101);
            trvDBList.ImageList.Images.Add("Column_1110", NppDB.Core.Properties.Resources.column1110);
            trvDBList.ImageList.Images.Add("Column_1111", NppDB.Core.Properties.Resources.column1111);

            foreach (var dbcnn in DBServerManager.Instance.Connections)
            {
                var node = dbcnn as TreeNode;
                var id = DBServerManager.Instance.GetDatabaseTypes().First(x => x.ConnectType == dbcnn.GetType()).Id;
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
        }

        private List<NotifyHandler> _notifyHnds = new List<NotifyHandler>();
        public void AddNotifyHandler(NotifyHandler handler)
        {
            _notifyHnds.Add(handler);
        }

        protected override void WndProc(ref Message m)
        {
            if (_notifyHnds.Count > 0 && m.Msg == 0x4e)//WM_NOTIFY
                foreach (var hnd in _notifyHnds)
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
            var dlg = new frmSelectDbType();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            var selDbType = dlg.SelectedDatabaseType;
            var dbcnn = DBServerManager.Instance.CreateConnect(selDbType);
            if (!dbcnn.CheckLogin()) return;

            var tmpName = dbcnn.GetDefaultTitle();
            var maxVal = DBServerManager.Instance.Connections.Where(x => x.Title.StartsWith(tmpName)).Count();

            dbcnn.Title = tmpName + (maxVal == 0 ? "" : "(" + maxVal + ")");

            dbcnn.Connect();
            DBServerManager.Instance.Register(dbcnn);
            var id = selDbType.Id;
            var node = dbcnn as TreeNode;

            SetTreeNodeImage(node, id);

            trvDBList.Nodes.Add(node);

            if (trvDBList.TopNode != null && trvDBList.ItemHeight != trvDBList.TopNode.Bounds.Height + 4)
                trvDBList.ItemHeight = trvDBList.TopNode.Bounds.Height + 4;

            dbcnn.Attach();
            dbcnn.Refresh();

            ((TreeNode)dbcnn).Expand();
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
            if (!(trvDBList.SelectedNode is IDBConnect connector)) return;
            try
            {
                string result = connector.ConnectAndAttach();
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
            var connection = trvDBList.SelectedNode as IDBConnect;
            if (connection == null || trvDBList.SelectedNode.Level > 0) return;
            
            if (DisconnectHandler != null) DisconnectHandler(connection);
        }

        private void btnUnregister_Click(object sender, EventArgs e)
        {
            var connection = trvDBList.SelectedNode as IDBConnect;
            if (connection == null || trvDBList.SelectedNode.Level > 0) return;

            trvDBList.Nodes.Remove(trvDBList.SelectedNode);
            btnRegister.Enabled = true;
            btnUnregister.Enabled = false;
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = false;
            btnRefresh.Enabled = false;

            if (UnregisterHandler != null) UnregisterHandler(connection);
        }

        public delegate void DatabaseEventHandler(IDBConnect connection);

        public DatabaseEventHandler DisconnectHandler { get; set; }
        public DatabaseEventHandler UnregisterHandler { get; set; }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!(trvDBList.SelectedNode is IRefreshable refreshable)) return;
            refreshable.Refresh();
        }

        private void trvDBList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            btnUnregister.Enabled = e.Node is IDBConnect;
            var dbconn = GetRootParent(e.Node) as IDBConnect;
            btnConnect.Enabled = e.Node is IDBConnect && !dbconn.IsOpened;
            btnDisconnect.Enabled = e.Node is IDBConnect && dbconn.IsOpened;
            btnRefresh.Enabled = e.Node is IRefreshable && dbconn.IsOpened ;

        }

        private void trvDBList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                trvDBList.SelectedNode = e.Node;
                e.Node.ContextMenuStrip = CreateMenu(e.Node);
            }
        }

        private static TreeNode GetRootParent(TreeNode node)
        {
            while (node.Parent != null) node = node.Parent;
            return node;
        }

        private void trvDBList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            IRefreshable r = null;
            if (e.Button != MouseButtons.Left || (r = e.Node as IRefreshable) == null) return;
            var dbconn = GetRootParent(e.Node) as IDBConnect;
            if ( e.Node.Nodes.Count == 0)
            {
                string result = dbconn.ConnectAndAttach();
                if (result != "CONTINUE") { return;  }
                r.Refresh();
            }
            e.Node.Expand();
        }

        private ContextMenuStrip CreateMenu(TreeNode node)
        {
            Console.WriteLine("createmenu: " + node.Text);
            if (!(node is IMenuProvider menuCreator)) return null;
            var menu = menuCreator.GetMenu();
            if (!(node is IDBConnect connection)) return menu;
            
            menu.Items.Insert(0, new ToolStripButton("Unregister", null, btnUnregister_Click));
            menu.Items.Insert(1, new ToolStripSeparator());
            menu.Items.Insert(2, new ToolStripButton("Connect", null, btnConnect_Click) { Enabled = !connection.IsOpened });
            menu.Items.Insert(3, new ToolStripButton("Disconnect", null, btnDisconnect_Click) { Enabled = connection.IsOpened });
            menu.Items.Insert(4, new ToolStripSeparator());
            return menu;
        }
    }
}
