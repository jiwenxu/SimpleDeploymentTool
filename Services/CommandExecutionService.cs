using System;
using System.Diagnostics;
using System.Text;
using SimpleDeploymentTool.Models;

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
            if (string.IsNullOrEmpty(_toolSettings.WinSCPPath) || !System.IO.File.Exists(_toolSettings.WinSCPPath)) {
                OnErrorReceived("WinSCP路径无效或未配置");
                OnExecutionCompleted();
                return;
            }

            try {
                // 构建WinSCP脚本内容
                var scriptBuilder = new StringBuilder();
                scriptBuilder.AppendLine("echo \"连接远程服务器\"");

                // 连接命令
                if (!string.IsNullOrEmpty(config.SshKeyPath) && System.IO.File.Exists(config.SshKeyPath)) {
                    scriptBuilder.AppendLine($"open sftp://{config.Username}@{config.IpAddress}:{config.Port} -privatekey=\"{config.SshKeyPath}\"");
                } else {
                    scriptBuilder.AppendLine($"open sftp://{config.Username}:{config.Password}@{config.IpAddress}:{config.Port}");
                }

                // 切换到服务器保存目录
                scriptBuilder.AppendLine("echo \"进入目录\"");
                scriptBuilder.AppendLine($"cd \"{config.RemoteSavePath}\"");
                scriptBuilder.AppendLine("echo \"上传文件\"");
                // 上传文件
                scriptBuilder.AppendLine($"put \"{config.LocalFilePath}\"");

                // 退出
                scriptBuilder.AppendLine("exit");

                // 创建临时脚本文件
                var scriptPath = System.IO.Path.GetTempFileName() + ".txt";
                System.IO.File.WriteAllText(scriptPath, scriptBuilder.ToString());
                // 打印完整命令用于调试
                OnOutputReceived($"[调试] 执行命令: {_toolSettings.WinSCPPath} {scriptBuilder.ToString()}");
                // 配置进程
                _process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = _toolSettings.WinSCPPath,
                        Arguments = $"/console /script=\"{scriptPath}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                // 注册事件
                _process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OnOutputReceived(e.Data);
                };

                _process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OnErrorReceived(e.Data);
                };

                _process.Exited += (sender, e) => {
                    OnExecutionCompleted();
                    // 清理临时文件
                    if (System.IO.File.Exists(scriptPath)) {
                        try { System.IO.File.Delete(scriptPath); } catch { }
                    }
                };

                // 启动进程
                _process.EnableRaisingEvents = true;
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
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
                                 //$"if [ -f \"{fileName}\" ]; then " +  // 检查文件是否存在
                                 //$"cp \"{fileName}\" \"{EscapePath(config.RemoteBackupPath)}/{fileName}.{timeStamp}\"; " +
                                 //"echo \"备份成功: " + fileName + "." + timeStamp + "\"; " +
                                 //"else " +
                                 //"echo \"警告: 源文件不存在，继续执行编译\"; " +
                                 //"fi; " +
                                 "cd ..;" +
                                 "if [ -f \"run.sh\" ]; then " +  // 检查脚本是否存在
                                 "sh run.sh build; " +
                                 "sh run.sh status; " +
                                 "else " +
                                 "echo \"错误: run.sh脚本不存在于当前目录\"; exit 1; " +
                                 "fi";

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
                arguments.Append($"-ssh {config.Username}@{config.IpAddress} ");
                arguments.Append($"-P {config.Port} ");
                arguments.Append($"-no-antispoof ");

                // 根据认证方式添加参数
                if (!string.IsNullOrEmpty(config.SshKeyPath) && System.IO.File.Exists(config.SshKeyPath)) {
                    // SSH密钥认证
                    arguments.Append($"-i \"{EscapePath(config.SshKeyPath)}\" ");
                    arguments.Append($"-m -");  // 通过标准输入传递命令
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
                                   "fi";

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
