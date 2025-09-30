using System;
using System.Windows.Forms;

namespace SimpleDeploymentTool {
    static class Program {
        /// <summary>
        /// Ӧ�ó��������ڵ㡣
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ����ʾ���ù���·���ı�
            using (var configForm = new ToolPathConfigForm()) {
                if (configForm.ShowDialog() == DialogResult.OK) {
                    // ����·��������ɺ���ʾ����
                    Application.Run(new MainForm());
                }
            }
        }
    }
}
