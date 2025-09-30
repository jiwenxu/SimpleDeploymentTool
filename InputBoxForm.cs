using System;
using System.Windows.Forms;

namespace SimpleDeploymentTool {
    public partial class InputBoxForm : Form {
        public string InputText { get; private set; }

        public InputBoxForm(string title, string prompt) : this(title, prompt, "") {
        }

        public InputBoxForm(string title, string prompt, string defaultValue) {
            InitializeComponent();
            Text = title;
            lblPrompt.Text = prompt;
            txtInput.Text = defaultValue;
            txtInput.SelectAll();
        }

        private void btnOK_Click(object sender, EventArgs e) {
            InputText = txtInput.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void txtInput_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                btnOK_Click(sender, e);
            } else if (e.KeyChar == (char)Keys.Escape) {
                btnCancel_Click(sender, e);
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
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(12, 15);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(35, 12);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "提示";

            this.txtInput.Location = new System.Drawing.Point(14, 30);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(350, 21);
            this.txtInput.TabIndex = 1;
            this.txtInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtInput_KeyPress);

            this.btnOK.Location = new System.Drawing.Point(210, 60);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            this.btnCancel.Location = new System.Drawing.Point(290, 60);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 95);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.lblPrompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBoxForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "输入框";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label lblPrompt;
        private TextBox txtInput;
        private Button btnOK;
        private Button btnCancel;
        #endregion
    }
}
