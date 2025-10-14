using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using WinSCP;
using SimpleDeploymentTool.Models;
using System.Text.RegularExpressions;

namespace SimpleDeploymentTool.Services {
    public class CommandExecutionService {
        private readonly ToolSettings _toolSettings;
        private Process _process;

        public event EventHandler<string> OutputReceived;
        public event EventHandler<string> ErrorReceived;
        public event EventHandler ExecutionCompleted;

        public CommandExecutionService(ToolSettings toolSettings) {
            _toolSettings = toolSettings;
        }

        /// <summary>
        /// 执行上传命令
        /// </summary>
        public void ExecuteUpload(DeploymentConfig config) {

            try {
                var fingerPrint = config.fingerPrint;
                SessionOptions sessionOptions = new SessionOptions {
                    Protocol = Protocol.Sftp,
                    HostName = config.IpAddress,
                    UserName = config.Username,
                    SshHostKeyFingerprint = fingerPrint,
                };
                sessionOptions.PortNumber = config.Port;

                // 连接命令
                if (!string.IsNullOrEmpty(config.SshKeyPath) && System.IO.File.Exists(config.SshKeyPath)) {
                    sessionOptions.SshPrivateKeyPath = config.SshKeyPath;
                } else {
                    sessionOptions.Password = config.Password;
                }
                using (Session session = new Session()) {
                    session.FileTransferProgress += (s, e) =>
                    {
                        OnOutputReceived($"Progress: {e.FileProgress * 100}%");
                    };
                    session.Open(sessionOptions);

                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    string remotePath = config.RemoteSavePath;
                    if (!remotePath.EndsWith("/")) {
                        remotePath += "/";
                    }
                    remotePath += Path.GetFileName(config.LocalFilePath);
                    TransferOperationResult transferResult = session.PutFiles(
                        config.LocalFilePath, remotePath, false, transferOptions);
                    transferResult.Check();
                }

                OnExecutionCompleted();
            } catch (Exception ex) {
                OnErrorReceived($"执行上传命令出错: {ex.Message}");
                OnExecutionCompleted();
            }
        }

