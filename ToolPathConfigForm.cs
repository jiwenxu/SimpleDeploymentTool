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
            txtPuTTYPath.Text = _toolSettings.PuTTYPath;
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

            if (string.IsNullOrEmpty(txtPuTTYPath.Text) || !System.IO.File.Exists(txtPuTTYPath.Text)) {
                MessageBox.Show("请指定有效的plink路径", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 保存配置
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtPuTTYPath = new System.Windows.Forms.TextBox();
            this.btnBrowsePuTTY = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 33);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "plink路径";
            // 
            // txtPuTTYPath
            // 
            this.txtPuTTYPath.Location = new System.Drawing.Point(15, 54);
            this.txtPuTTYPath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPuTTYPath.Name = "txtPuTTYPath";
            this.txtPuTTYPath.Size = new System.Drawing.Size(408, 23);
            this.txtPuTTYPath.TabIndex = 4;
            // 
            // btnBrowsePuTTY
            // 
            this.btnBrowsePuTTY.Location = new System.Drawing.Point(431, 51);
            this.btnBrowsePuTTY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnBrowsePuTTY.Name = "btnBrowsePuTTY";
            this.btnBrowsePuTTY.Size = new System.Drawing.Size(88, 33);
            this.btnBrowsePuTTY.TabIndex = 5;
            this.btnBrowsePuTTY.Text = "浏览...";
            this.btnBrowsePuTTY.UseVisualStyleBackColor = true;
            this.btnBrowsePuTTY.Click += new System.EventHandler(this.btnBrowsePuTTY_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(282, 124);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 33);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(435, 124);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // ToolPathConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 173);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnBrowsePuTTY);
            this.Controls.Add(this.txtPuTTYPath);
            this.Controls.Add(this.label2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ToolPathConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "工具路径配置";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Label label2;
        private TextBox txtPuTTYPath;
        private Button btnBrowsePuTTY;
        private Button btnSave;
        private Button btnCancel;
        #endregion
    }
}
