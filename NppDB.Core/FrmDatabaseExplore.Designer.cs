using System.ComponentModel;
using System.Windows.Forms;

namespace NppDB.Core
{
    partial class FrmDatabaseExplore
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRegister = new System.Windows.Forms.ToolStripButton();
            this.btnUnregister = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnConnect = new System.Windows.Forms.ToolStripButton();
            this.btnDisconnect = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.shortcuts = new System.Windows.Forms.ToolStripButton();
            this.trvDBList = new System.Windows.Forms.TreeView();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.btnRegister, this.btnUnregister, this.toolStripSeparator2, this.btnConnect, this.btnDisconnect, this.toolStripSeparator1, this.btnRefresh, this.toolStripSeparator3, this.shortcuts });
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(415, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnRegister
            // 
            this.btnRegister.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRegister.Image = global::NppDB.Core.Properties.Resources.add16;
            this.btnRegister.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(23, 22);
            this.btnRegister.Text = "Add a new database";
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnUnregister
            // 
            this.btnUnregister.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUnregister.Image = global::NppDB.Core.Properties.Resources.del16;
            this.btnUnregister.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUnregister.Name = "btnUnregister";
            this.btnUnregister.Size = new System.Drawing.Size(23, 22);
            this.btnUnregister.Text = "Remove the selected database connection";
            this.btnUnregister.Click += new System.EventHandler(this.btnUnregister_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnConnect
            // 
            this.btnConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnConnect.Image = global::NppDB.Core.Properties.Resources.connect16;
            this.btnConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(23, 22);
            this.btnConnect.Text = "Connect to the selected database";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDisconnect.Image = global::NppDB.Core.Properties.Resources.disconnect16;
            this.btnDisconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(23, 22);
            this.btnDisconnect.Text = "Disconnect from the selected database";
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRefresh
            // 
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefresh.Image = global::NppDB.Core.Properties.Resources.refresh16;
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(23, 22);
            this.btnRefresh.Text = "Refresh the selected database connection";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // shortcuts
            // 
            this.shortcuts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.shortcuts.Image = global::NppDB.Core.Properties.Resources.shortcuts6;
            this.shortcuts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.shortcuts.Name = "shortcuts";
            this.shortcuts.Size = new System.Drawing.Size(23, 22);
            this.shortcuts.Text = "Show plugin shortcuts";
            this.shortcuts.Click += new System.EventHandler(this.shortcuts_Click);
            // 
            // trvDBList
            // 
            this.trvDBList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvDBList.Location = new System.Drawing.Point(0, 25);
            this.trvDBList.Name = "trvDBList";
            this.trvDBList.PathSeparator = "!>!";
            this.trvDBList.Size = new System.Drawing.Size(415, 559);
            this.trvDBList.TabIndex = 1;
            this.trvDBList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvDBList_AfterSelect);
            this.trvDBList.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.trvDBList_NodeMouseClick);
            this.trvDBList.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.trvDBList_NodeMouseDoubleClick);
            // 
            // FrmDatabaseExplore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 584);
            this.Controls.Add(this.trvDBList);
            this.Controls.Add(this.toolStrip1);
            this.Name = "FrmDatabaseExplore";
            this.Text = "FrmDatabaseExplore";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;

        private System.Windows.Forms.ToolStripButton shortcuts;

        #endregion

        private ToolStrip toolStrip1;
        private ToolStripButton btnConnect;
        private ToolStripButton btnDisconnect;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnRefresh;
        private TreeView trvDBList;
        private ToolStripButton btnRegister;
        private ToolStripButton btnUnregister;
        private ToolStripSeparator toolStripSeparator2;
    }
}