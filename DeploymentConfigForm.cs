using System;
using System.Windows.Forms;
using SimpleDeploymentTool.Models;

namespace SimpleDeploymentTool {
    public partial class DeploymentConfigForm : Form {
        public DeploymentConfig DeploymentConfig { get; private set; }

        public DeploymentConfigForm() {
            InitializeComponent();
            DeploymentConfig = new DeploymentConfig();
            rbtnPassword.Checked = true;
        }

        public DeploymentConfigForm(DeploymentConfig config) {
            InitializeComponent();
            DeploymentConfig = new DeploymentConfig();

            // 复制配置数据
            DeploymentConfig.Id = config.Id;
            DeploymentConfig.Alias = config.Alias;
            DeploymentConfig.ServiceProvider = config.ServiceProvider;
            DeploymentConfig.IpAddress = config.IpAddress;
            DeploymentConfig.Port = config.Port;
            DeploymentConfig.Username = config.Username;
            DeploymentConfig.Password = config.Password;
            DeploymentConfig.SshKeyPath = config.SshKeyPath;
            DeploymentConfig.RemoteSavePath = config.RemoteSavePath;
            DeploymentConfig.LocalFilePath = config.LocalFilePath;
            DeploymentConfig.RemoteBackupPath = config.RemoteBackupPath;
            DeploymentConfig.CreatedDate = config.CreatedDate;

            // 填充表单
            txtAlias.Text = config.Alias;
            txtServiceProvider.Text = config.ServiceProvider;
            txtIpAddress.Text = config.IpAddress;
            txtPort.Text = config.Port.ToString();
            txtUsername.Text = config.Username;
            txtPassword.Text = config.Password;
            txtSshKeyPath.Text = config.SshKeyPath;
            txtRemoteSavePath.Text = config.RemoteSavePath;
            txtLocalFilePath.Text = config.LocalFilePath;
            txtRemoteBackupPath.Text = config.RemoteBackupPath;

            // 选择认证方式
            rbtnPassword.Checked = string.IsNullOrEmpty(config.SshKeyPath);
            rbtnSshKey.Checked = !string.IsNullOrEmpty(config.SshKeyPath);

            UpdateControlStates();
        }

        private void UpdateControlStates() {
            txtPassword.Enabled = rbtnPassword.Checked;
            txtSshKeyPath.Enabled = rbtnSshKey.Checked;
            btnBrowseSshKey.Enabled = rbtnSshKey.Checked;
        }

