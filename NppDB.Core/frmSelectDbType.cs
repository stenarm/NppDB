using System;
using System.Linq;
using System.Windows.Forms;

namespace NppDB.Core
{
    public partial class frmSelectDbType : Form
    {
        public frmSelectDbType()
        {
            InitializeComponent();
        }

        private void frmSelectDbType_Load(object sender, EventArgs e)
        {
            cbxDbTypes.Items.AddRange(DBServerManager.Instance.GetDatabaseTypes().ToArray());
            if(cbxDbTypes.Items.Count> 0) cbxDbTypes.SelectedIndex = 0;
        }

        public DatabaseType SelectedDatabaseType
        {
            get
            {
                if (DialogResult != DialogResult.OK) return null;
                return (DatabaseType)cbxDbTypes.SelectedItem;
            }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult =  DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult =  DialogResult.Cancel;
            Close();
        }

        private void frmSelectDbType_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
