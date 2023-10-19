using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.Core
{
    public partial class SQLResult : UserControl
    {
        public SQLResult(IDBConnect connect,ISQLExecutor sqlExecutor)
        {
            InitializeComponent();
            Init();
            SetConnect(connect, sqlExecutor);
        }
        
        private int _selectedTabIndex = 0;
        private int _tabCounter = 1;

        private void Init()
        {
            this.btnStop.Click += (s, e) =>
            {
                btnStop.Enabled = false;
                try
                {
                    _exec.Stop();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                finally
                {
                    btnStop.Enabled = _exec.CanStop();
                }
            };

            tclSqlResult.SizeMode = TabSizeMode.Fixed;
            tclSqlResult.ItemSize = new Size(185, 20);
            tclSqlResult.DrawMode = TabDrawMode.OwnerDrawFixed;

            tclSqlResult.DrawItem += (s, e) =>
            {
                var tp = tclSqlResult.TabPages[e.Index];
                var tr = tclSqlResult.GetTabRect(e.Index);
                tr.Inflate(-2, -2);
                if (e.Index > 0)
                {
                    e.Graphics.DrawString("x", e.Font, Brushes.Black, tr.Right - 10, tr.Top + 2);
                }
                e.Graphics.DrawString(tp.Text, e.Font, Brushes.Black, tr.Left + 4, tr.Top + 2);
                // e.DrawFocusRectangle();
            };
            tclSqlResult.MouseUp += (s, e) =>
            {
                for (var i = 1; i < tclSqlResult.TabPages.Count; ++i)
                {
                    var tr = tclSqlResult.GetTabRect(i);
                    tr.Inflate(-2, -2);

                    var closeButton = new Rectangle(tr.Right - 10, tr.Top + 2, 18, 18);
                    if (!closeButton.Contains(e.Location)) continue;

                    tclSqlResult.TabPages.RemoveAt(i);
                    var nextIndex = Math.Min(
                        i >= _selectedTabIndex
                            ? _selectedTabIndex
                            : _selectedTabIndex - 1
                        , tclSqlResult.TabPages.Count - 1);
                    tclSqlResult.SelectTab(nextIndex);
                    Console.WriteLine($"Selected: {_selectedTabIndex}, Closed: {i}, Select: {nextIndex}");
                    break;
                }
                _selectedTabIndex = tclSqlResult.SelectedIndex;
            };
            tclSqlResult.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                
                var tr = tclSqlResult.GetTabRect(0);
                if (!tr.Contains(e.Location)) return;

                var menu = new ContextMenu();
                menu.MenuItems.Add("Clear messages", (ss, ee) =>
                {
                    txtMsg.Clear();
                });
                menu.MenuItems.Add("Close result tabs", (ss, ee) =>
                {
                    tclSqlResult.SelectTab(0);
                    for (var i = tclSqlResult.TabPages.Count - 1; i > 0; --i)
                        tclSqlResult.TabPages.RemoveAt(i);
                });
                menu.Show(tclSqlResult, e.Location);
            };
        }
        private void Numbering(DataGridView dgv)
        {
            int idx = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = idx++.ToString();
            }
        }
        private void AdjustResizeColumnRow(DataGridView dgv)
        {
            //dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            dgv.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        public IDBConnect LinkedDBConnect { get; private set; }

        private ISQLExecutor _exec = null;
        private void SetConnect(IDBConnect connect, ISQLExecutor sqlExecutor)
        {
            if (_exec == null)
            {
                _exec = sqlExecutor;
            }
            LinkedDBConnect = connect;
            lblConnect.Text = connect.Title;
            lblAccount.Text = connect.Account;
            lblElapsed.Text = "";
            btnStop.Enabled = false;
        }

        public void SetError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                lblError.Visible = false;
                tspMain.Visible = true;
                tclSqlResult.Visible = true;
            }
            else
            {
                lblError.Visible = true;
                tspMain.Visible = false;
                tclSqlResult.Visible = false;
            }
            lblError.Text = message;
        }

        public ParserResult Parse(string sql, CaretPosition caretPosition)
        {
            return _exec.Parse(sql, caretPosition);
        }

        public void AddResultTabPage(int index, object dataSource, string titleText, string toolTipText)
        {
            var tp = new TabPage();
            var dgv = new DataGridView();

            tp.SuspendLayout();
            ((ISupportInitialize)dgv).BeginInit();
            tclSqlResult.TabPages.Add(tp);

            tp.Controls.Add(dgv);
            tp.Location = new System.Drawing.Point(4, 22);
            tp.Margin = new System.Windows.Forms.Padding(0);
            tp.Name = $"tabResult{index}";
            tp.Size = new System.Drawing.Size(469, 373);
            tp.TabIndex = 1;
            tp.Text = titleText;
            tp.ToolTipText = toolTipText;
            tp.UseVisualStyleBackColor = true;

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSize = false;
            dgv.BackgroundColor = System.Drawing.Color.White;
            dgv.BorderStyle = System.Windows.Forms.BorderStyle.None;

            dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(3);
            dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            dgv.DataSource = dataSource;
            dgv.DefaultCellStyle = dataGridViewCellStyle2;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            dgv.GridColor = System.Drawing.SystemColors.Control;
            dgv.Location = new System.Drawing.Point(0, 0);
            dgv.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            dgv.Name = $"grdResult{index}";
            dgv.ReadOnly = true;

            dgv.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.RowHeadersWidth = 10;
            dgv.RowTemplate.Height = 23;
            dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgv.ShowEditingIcon = false;
            dgv.ShowRowErrors = false;
            dgv.Size = new System.Drawing.Size(469, 373);
            dgv.TabIndex = 0;
            dgv.VirtualMode = true;

            dgv.Sorted += (s, e) => { Numbering(dgv); };

            ((ISupportInitialize)dgv).EndInit();
            tp.ResumeLayout(false);

            Numbering(dgv);
            AdjustResizeColumnRow(dgv);
        }

        public void Execute(IList<string> sqlQueries)
        {
            if (!_exec.CanExecute())
            {
                MessageBox.Show("There are tasks that are in a transactional state or have not ended.");
                return;
            }

            btnStop.Enabled = true;

            var startPoint = DateTime.Now;

            _exec.Execute(sqlQueries, (results) =>
            {
                Invoke(new Action(delegate
                {
                    var elapsed = (DateTime.Now - startPoint).ToString("c");
                    lblElapsed.Text = elapsed;

                    btnStop.Enabled = _exec.CanStop();

                    try
                    {
                        _selectedTabIndex = 0;
                        if (tclSqlResult.TabPages.Count == 1) _tabCounter = 1;
                        foreach (var result in results)
                        {
                            if (result.Error != null)
                            {
                                var additionalMessage = result.CommandText == null ? "" : $"\r\nThis error occurred while executing statement:\r\n{result.CommandText}";
                                var message = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {result.Error.GetType().Name}: {result.Error.Message}{additionalMessage}\r\n\r\n";
                                txtMsg.SelectionFont = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(129)));
                                txtMsg.AppendText(message);
                                txtMsg.SelectionFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(129)));
                            }
                            else if (result.RecordsAffected > 0 && result.QueryResult.Columns.Count == 0)
                            {
                                var message = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {result.RecordsAffected} rows affected by statement:\r\n{result.CommandText}\r\n\r\n";
                                txtMsg.AppendText(message);
                            }
                            else if (result.QueryResult.Columns.Count == 0)
                            {
                                var message = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Statement executed successfully:\r\n{result.CommandText}\r\n\r\n";
                                txtMsg.AppendText(message);
                            }
                            else
                            {
                                var tabIndex = _tabCounter++;
                                var message = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {result.QueryResult.Rows.Count} rows returned into \"Result {tabIndex}\" by statement:\r\n{result.CommandText}\r\n\r\n";
                                var title = $"Result {tabIndex} ({DateTime.Now:dd/MM/yyyy HH:mm:ss})";
                                var tooltip = $"{result.QueryResult.Rows.Count} rows returned by statement:\r\n{result.CommandText}";
                                txtMsg.AppendText(message);
                                AddResultTabPage(tabIndex, result.QueryResult, title, tooltip);
                                _selectedTabIndex = tclSqlResult.TabPages.Count - 1;
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        MessageBox.Show(ex1.Message);
                    }
                    
                    tclSqlResult.SelectTab(_selectedTabIndex);
                }));
            });
        }
    }
}
