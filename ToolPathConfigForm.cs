using System;
using System.Windows.Forms;
using SimpleDeploymentTool.Models;
using SimpleDeploymentTool.Services;

namespace SimpleDeploymentTool {
    public partial class ToolPathConfigForm : Form {
        private readonly FileDataService _dataService;
        private readonly ToolSettings _toolSettings;

        public ToolPathConfigForm() {
            InitializeComponent();
            _dataService = new FileDataService();
            _toolSettings = _dataService.LoadToolSettings();

            // 加载现有配置
            txtWinSCPPath.Text = _toolSettings.WinSCPPath;
            txtPuTTYPath.Text = _toolSettings.PuTTYPath;
        }

        private void btnBrowseWinSCP_Click(object sender, EventArgs e) {
            using (var openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "可执行文件|*.exe|所有文件|*.*";
                openFileDialog.Title = "选择WinSCP可执行文件";
                openFileDialog.FileName = "winscp.exe";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    txtWinSCPPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnBrowsePuTTY_Click(object sender, EventArgs e) {
            using (var openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "可执行文件|*.exe|所有文件|*.*";
                openFileDialog.Title = "选择plink可执行文件";
                openFileDialog.FileName = "plink.exe";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    txtPuTTYPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            // 验证路径
            if (string.IsNullOrEmpty(txtWinSCPPath.Text) || !System.IO.File.Exists(txtWinSCPPath.Text)) {
                MessageBox.Show("请指定有效的WinSCP路径", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtPuTTYPath.Text) || !System.IO.File.Exists(txtPuTTYPath.Text)) {
                MessageBox.Show("请指定有效的plink路径", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 保存配置
            _toolSettings.WinSCPPath = txtWinSCPPath.Text;
            _toolSettings.PuTTYPath = txtPuTTYPath.Text;
            _dataService.SaveToolSettings(_toolSettings);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #region Windows 窗体设计器生成的代码
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.txtWinSCPPath = new System.Windows.Forms.TextBox();
            this.btnBrowseWinSCP = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPuTTYPath = new System.Windows.Forms.TextBox();
            this.btnBrowsePuTTY = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "WinSCP路径";

            this.txtWinSCPPath.Location = new System.Drawing.Point(14, 30);
            this.txtWinSCPPath.Name = "txtWinSCPPath";
            this.txtWinSCPPath.Size = new System.Drawing.Size(350, 21);
            this.txtWinSCPPath.TabIndex = 1;

            this.btnBrowseWinSCP.Location = new System.Drawing.Point(370, 28);
            this.btnBrowseWinSCP.Name = "btnBrowseWinSCP";
            this.btnBrowseWinSCP.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseWinSCP.TabIndex = 2;
            this.btnBrowseWinSCP.Text = "浏览...";
            this.btnBrowseWinSCP.UseVisualStyleBackColor = true;
            this.btnBrowseWinSCP.Click += new System.EventHandler(this.btnBrowseWinSCP_Click);

            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "plink路径";

            this.txtPuTTYPath.Location = new System.Drawing.Point(14, 80);
            this.txtPuTTYPath.Name = "txtPuTTYPath";
            this.txtPuTTYPath.Size = new System.Drawing.Size(350, 21);
            this.txtPuTTYPath.TabIndex = 4;

            this.btnBrowsePuTTY.Location = new System.Drawing.Point(370, 78);
            this.btnBrowsePuTTY.Name = "btnBrowsePuTTY";
            this.btnBrowsePuTTY.Size = new System.Drawing.Size(75, 23);
            this.btnBrowsePuTTY.TabIndex = 5;
            this.btnBrowsePuTTY.Text = "浏览...";
            this.btnBrowsePuTTY.UseVisualStyleBackColor = true;
            this.btnBrowsePuTTY.Click += new System.EventHandler(this.btnBrowsePuTTY_Click);

            this.btnSave.Location = new System.Drawing.Point(239, 120);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Location = new System.Drawing.Point(370, 120);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 155);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnBrowsePuTTY);
            this.Controls.Add(this.txtPuTTYPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowseWinSCP);
            this.Controls.Add(this.txtWinSCPPath);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ToolPathConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "工具路径配置";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label label1;
        private TextBox txtWinSCPPath;
        private Button btnBrowseWinSCP;
        private Label label2;
        private TextBox txtPuTTYPath;
        private Button btnBrowsePuTTY;
        private Button btnSave;
        private Button btnCancel;
        #endregion
    }
}