        /// <summary>
        /// 执行编译命令
        /// </summary>
        public void ExecuteBuild(DeploymentConfig config) {
            // 检查PuTTY/Plink路径
            string executablePath = GetSshClientPath();
            if (string.IsNullOrEmpty(executablePath) || !System.IO.File.Exists(executablePath)) {
                OnErrorReceived("PuTTY/Plink路径无效或未配置");
                OnExecutionCompleted();
                return;
            }

            try {
                // 获取当前时间字符串作为备份文件后缀
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                // 获取文件名
                var fileName = System.IO.Path.GetFileName(config.LocalFilePath);

                // 构建编译命令 - 改进命令结构，增加错误检查
                var buildCommand = $"export LANG=en_US.UTF-8; " +
                                 $"export LC_ALL=en_US.UTF-8; " +  // 强制所有本地化设置为UTF-8
                                   $"cd {EscapePath(config.RemoteSavePath)}; " +
                                 $"if [ $? -ne 0 ]; then " +  // 检查cd命令是否成功
                                 $"echo \"错误: 无法进入目录 {config.RemoteSavePath}\"; exit 1; " +
                                 "fi; " +
                                 "cd ..;" +
                                 "if [ -f \"run.sh\" ]; then " +  // 检查脚本是否存在
                                 "sh run.sh build; " +
                                 "sh run.sh status; " +
                                 "else " +
                                 "echo \"错误: run.sh脚本不存在于当前目录\"; exit 1; " +
                                 "fi;" + 
                                 "exit;";

                // 创建进程启动信息
                var startInfo = new ProcessStartInfo {
                    FileName = executablePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                // 构建命令参数 - 修复参数顺序问题
                var arguments = new StringBuilder();

                // 基础SSH连接参数
                arguments.Append($"-l {config.Username} ");
                //arguments.Append($"-ssh {config.Username}@{config.IpAddress} ");
                arguments.Append($"-P {config.Port} ");
                arguments.Append($"-ssh -no-antispoof {config.Username}@{config.IpAddress} ");

                // 根据认证方式添加参数
                if (!string.IsNullOrEmpty(config.SshKeyPath) && System.IO.File.Exists(config.SshKeyPath)) {
                    // SSH密钥认证
                    arguments.Append($"-i \"{EscapePath(config.SshKeyPath)}\" ");
                    //arguments.Append($"-m - ");  // 通过标准输入传递命令
                    startInfo.RedirectStandardInput = true;
                } else if (!string.IsNullOrEmpty(config.Password)) {
                    arguments.Append($"-pw \"{EscapePassword(config.Password)}\" ");
                    arguments.Append($"\"{buildCommand}\"");  // 直接作为参数传递命令
                } else {
                    OnErrorReceived("请配置SSH密钥或密码用于认证");
                    OnExecutionCompleted();
                    return;
                }

                startInfo.Arguments = arguments.ToString();

                // 打印完整执行命令用于调试
                OnOutputReceived($"[调试] 执行命令: {executablePath} {startInfo.Arguments}");

                // 配置进程
                _process = new Process { StartInfo = startInfo };

                // 注册事件
                _process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OnOutputReceived(e.Data);
                };

                _process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OnErrorReceived(e.Data);
                };

                _process.Exited += (sender, e) => OnExecutionCompleted();

                // 启动进程
                _process.EnableRaisingEvents = true;
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                // 对于密钥认证，通过标准输入发送命令
                if (!string.IsNullOrEmpty(config.SshKeyPath) && System.IO.File.Exists(config.SshKeyPath)) {
                    _process.StandardInput.WriteLine(buildCommand);
                    _process.StandardInput.Flush();
                    _process.StandardInput.Close();
                }
            } catch (Exception ex) {
                OnErrorReceived($"执行编译命令出错: {ex.Message}");
                OnExecutionCompleted();
            }
        }
        /// <summary>
        /// 执行备份命令
        /// </summary>
        public void ExecuteBackup(DeploymentConfig config) {
            // 检查PuTTY/Plink路径
            string executablePath = GetSshClientPath();
            if (string.IsNullOrEmpty(executablePath) || !System.IO.File.Exists(executablePath)) {
                OnErrorReceived("Plink路径无效或未配置");
                OnExecutionCompleted();
                return;
            }

            try {
                // 获取当前时间字符串作为备份文件后缀
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                // 获取文件名
                var fileName = System.IO.Path.GetFileName(config.LocalFilePath);

                // 构建备份命令
                var backupCommand = $"export LANG=en_US.UTF-8; " +
                                   $"export LC_ALL=en_US.UTF-8; " +  // 强制所有本地化设置为UTF-8
                                   $"cd {EscapePath(config.RemoteSavePath)}; " +
                                   $"if [ -f \"{fileName}\" ]; then " +
                                   $"cp \"{fileName}\" \"{EscapePath(config.RemoteBackupPath)}/{fileName}.{timeStamp}\"; " +
                                   "echo \"备份成功: " + fileName + "." + timeStamp + "\"; " +
                                   "else " +
                                   "echo \"错误: 源文件不存在\"; " +
                                   "fi;";

                // 创建进程启动信息
                var startInfo = new ProcessStartInfo {
                    FileName = executablePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                // 构建命令参数
                var arguments = new StringBuilder();

                // 根据认证方式构建不同参数
                if (!string.IsNullOrEmpty(config.SshKeyPath) && System.IO.File.Exists(config.SshKeyPath)) {
                    // SSH密钥认证方式 (使用putty或plink均可)
                    arguments.Append($"-ssh {config.Username}@{config.IpAddress} ");
                    arguments.Append($"-P {config.Port} ");
                    arguments.Append($"-i \"{EscapePath(config.SshKeyPath)}\" ");
                    arguments.Append($"-no-antispoof -m -");
                    startInfo.RedirectStandardInput = true;
                } else if (!string.IsNullOrEmpty(config.Password)) {

                    arguments.Append($"-ssh {config.Username}@{config.IpAddress} -P {config.Port} ");
                    arguments.Append($"-pw \"{EscapePassword(config.Password)}\" ");
                    arguments.Append($"-no-antispoof ");
                    startInfo.Arguments = $"{arguments} \"{backupCommand}\"";
                } else {
                    OnErrorReceived("请配置SSH密钥或密码用于认证");
                    OnExecutionCompleted();
                    return;
                }

                // 设置参数（密钥认证的参数在上面单独设置了）
                if (string.IsNullOrEmpty(startInfo.Arguments)) {
                    startInfo.Arguments = arguments.ToString();
                }
                // 打印完整执行命令用于调试
                OnOutputReceived($"[调试] 执行命令: {executablePath} {startInfo.Arguments}");
                // 配置进程
                _process = new Process { StartInfo = startInfo };

                // 注册事件（保持不变）
                _process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OnOutputReceived(e.Data);
                };

                _process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OnErrorReceived(e.Data);
                };

                _process.Exited += (sender, e) => OnExecutionCompleted();

                // 启动进程
                _process.EnableRaisingEvents = true;
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();

                // 对于密钥认证，通过标准输入发送命令
                if (!string.IsNullOrEmpty(config.SshKeyPath) && System.IO.File.Exists(config.SshKeyPath)) {
                    _process.StandardInput.WriteLine(backupCommand);
                    _process.StandardInput.Flush();
                    _process.StandardInput.Close();
                }
            } catch (Exception ex) {
                OnErrorReceived($"执行备份命令出错: {ex.Message}");
                OnExecutionCompleted();
            }
        }

        /// <summary>
        /// 获取SSH客户端路径（优先使用plink，没有则使用putty）
        /// </summary>
        private string GetSshClientPath() {

            // 否则使用putty路径
            return _toolSettings.PuTTYPath;
        }

        /// <summary>
        /// 获取远程服务器的 SshHostKeyFingerprint
        /// </summary>
        /// <param name="winscpPath">WinSCP.com 的完整路径</param>
        /// <param name="host">主机名或 IP</param>
        /// <param name="port">端口，默认 22</param>
        /// <param name="username">用户名</param>
        /// <returns>ssh-rsa / ssh-ed25519 指纹字符串</returns>
        private string GetSshHostKeyFingerprint(
            string winscpPath,
            string host,
            string username,
            string privateKeyPath,
            string password,
            int port = 22) {
            var scriptBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(privateKeyPath) && System.IO.File.Exists(privateKeyPath)) {
                scriptBuilder.AppendLine($"\"open sftp://{username}@{host}:{port} -privatekey=\"\"{privateKeyPath}\" -hostkey=*\" \"exit\"");
            } else {
                scriptBuilder.AppendLine($"\"open sftp://{username}:{password}@{host}:{port} -hostkey=*\" \"exit\"");
            }

            var startInfo = new ProcessStartInfo {
                FileName = winscpPath,
                Arguments = $"/command {scriptBuilder.ToString()}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            OnOutputReceived($"获取指纹: {winscpPath} /command {scriptBuilder.ToString()}");
            using var process = new Process { StartInfo = startInfo };
            var outputBuilder = new StringBuilder();
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            string output = outputBuilder.ToString();

            // WinSCP 输出里会包含类似：
            // "Host key fingerprint is ssh-ed25519 255 SHA256:xxxxxxxxxxxxxxxx"
            var match = Regex.Match(output, @"Host key fingerprint is (?<fp>.+)");
            if (match.Success) {
                return match.Groups["fp"].Value.Trim();
            }

            throw new Exception("未能解析到 HostKey 指纹。输出如下：\n" + output);
        }

        /// <summary>
        /// 处理密码中的特殊字符
        /// </summary>
        private string EscapePassword(string password) {
            if (string.IsNullOrEmpty(password))
                return password;

            // 转义双引号和反斜杠
            return password.Replace("\\", "\\\\")
                           .Replace("\"", "\\\"")
                           .Replace("$", "\\$")
                           .Replace("`", "\\`");
        }

        /// <summary>
        /// 处理路径中的特殊字符
        /// </summary>
        private string EscapePath(string path) {
            if (string.IsNullOrEmpty(path))
                return path;

            // 替换路径中的双引号为转义后的双引号
            return path.Replace("\"", "\\\"");
        }
        /// <summary>
        /// 终止当前执行的进程
        /// </summary>
        public void Terminate() {
            if (_process != null && !_process.HasExited) {
                try {
                    _process.Kill();
                } catch (Exception ex) {
                    OnErrorReceived($"终止进程出错: {ex.Message}");
                }
            }
        }

        protected virtual void OnOutputReceived(string message) {
            OutputReceived?.Invoke(this, message);
        }

        protected virtual void OnErrorReceived(string message) {
            ErrorReceived?.Invoke(this, message);
        }

        protected virtual void OnExecutionCompleted() {
            ExecutionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
