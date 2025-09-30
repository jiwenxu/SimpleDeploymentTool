using System;
using System.Windows.Forms;

namespace SimpleDeploymentTool {
    static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 先显示配置工具路径的表单
            using (var configForm = new ToolPathConfigForm()) {
                if (configForm.ShowDialog() == DialogResult.OK) {
                    // 工具路径配置完成后显示主表单
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
