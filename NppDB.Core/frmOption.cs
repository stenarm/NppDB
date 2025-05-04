using System;
using System.Windows.Forms;

namespace NppDB.Core
{
    public partial class frmOption : Form
    {
        public frmOption()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            cbxUseTrans.Checked = (bool)Options.Instance["forcetrans"].Value;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Options.Instance["forcetrans"].Value = cbxUseTrans.Checked;
            DialogResult =  DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void frmOption_Load(object sender, EventArgs e)
        {
        }

    }
}
