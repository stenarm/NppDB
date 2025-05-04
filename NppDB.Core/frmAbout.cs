using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace NppDB.Core
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/gutkyu/NppDB");
        }

        private void frmAbout_Load(object sender, EventArgs e)
        {
            lblVer.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
