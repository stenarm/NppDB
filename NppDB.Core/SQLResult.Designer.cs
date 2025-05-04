using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NppDB.Core.Properties;

namespace NppDB.Core
{
    partial class SqlResult
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private IContainer components = null;

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(SqlResult));
            this.tspMain = new ToolStrip();
            this.btnStop = new ToolStripButton();
            this.sep0 = new ToolStripSeparator();
            this.lblElapsed = new ToolStripLabel();
            this.toolStripSeparator3 = new ToolStripSeparator();
            this.lblDatabase = new ToolStripLabel();
            this.sepDatabase = new ToolStripSeparator();
            this.lblAccount = new ToolStripLabel();
            this.sepAccount = new ToolStripSeparator();
            this.lblConnect = new ToolStripLabel();
            this.toolStripButton1 = new ToolStripButton();
            this.tclSqlResult = new TabControl();
            this.tabMsg = new TabPage();
            this.btnCloseAllResultWindows = new RichTextBox();
            this.lblError = new Label();
            this.tspMain.SuspendLayout();
            this.tclSqlResult.SuspendLayout();
            this.tabMsg.SuspendLayout();
            this.SuspendLayout();
            // 
            // tspMain
            // 
            this.tspMain.GripStyle = ToolStripGripStyle.Hidden;
            this.tspMain.Items.AddRange(new ToolStripItem[] { this.btnStop, this.sep0, this.lblElapsed, this.toolStripSeparator3, this.lblDatabase, this.sepDatabase, this.lblAccount, this.sepAccount, this.lblConnect, this.toolStripButton1 });
            this.tspMain.Location = new Point(0, 0);
            this.tspMain.Name = "tspMain";
            this.tspMain.Padding = new Padding(0);
            this.tspMain.Size = new Size(477, 25);
            this.tspMain.TabIndex = 1;
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.btnStop.Image = ((Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = Color.Magenta;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new Size(23, 22);
            this.btnStop.Text = "Stop";
            // 
            // sep0
            // 
            this.sep0.Name = "sep0";
            this.sep0.Size = new Size(6, 25);
            // 
            // lblElapsed
            // 
            this.lblElapsed.Alignment = ToolStripItemAlignment.Right;
            this.lblElapsed.Name = "lblElapsed";
            this.lblElapsed.Size = new Size(16, 22);
            this.lblElapsed.Text = "   ";
            this.lblElapsed.ToolTipText = "elapsed time";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Alignment = ToolStripItemAlignment.Right;
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new Size(6, 25);
            // 
            // lblDatabase
            // 
            this.lblDatabase.Alignment = ToolStripItemAlignment.Right;
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new Size(16, 22);
            this.lblDatabase.Text = "   ";
            // 
            // sepDatabase
            // 
            this.sepDatabase.Alignment = ToolStripItemAlignment.Right;
            this.sepDatabase.Name = "sepDatabase";
            this.sepDatabase.Size = new Size(6, 25);
            // 
            // lblAccount
            // 
            this.lblAccount.Alignment = ToolStripItemAlignment.Right;
            this.lblAccount.Name = "lblAccount";
            this.lblAccount.Size = new Size(16, 22);
            this.lblAccount.Text = "   ";
            // 
            // sepAccount
            // 
            this.sepAccount.Alignment = ToolStripItemAlignment.Right;
            this.sepAccount.Name = "sepAccount";
            this.sepAccount.Size = new Size(6, 25);
            // 
            // lblConnect
            // 
            this.lblConnect.Alignment = ToolStripItemAlignment.Right;
            this.lblConnect.Name = "lblConnect";
            this.lblConnect.Size = new Size(16, 22);
            this.lblConnect.Text = "   ";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = Resources.del16;
            this.toolStripButton1.ImageTransparentColor = Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new Size(23, 22);
            this.toolStripButton1.Text = "Close all result tabs";
            this.toolStripButton1.Click += new EventHandler(this.btnCloseAllResultWindows_Click);
            // 
            // tclSqlResult
            // 
            this.tclSqlResult.Controls.Add(this.tabMsg);
            this.tclSqlResult.Dock = DockStyle.Fill;
            this.tclSqlResult.Location = new Point(0, 25);
            this.tclSqlResult.Margin = new Padding(0);
            this.tclSqlResult.Multiline = true;
            this.tclSqlResult.Name = "tclSqlResult";
            this.tclSqlResult.Padding = new Point(0, 0);
            this.tclSqlResult.SelectedIndex = 0;
            this.tclSqlResult.ShowToolTips = true;
            this.tclSqlResult.Size = new Size(477, 399);
            this.tclSqlResult.TabIndex = 0;
            // 
            // tabMsg
            // 
            this.tabMsg.Controls.Add(this.btnCloseAllResultWindows);
            this.tabMsg.Location = new Point(4, 22);
            this.tabMsg.Margin = new Padding(0);
            this.tabMsg.Name = "tabMsg";
            this.tabMsg.Size = new Size(469, 373);
            this.tabMsg.TabIndex = 0;
            this.tabMsg.Text = "Messages";
            this.tabMsg.UseVisualStyleBackColor = true;
            // 
            // btnCloseAllResultWindows
            // 
            this.btnCloseAllResultWindows.BackColor = Color.Ivory;
            this.btnCloseAllResultWindows.BorderStyle = BorderStyle.None;
            this.btnCloseAllResultWindows.Dock = DockStyle.Fill;
            this.btnCloseAllResultWindows.HideSelection = false;
            this.btnCloseAllResultWindows.Location = new Point(0, 0);
            this.btnCloseAllResultWindows.Name = "btnCloseAllResultWindows";
            this.btnCloseAllResultWindows.ReadOnly = true;
            this.btnCloseAllResultWindows.Size = new Size(469, 373);
            this.btnCloseAllResultWindows.TabIndex = 0;
            this.btnCloseAllResultWindows.Text = "";
            // 
            // lblError
            // 
            this.lblError.BackColor = Color.Transparent;
            this.lblError.Dock = DockStyle.Fill;
            this.lblError.Font = new Font("Malgun Gothic", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));
            this.lblError.ForeColor = Color.Brown;
            this.lblError.Location = new Point(0, 0);
            this.lblError.Margin = new Padding(0);
            this.lblError.Name = "lblError";
            this.lblError.Padding = new Padding(26, 32, 26, 32);
            this.lblError.Size = new Size(477, 424);
            this.lblError.TabIndex = 3;
            this.lblError.Text = "fdasf";
            this.lblError.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SqlResult
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this.tclSqlResult);
            this.Controls.Add(this.tspMain);
            this.Controls.Add(this.lblError);
            this.Margin = new Padding(0);
            this.Name = "SqlResult";
            this.Size = new Size(477, 424);
            this.tspMain.ResumeLayout(false);
            this.tspMain.PerformLayout();
            this.tclSqlResult.ResumeLayout(false);
            this.tabMsg.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private ToolStripLabel lblDatabase;
        private ToolStripSeparator sepDatabase;

        private ToolStripButton toolStripButton1;

        #endregion

        private ToolStrip tspMain;
        private ToolStripButton btnStop;
        private ToolStripSeparator sep0;
        private ToolStripLabel lblElapsed;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripLabel lblAccount;
        private ToolStripSeparator sepAccount;
        private ToolStripLabel lblConnect;
        private TabControl tclSqlResult;
        private TabPage tabMsg;
        private RichTextBox btnCloseAllResultWindows;
        private Label lblError;

        private DataGridViewCellStyle dataGridViewCellStyle1;
        private DataGridViewCellStyle dataGridViewCellStyle2;
        private DataGridViewCellStyle dataGridViewCellStyle3;
    }
}
