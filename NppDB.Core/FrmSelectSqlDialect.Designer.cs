﻿using System.ComponentModel;

namespace NppDB.Core
{
    partial class FrmSelectSqlDialect
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
            this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss
            // 
            this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.FormattingEnabled = true;
            this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.Location = new System.Drawing.Point(52, 58);
            this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.Name = "cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss";
            this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.Size = new System.Drawing.Size(166, 21);
            this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.Location = new System.Drawing.Point(52, 105);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(69, 40);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(149, 105);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(69, 40);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FrmSelectSqlDialect
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(272, 194);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss);
            this.Name = "FrmSelectSqlDialect";
            this.Text = "Select SQL Dialect for Analysis:";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnCancel;

        private System.Windows.Forms.Button btnOK;

        private System.Windows.Forms.ComboBox cbxDbTypescbxDbTypescbxDbTypescbxDbTypessss;

        #endregion
    }
}