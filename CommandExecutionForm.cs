using System;
using System.Windows.Forms;
using SimpleDeploymentTool.Models;
using SimpleDeploymentTool.Services;

namespace SimpleDeploymentTool {
    public partial class CommandExecutionForm : Form {
        private readonly CommandExecutionService _executionService;
        private readonly DeploymentConfig _config;
        private readonly CommandEnum _commandType;

        public CommandExecutionForm(ToolSettings toolSettings, DeploymentConfig config, CommandEnum commandType) {
            InitializeComponent();
            _config = config;
            _commandType = commandType;

            // 初始化执行服务
            _executionService = new CommandExecutionService(toolSettings);
            _executionService.OutputReceived += ExecutionService_OutputReceived;
            _executionService.ErrorReceived += ExecutionService_ErrorReceived;
            _executionService.ExecutionCompleted += ExecutionService_ExecutionCompleted;

            // 设置窗口标题
            Text = $"{(_commandType.GetDescription())} - {_config.Alias} ({_config.IpAddress})";

            // 开始执行命令
            StartExecution();
        }

        /// <summary>
        /// 开始执行命令
        /// </summary>
        private void StartExecution() {
            AppendOutput($"开始{(_commandType.GetDescription())}操作...");
            AppendOutput($"目标服务器: {_config.IpAddress}:{_config.Port}");

            btnCancel.Enabled = true;
            btnClose.Enabled = false;

            // 在单独的线程中执行命令，避免UI阻塞
            System.Threading.ThreadPool.QueueUserWorkItem((state) => {
                switch (_commandType) {
                    case CommandEnum.Upload:
                        _executionService.ExecuteUpload(_config);
                        break;
                    case CommandEnum.Build:
                        _executionService.ExecuteBuild(_config);
                        break;
                    case CommandEnum.Backup:
                        _executionService.ExecuteBackup(_config);
                        break;
                }
            });
        }

        /// <summary>
        /// 输出收到事件处理
        /// </summary>
        private void ExecutionService_OutputReceived(object sender, string message) {
            // 确保在UI线程中更新
            if (txtOutput.InvokeRequired) {
                txtOutput.Invoke(new Action<string>(AppendOutput), message);
            } else {
                AppendOutput(message);
            }
        }

        /// <summary>
        /// 错误收到事件处理
        /// </summary>
        private void ExecutionService_ErrorReceived(object sender, string message) {
            // 确保在UI线程中更新
            if (txtOutput.InvokeRequired) {
                txtOutput.Invoke(new Action<string>(AppendError), message);
            } else {
                AppendError(message);
            }
        }

        /// <summary>
        /// 执行完成事件处理
        /// </summary>
        private void ExecutionService_ExecutionCompleted(object sender, EventArgs e) {
            if (InvokeRequired) {
                Invoke(new Action(OnExecutionCompleted));
            } else {
                OnExecutionCompleted();
            }
        }

        /// <summary>
        /// 执行完成处理
        /// </summary>
        private void OnExecutionCompleted() {
            AppendOutput($"{_commandType.GetDescription()}操作已完成");
            btnCancel.Enabled = false;
            btnClose.Enabled = true;
        }

        /// <summary>
        /// 添加普通输出
        /// </summary>
        private void AppendOutput(string message) {
            txtOutput.SelectionColor = System.Drawing.Color.Black;
            txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtOutput.ScrollToCaret();
        }

        /// <summary>
        /// 添加错误输出
        /// </summary>
        private void AppendError(string message) {
            txtOutput.SelectionColor = System.Drawing.Color.Red;
            txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] 错误: {message}{Environment.NewLine}");
            txtOutput.ScrollToCaret();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            if (MessageBox.Show("确定要取消当前操作吗？", "确认取消", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                _executionService.Terminate();
                AppendOutput("操作已被用户取消");
                btnCancel.Enabled = false;
                btnClose.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e) {
            Close();
        }

        private void CommandExecutionForm_FormClosing(object sender, FormClosingEventArgs e) {
            // 如果正在执行中，询问用户是否确定关闭
            if (btnCancel.Enabled &&
                MessageBox.Show("当前操作正在进行中，确定要关闭窗口吗？", "确认关闭",
                              MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No) {
                e.Cancel = true;
            }
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
            this.txtOutput = new System.Windows.Forms.RichTextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtOutput.Location = new System.Drawing.Point(0, 0);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(700, 400);
            this.txtOutput.TabIndex = 0;
            this.txtOutput.Text = "";

            this.btnCancel.Location = new System.Drawing.Point(542, 410);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.btnClose.Location = new System.Drawing.Point(623, 410);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 445);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtOutput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CommandExecutionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "命令执行";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CommandExecutionForm_FormClosing);
            this.ResumeLayout(false);
        }

        private RichTextBox txtOutput;
        private Button btnCancel;
        private Button btnClose;
        #endregion
    }
}