        private void btnBrowseSshKey_Click(object sender, EventArgs e) {
            using (var openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "Private Key Files|*.ppk;*.pem|All Files|*.*";
                openFileDialog.Title = "选择SSH私钥文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    txtSshKeyPath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnBrowseLocalFile_Click(object sender, EventArgs e) {
            using (var openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "All Files|*.*";
                openFileDialog.Title = "选择要上传的本地文件";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    txtLocalFilePath.Text = openFileDialog.FileName;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e) {
            // 验证输入
            if (string.IsNullOrWhiteSpace(txtAlias.Text)) {
                MessageBox.Show("请输入配置别名", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtAlias.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtIpAddress.Text)) {
                MessageBox.Show("请输入IP地址", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtIpAddress.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPort.Text) || !int.TryParse(txtPort.Text, out int port) || port <= 0 || port > 65535) {
                MessageBox.Show("请输入有效的端口号", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPort.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUsername.Text)) {
                MessageBox.Show("请输入用户名", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUsername.Focus();
                return;
            }

            if (rbtnPassword.Checked && string.IsNullOrWhiteSpace(txtPassword.Text)) {
                MessageBox.Show("请输入密码", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Focus();
                return;
            }

            if (rbtnSshKey.Checked && (string.IsNullOrWhiteSpace(txtSshKeyPath.Text) || !System.IO.File.Exists(txtSshKeyPath.Text))) {
                MessageBox.Show("请指定有效的SSH密钥文件", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtSshKeyPath.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRemoteSavePath.Text)) {
                MessageBox.Show("请输入服务器保存路径", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRemoteSavePath.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLocalFilePath.Text) || !System.IO.File.Exists(txtLocalFilePath.Text)) {
                MessageBox.Show("请指定有效的本地文件路径", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLocalFilePath.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRemoteBackupPath.Text)) {
                MessageBox.Show("请输入服务器备份路径", "验证错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtRemoteBackupPath.Focus();
                return;
            }

            // 保存配置
            DeploymentConfig.Alias = txtAlias.Text.Trim();
            DeploymentConfig.ServiceProvider = txtServiceProvider.Text.Trim();
            DeploymentConfig.IpAddress = txtIpAddress.Text.Trim();
            DeploymentConfig.Port = port;
            DeploymentConfig.Username = txtUsername.Text.Trim();
            DeploymentConfig.Password = rbtnPassword.Checked ? txtPassword.Text : string.Empty;
            DeploymentConfig.SshKeyPath = rbtnSshKey.Checked ? txtSshKeyPath.Text.Trim() : string.Empty;
            DeploymentConfig.RemoteSavePath = txtRemoteSavePath.Text.Trim();
            DeploymentConfig.LocalFilePath = txtLocalFilePath.Text.Trim();
            DeploymentConfig.RemoteBackupPath = txtRemoteBackupPath.Text.Trim();
            DeploymentConfig.UpdatedDate = DateTime.Now;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void rbtnAuthentication_CheckedChanged(object sender, EventArgs e) {
            UpdateControlStates();
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
            this.txtAlias = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtServiceProvider = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtIpAddress = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnBrowseSshKey = new System.Windows.Forms.Button();
            this.txtSshKeyPath = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.rbtnSshKey = new System.Windows.Forms.RadioButton();
            this.rbtnPassword = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.txtRemoteSavePath = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtLocalFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowseLocalFile = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txtRemoteBackupPath = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();

            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "配置别名";

            this.txtAlias.Location = new System.Drawing.Point(14, 30);
            this.txtAlias.Name = "txtAlias";
            this.txtAlias.Size = new System.Drawing.Size(450, 21);
            this.txtAlias.TabIndex = 1;

            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "服务商";

            this.txtServiceProvider.Location = new System.Drawing.Point(14, 80);
            this.txtServiceProvider.Name = "txtServiceProvider";
            this.txtServiceProvider.Size = new System.Drawing.Size(450, 21);
            this.txtServiceProvider.TabIndex = 3;

            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "IP地址";

            this.txtIpAddress.Location = new System.Drawing.Point(14, 130);
            this.txtIpAddress.Name = "txtIpAddress";
            this.txtIpAddress.Size = new System.Drawing.Size(250, 21);
            this.txtIpAddress.TabIndex = 5;

            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(270, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "端口号";

            this.txtPort.Location = new System.Drawing.Point(272, 130);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(192, 21);
            this.txtPort.TabIndex = 7;
            this.txtPort.Text = "22";

            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "用户名";

            this.txtUsername.Location = new System.Drawing.Point(14, 180);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(450, 21);
            this.txtUsername.TabIndex = 9;

            this.groupBox1.Controls.Add(this.btnBrowseSshKey);
            this.groupBox1.Controls.Add(this.txtSshKeyPath);
            this.groupBox1.Controls.Add(this.txtPassword);
            this.groupBox1.Controls.Add(this.rbtnSshKey);
            this.groupBox1.Controls.Add(this.rbtnPassword);
            this.groupBox1.Location = new System.Drawing.Point(14, 215);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(450, 110);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "认证方式";

            this.btnBrowseSshKey.Location = new System.Drawing.Point(369, 70);
            this.btnBrowseSshKey.Name = "btnBrowseSshKey";
            this.btnBrowseSshKey.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseSshKey.TabIndex = 4;
            this.btnBrowseSshKey.Text = "浏览...";
            this.btnBrowseSshKey.UseVisualStyleBackColor = true;
            this.btnBrowseSshKey.Click += new System.EventHandler(this.btnBrowseSshKey_Click);

            this.txtSshKeyPath.Location = new System.Drawing.Point(6, 72);
            this.txtSshKeyPath.Name = "txtSshKeyPath";
            this.txtSshKeyPath.Size = new System.Drawing.Size(357, 21);
            this.txtSshKeyPath.TabIndex = 3;

            this.txtPassword.Location = new System.Drawing.Point(6, 30);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(438, 21);
            this.txtPassword.TabIndex = 2;

            this.rbtnSshKey.AutoSize = true;
            this.rbtnSshKey.Location = new System.Drawing.Point(6, 50);
            this.rbtnSshKey.Name = "rbtnSshKey";
            this.rbtnSshKey.Size = new System.Drawing.Size(71, 16);
            this.rbtnSshKey.TabIndex = 1;
            this.rbtnSshKey.Text = "SSH密钥";
            this.rbtnSshKey.UseVisualStyleBackColor = true;
            this.rbtnSshKey.CheckedChanged += new System.EventHandler(this.rbtnAuthentication_CheckedChanged);

            this.rbtnPassword.AutoSize = true;
            this.rbtnPassword.Location = new System.Drawing.Point(6, 15);
            this.rbtnPassword.Name = "rbtnPassword";
            this.rbtnPassword.Size = new System.Drawing.Size(47, 16);
            this.rbtnPassword.TabIndex = 0;
            this.rbtnPassword.Text = "密码";
            this.rbtnPassword.UseVisualStyleBackColor = true;
            this.rbtnPassword.CheckedChanged += new System.EventHandler(this.rbtnAuthentication_CheckedChanged);

            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 335);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "服务器保存路径";

            this.txtRemoteSavePath.Location = new System.Drawing.Point(14, 350);
            this.txtRemoteSavePath.Name = "txtRemoteSavePath";
            this.txtRemoteSavePath.Size = new System.Drawing.Size(450, 21);
            this.txtRemoteSavePath.TabIndex = 12;

            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 385);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 13;
            this.label7.Text = "本地文件路径";

            this.txtLocalFilePath.Location = new System.Drawing.Point(14, 400);
            this.txtLocalFilePath.Name = "txtLocalFilePath";
            this.txtLocalFilePath.Size = new System.Drawing.Size(369, 21);
            this.txtLocalFilePath.TabIndex = 14;

            this.btnBrowseLocalFile.Location = new System.Drawing.Point(389, 398);
            this.btnBrowseLocalFile.Name = "btnBrowseLocalFile";
            this.btnBrowseLocalFile.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseLocalFile.TabIndex = 15;
            this.btnBrowseLocalFile.Text = "浏览...";
            this.btnBrowseLocalFile.UseVisualStyleBackColor = true;
            this.btnBrowseLocalFile.Click += new System.EventHandler(this.btnBrowseLocalFile_Click);

            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 435);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 12);
            this.label8.TabIndex = 16;
            this.label8.Text = "服务器备份路径";

            this.txtRemoteBackupPath.Location = new System.Drawing.Point(14, 450);
            this.txtRemoteBackupPath.Name = "txtRemoteBackupPath";
            this.txtRemoteBackupPath.Size = new System.Drawing.Size(450, 21);
            this.txtRemoteBackupPath.TabIndex = 17;

            this.btnSave.Location = new System.Drawing.Point(308, 490);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            this.btnCancel.Location = new System.Drawing.Point(389, 490);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 525);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtRemoteBackupPath);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.btnBrowseLocalFile);
            this.Controls.Add(this.txtLocalFilePath);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtRemoteSavePath);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtIpAddress);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtServiceProvider);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtAlias);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeploymentConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "部署配置";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label label1;
        private TextBox txtAlias;
        private Label label2;
        private TextBox txtServiceProvider;
        private Label label3;
        private TextBox txtIpAddress;
        private Label label4;
        private TextBox txtPort;
        private Label label5;
        private TextBox txtUsername;
        private GroupBox groupBox1;
        private Button btnBrowseSshKey;
        private TextBox txtSshKeyPath;
        private TextBox txtPassword;
        private RadioButton rbtnSshKey;
        private RadioButton rbtnPassword;
        private Label label6;
        private TextBox txtRemoteSavePath;
        private Label label7;
        private TextBox txtLocalFilePath;
        private Button btnBrowseLocalFile;
        private Label label8;
        private TextBox txtRemoteBackupPath;
        private Button btnSave;
        private Button btnCancel;
        #endregion
    }
}
