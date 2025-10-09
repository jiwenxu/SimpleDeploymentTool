
using System;
using System.Windows.Forms;

namespace SimpleDeploymentTool {
    partial class FileManagerForm {
        private System.ComponentModel.IContainer components = null;
        private ComboBox cmbDirectories;
        private ListBox lstFiles;
        private Label lblDirectory;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.cmbDirectories = new ComboBox();
            this.lstFiles = new ListBox();
            this.lblDirectory = new Label();
            this.SuspendLayout();

            // lblDirectory
            this.lblDirectory.AutoSize = true;
            this.lblDirectory.Location = new System.Drawing.Point(12, 15);
            this.lblDirectory.Name = "lblDirectory";
            this.lblDirectory.Size = new System.Drawing.Size(44, 12);
            this.lblDirectory.TabIndex = 0;
            this.lblDirectory.Text = "目录：";

            // cmbDirectories
            this.cmbDirectories.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.cmbDirectories.FormattingEnabled = true;
            this.cmbDirectories.Location = new System.Drawing.Point(62, 12);
            this.cmbDirectories.Name = "cmbDirectories";
            this.cmbDirectories.Size = new System.Drawing.Size(410, 20);
            this.cmbDirectories.TabIndex = 1;
            this.cmbDirectories.SelectedIndexChanged += new EventHandler(this.cmbDirectories_SelectedIndexChanged);
            this.cmbDirectories.KeyDown += new KeyEventHandler(this.cmbDirectories_KeyDown);

            // lstFiles
            this.lstFiles.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.ItemHeight = 12;
            this.lstFiles.Location = new System.Drawing.Point(12, 38);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(460, 400);
            this.lstFiles.TabIndex = 2;

            // FileManagerForm
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.cmbDirectories);
            this.Controls.Add(this.lblDirectory);
            this.Name = "FileManagerForm";
            this.Text = "远程文件管理";
            this.Load += new EventHandler(this.FileManagerForm_Load);
            this.FormClosing += new FormClosingEventHandler(this.FileManagerForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}