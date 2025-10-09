using SimpleDeploymentTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WinSCP;

namespace SimpleDeploymentTool {
    public partial class FileManagerForm : Form {
        private DeploymentConfig _config;
        private Session _session;
        private string _currentDirectory;

        public FileManagerForm(DeploymentConfig config) {
            InitializeComponent();
            _config = config;
        }

        private void FileManagerForm_Load(object sender, EventArgs e) {
            InitializeDirectoryComboBox();
            InitializeContextMenu();
            ConnectToServer();
        }

        private void InitializeDirectoryComboBox() {
            cmbDirectories.Items.Clear();
            cmbDirectories.Items.Add(_config.RemoteSavePath);
            cmbDirectories.Items.Add(_config.RemoteBackupPath);

            if (cmbDirectories.Items.Count > 0)
                cmbDirectories.SelectedIndex = 0;
        }

        private void InitializeContextMenu() {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem deleteItem = new ToolStripMenuItem("删除");
            deleteItem.Click += DeleteItem_Click;

            ToolStripMenuItem deleteAllItem = new ToolStripMenuItem("全部删除");
            deleteAllItem.Click += DeleteAllItem_Click;

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(deleteAllItem);

            lstFiles.ContextMenuStrip = contextMenu;
        }

        private void ConnectToServer() {
            try {
                SessionOptions sessionOptions = new SessionOptions {
                    Protocol = Protocol.Sftp,
                    HostName = _config.IpAddress,
                    PortNumber = _config.Port,
                    UserName = _config.Username,
                    Password = _config.Password,
                    SshHostKeyFingerprint = _config.fingerPrint
                };

                // 如果有SSH密钥路径，使用密钥认证
                if (!string.IsNullOrEmpty(_config.SshKeyPath)) {
                    sessionOptions.SshPrivateKeyPath = _config.SshKeyPath;
                    sessionOptions.Password = string.Empty; // 使用密钥时不需要密码
                }

                _session = new Session();
                _session.Open(sessionOptions);

                LoadDirectoryFiles();
            } catch (Exception ex) {
                MessageBox.Show($"连接服务器失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDirectoryFiles() {
            if (_session == null || !_session.Opened)
                return;

            try {
                string selectedDirectory = cmbDirectories.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedDirectory))
                    return;

                _currentDirectory = selectedDirectory;

                RemoteDirectoryInfo directoryInfo = _session.ListDirectory(selectedDirectory);
                lstFiles.Items.Clear();

                foreach (RemoteFileInfo fileInfo in directoryInfo.Files) {
                    if (!fileInfo.IsDirectory && fileInfo.Name != "." && fileInfo.Name != "..") {
                        lstFiles.Items.Add(new FileListItem {
                            FileName = fileInfo.Name,
                            FileSize = FormatFileSize(fileInfo.Length),
                            LastModified = fileInfo.LastWriteTime
                        });
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show($"加载文件列表失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatFileSize(long bytes) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1) {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void cmbDirectories_SelectedIndexChanged(object sender, EventArgs e) {
            LoadDirectoryFiles();
        }

        private void cmbDirectories_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                LoadDirectoryFiles();
            }
        }

        private void DeleteItem_Click(object sender, EventArgs e) {
            if (lstFiles.SelectedItem == null) {
                MessageBox.Show("请选择要删除的文件", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FileListItem selectedFile = (FileListItem)lstFiles.SelectedItem;

            DialogResult result = MessageBox.Show(
                $"确定要删除文件 '{selectedFile.FileName}' 吗？",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes) {
                DeleteFile(selectedFile.FileName);
            }
        }

        private void DeleteAllItem_Click(object sender, EventArgs e) {
            if (lstFiles.Items.Count == 0) {
                MessageBox.Show("没有可删除的文件", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "确定要删除当前目录下的所有文件吗？此操作不可恢复！",
                "确认全部删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes) {
                DeleteAllFiles();
            }
        }

        private void DeleteFile(string fileName) {
            try {
                string filePath = $"{_currentDirectory}/{fileName}";
                _session.RemoveFiles(filePath);

                MessageBox.Show($"文件 '{fileName}' 删除成功", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadDirectoryFiles(); // 刷新文件列表
            } catch (Exception ex) {
                MessageBox.Show($"删除文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteAllFiles() {
            try {
                foreach (FileListItem item in lstFiles.Items) {
                    string filePath = $"{_currentDirectory}/{item.FileName}";
                    _session.RemoveFiles(filePath);
                }

                MessageBox.Show("所有文件删除成功", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadDirectoryFiles(); // 刷新文件列表
            } catch (Exception ex) {
                MessageBox.Show($"删除文件失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FileManagerForm_FormClosing(object sender, FormClosingEventArgs e) {
            _session?.Dispose();
        }
    }

    // 用于在ListBox中显示文件信息的类
    public class FileListItem {
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public DateTime LastModified { get; set; }

        public override string ToString() {
            return $"{FileName} ({FileSize}) - {LastModified:yyyy-MM-dd HH:mm:ss}";
        }
    }
}
