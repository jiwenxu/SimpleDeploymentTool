using System;

namespace SimpleDeploymentTool.Models {
    [Serializable]
    public class DeploymentConfig {
        public Guid Id { get; set; }
        public string Alias { get; set; } // 配置别名
        public string ServiceProvider { get; set; } // 服务商
        public string IpAddress { get; set; } // IP地址
        public int Port { get; set; } // 端口号
        public string Username { get; set; } // 账号
        public string Password { get; set; } // 密码
        public string SshKeyPath { get; set; } // SSH文件路径
        public string RemoteSavePath { get; set; } // 服务器保存路径
        public string LocalFilePath { get; set; } // 本地文件路径
        public string RemoteBackupPath { get; set; } // 服务器备份路径
        public string fingerPrint { get; set; }// 指纹
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        public DeploymentConfig() {
            Id = Guid.NewGuid();
            Port = 22; // 默认SSH端口
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }
    }
}
