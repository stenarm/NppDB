using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.Core
{
    public partial class SqlResult : UserControl
    {
        private static readonly List<WeakReference<SqlResult>> ActiveInstances = new List<WeakReference<SqlResult>>();
        private static readonly object ListLock = new object();

        public SqlResult(IDBConnect connect, ISQLExecutor sqlExecutor)
        {
            InitializeComponent();
            Init();
            SetConnect(connect, sqlExecutor);

            lock (ListLock)
            {
                ActiveInstances.RemoveAll(wr => !wr.TryGetTarget(out _));
                ActiveInstances.Add(new WeakReference<SqlResult>(this));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (ListLock)
                {
                    ActiveInstances.RemoveAll(wr => !wr.TryGetTarget(out var target) || target == this);
                }
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private static void CloseTabsInAllActiveInstances()
        {
            var liveInstances = new List<SqlResult>();
            lock(ListLock)
            {
                ActiveInstances.RemoveAll(wr =>
                {
                    if (!wr.TryGetTarget(out var target)) {
                        return true;
                    }
                    liveInstances.Add(target);
                    return false;
                });
            }

            if (liveInstances.Count == 0)
            {
                return;
            }

            foreach (var instance in liveInstances)
            {
                try
                {
                    instance.CloseAllResultTabs();
                }
                catch (Exception ex)
                {
                     MessageBox.Show($@"Error closing tabs: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void CloseAllResultTabs()
        {
            if (tclSqlResult == null || tclSqlResult.TabPages.Count <= 1) return;
            try
            {
                if (tclSqlResult.SelectedIndex != 0)
                {
                    tclSqlResult.SelectTab(0);
                }

                for (var i = tclSqlResult.TabPages.Count - 1; i > 0; i--)
                {
                    tclSqlResult.TabPages.RemoveAt(i);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred while closing result tabs in this view: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int _selectedTabIndex;
        private int _tabCounter = 1;

        private void Init()
        {
            btnStop.Click += (s, e) =>
            {
                btnStop.Enabled = false;
                try
                {
                    _exec.Stop();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    btnStop.Enabled = _exec.CanStop();
                }
            };

            tclSqlResult.SizeMode = TabSizeMode.Fixed;
            tclSqlResult.DrawMode = TabDrawMode.OwnerDrawFixed;

            int initialWidth;
            int initialHeight;
            using (var g = CreateGraphics())
            {
                 var tabFont = tclSqlResult.Font;
                 var initialText = tclSqlResult.TabPages.Count > 0 ? tclSqlResult.TabPages[0].Text : "Messages";
                 var textSize = TextRenderer.MeasureText(g, initialText, tabFont);
                 const int horizontalPadding = 4 + 4;
                 const int buttonSpace = 20;
                 const int minWidth = 50;
                 
                 initialWidth = Math.Max(textSize.Width + horizontalPadding + buttonSpace, minWidth);
                 initialHeight = textSize.Height + 4 + 4;
            }
            tclSqlResult.ItemSize = new Size(initialWidth, initialHeight); 

            tclSqlResult.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= tclSqlResult.TabCount) return;

                var tp = tclSqlResult.TabPages[e.Index];
                var g = e.Graphics;
                var tabRect = tclSqlResult.GetTabRect(e.Index);
                var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                var textFont = e.Font;

                using (new Font(e.Font, FontStyle.Bold))
                {
                    Color backColor;
                    Color textColor;
                    if (isSelected) {
                        backColor = SystemColors.Highlight;
                        textColor = SystemColors.HighlightText;
                    } else {
                        backColor = SystemColors.Control;
                        textColor = SystemColors.ControlText;
                    }

                    using (var backBrush = new SolidBrush(backColor)) {
                        g.FillRectangle(backBrush, tabRect);
                    }

                    var drawnIconWidth = 8;
                    var drawnIconHeight = 8;
                    Image buttonImage;
                    if (e.Index == 0) {
                        buttonImage = Properties.Resources.gui_eraser1;
                        drawnIconWidth = 16;
                        drawnIconHeight = 16;
                    } else {
                        buttonImage = Properties.Resources.x_letter1;
                    }


                    if (buttonImage != null)
                    {
                        var buttonY = tabRect.Top + (tabRect.Height - drawnIconHeight) / 2;
                        var buttonX = tabRect.Right - drawnIconWidth - 4;
                        var buttonRect = new Rectangle(buttonX, buttonY, drawnIconWidth, drawnIconHeight);

                        if (tabRect.Width > buttonRect.Width + 8)
                        {
                            g.DrawImage(buttonImage, buttonRect);
                        }
                    }

                    var textRect = new Rectangle(tabRect.Left + 4, tabRect.Top + 2, tabRect.Width - 8 - (buttonImage != null ? drawnIconWidth + 4 : 0), tabRect.Height - 4);

                    TextRenderer.DrawText(g, tp.Text, textFont, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                }
            };

            tclSqlResult.MouseUp += (s, e) =>
            {
                const int drawnIconWidth = 8;
                const int drawnIconHeight = 8;

                if (tclSqlResult.SelectedIndex == 0)
                {
                    var tr0 = tclSqlResult.GetTabRect(0);
                    var buttonY0 = tr0.Top + (tr0.Height - drawnIconHeight) / 2;
                    var clearButtonRect = new Rectangle(tr0.Right - drawnIconWidth - 4, buttonY0, drawnIconWidth, drawnIconHeight);
                    if (clearButtonRect.Contains(e.Location))
                    {
                        btnCloseAllResultWindows?.Clear();
                        return;
                    }
                }

                if (tclSqlResult.SelectedIndex > 0)
                {
                    var tr = tclSqlResult.GetTabRect(tclSqlResult.SelectedIndex);
                    var buttonY = tr.Top + (tr.Height - drawnIconHeight) / 2;
                    var closeButtonRect = new Rectangle(tr.Right - drawnIconWidth - 4, buttonY, drawnIconWidth, drawnIconHeight);
                    if (closeButtonRect.Contains(e.Location))
                    {
                        var indexToRemove = tclSqlResult.SelectedIndex;
                        if (indexToRemove <= 0) return;
                        tclSqlResult.TabPages.RemoveAt(indexToRemove);
                        var nextIndexToSelect = Math.Max(0, indexToRemove - 1);
                        if (tclSqlResult.TabPages.Count > nextIndexToSelect)
                        {
                            tclSqlResult.SelectedIndex = nextIndexToSelect;
                        }
                        _selectedTabIndex = tclSqlResult.SelectedIndex;
                        return;
                    }
                }

                if (tclSqlResult.SelectedIndex >= 0 && tclSqlResult.SelectedIndex != _selectedTabIndex)
                {
                    _selectedTabIndex = tclSqlResult.SelectedIndex;
                }
            };

            tclSqlResult.MouseClick += (s, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                var tr = tclSqlResult.GetTabRect(0);
                if (!tr.Contains(e.Location)) return;
                var menu = new ContextMenu();
                menu.MenuItems.Add("Clear messages", (ss, ee) =>
                {
                    btnCloseAllResultWindows?.Clear();
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
        
        private static void Numbering(DataGridView dgv)
        {
            var idx = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.HeaderCell.Value = idx++.ToString();
            }
        }
        private static void AdjustResizeColumnRow(DataGridView dgv)
        {
            dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            dgv.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        public IDBConnect LinkedDbConnect { get; private set; }

        private ISQLExecutor _exec;
        private void SetConnect(IDBConnect connect, ISQLExecutor sqlExecutor)
        {
            if (_exec == null)
            {
                _exec = sqlExecutor;
            }
            LinkedDbConnect = connect;
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

        private void AddResultTabPage(int index, DataTable dataSource, string titleText, string toolTipText)
        {
            int requiredWidth;
            using (var g = CreateGraphics())
            {
                var tabFont = tclSqlResult.Font;
                var textSize = TextRenderer.MeasureText(g, titleText, tabFont);
                const int horizontalPadding = 4 + 4;
                const int buttonSpace = 20;
                const int minWidth = 50;
                requiredWidth = Math.Max(textSize.Width + horizontalPadding + buttonSpace, minWidth);
            }

            var currentWidth = tclSqlResult.ItemSize.Width;
            if (requiredWidth > currentWidth)
            {
                tclSqlResult.ItemSize = new Size(requiredWidth, tclSqlResult.ItemSize.Height);
            }

            var tp = new TabPage();
            var dgv = new DataGridView();

            tp.SuspendLayout();
            ((ISupportInitialize)dgv).BeginInit();

            tp.Text = titleText;
            tp.ToolTipText = toolTipText;

            tclSqlResult.TabPages.Add(tp);

            tp.Controls.Add(dgv);
            tp.Location = new Point(4, 22);
            tp.Margin = new Padding(0);
            tp.Name = $"tabResult{index}";
            tp.UseVisualStyleBackColor = true;

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSize = false;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(3);
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.DataSource = dataSource;
             foreach (DataGridViewColumn col in dgv.Columns)
             {
                 if (dataSource.Columns.Contains(col.HeaderText))
                 {
                    col.HeaderText = dataSource.Columns[col.HeaderText].Caption;
                 }
             }
            dgv.DefaultCellStyle = dataGridViewCellStyle2;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.Dock = DockStyle.Fill;
            dgv.GridColor = SystemColors.Control;
            dgv.Location = new Point(0, 0);
            dgv.Margin = new Padding(0, 3, 0, 0);
            dgv.Name = $"grdResult{index}";
            dgv.ReadOnly = true;
            dgv.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgv.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.RowHeadersWidth = 43;
            dgv.RowTemplate.Height = 23;
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.ShowEditingIcon = false;
            dgv.ShowRowErrors = false;
            dgv.VirtualMode = true;

            dgv.Sorted += (s, e) => { Numbering(dgv); dgv.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders); };
            dgv.DataBindingComplete += (s, e) => { Numbering(dgv); dgv.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders); };

            ((ISupportInitialize)dgv).EndInit();
            tp.ResumeLayout(false);

            AdjustResizeColumnRow(dgv);
        }

        public void Execute(IList<string> sqlQueries)
        {
            if (!_exec.CanExecute())
            {
                MessageBox.Show(@"There are tasks that are in a transactional state or have not ended.");
                return;
            }

            btnStop.Enabled = true;

            var startPoint = DateTime.Now;

            _exec.Execute(sqlQueries, results =>
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
                                btnCloseAllResultWindows.SelectionFont = new Font("Microsoft Sans Serif", 8F, FontStyle.Bold, GraphicsUnit.Point, 129);
                                btnCloseAllResultWindows.AppendText(message);
                                btnCloseAllResultWindows.SelectionFont = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 129);
                            }
                            else if (result.RecordsAffected > 0 && result.QueryResult.Columns.Count == 0)
                            {
                                var message = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {result.RecordsAffected} rows affected by statement:\r\n{result.CommandText}\r\n\r\n";
                                btnCloseAllResultWindows.AppendText(message);
                            }
                            else if (result.QueryResult.Columns.Count == 0)
                            {
                                var message = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] Statement executed successfully:\r\n{result.CommandText}";
                                if (result.CommandMessage != null)
                                {
                                    message += $"\r\nStatement resulted in action: \r\n{result.CommandMessage}";
                                }
                                message += "\r\n\r\n";
                                btnCloseAllResultWindows.AppendText(message);
                            }
                            else
                            {
                                var tabIndex = _tabCounter++;
                                var message = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {result.QueryResult.Rows.Count} rows returned into \"Result {tabIndex}\" by statement:\r\n{result.CommandText}\r\n\r\n";
                                var title = $"{LinkedDbConnect?.Title ?? "DB"} {tabIndex} ({DateTime.Now:dd/MM/yyyy HH:mm:ss})";
                                var tooltip = $"{result.QueryResult.Rows.Count} rows returned by statement:\r\n{result.CommandText}";
                                btnCloseAllResultWindows.AppendText(message);
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

        private void btnCloseAllResultWindows_Click(object sender, EventArgs e)
        {
            try
            {
                CloseTabsInAllActiveInstances();
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred while trying to close all result tabs: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}