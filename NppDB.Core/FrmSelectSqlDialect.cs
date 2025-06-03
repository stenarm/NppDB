using System;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.Core
{
    public partial class FrmSelectSqlDialect : Form
    {
        public SqlDialect SelectedDialect { get; private set; } = SqlDialect.NONE;

        public FrmSelectSqlDialect()
        {
            InitializeComponent();
            PopulateDialectDropdown();
        }

        /// <summary>
        /// Populates the ComboBox items and sets a default.
        /// </summary>
        private void PopulateDialectDropdown()
        {
            if (cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss == null) return;
            cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.Items.Clear();
            cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.Items.Add("PostgreSQL");
            cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.Items.Add("MS Access");

            if (cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.Items.Count > 0)
            {
                cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.SelectedIndex = 0;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            var selectedText = cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.SelectedItem?.ToString();

            switch (selectedText)
            {
                case "PostgreSQL":
                    SelectedDialect = SqlDialect.POSTGRE_SQL;
                    break;
                case "MS Access":
                    SelectedDialect = SqlDialect.MS_ACCESS;
                    break;
                default:
                    SelectedDialect = SqlDialect.NONE;
                    MessageBox.Show("Please select a SQL dialect from the list.", @"Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
            }

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
             SelectedDialect = SqlDialect.NONE;
        }
    }
}