using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleDeploymentTool.Models {
    [Serializable]
    public class Project {
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<DeploymentConfig> Configurations { get; set; }

        public Project() {
            Configurations = new List<DeploymentConfig>();
            CreatedDate = DateTime.Now;
            UpdatedDate = DateTime.Now;
        }

        public Project(string name) : this() {
            Name = name;
        }

        /// <summary>
        /// 添加配置
        /// </summary>
        public void AddConfiguration(DeploymentConfig config) {
            Configurations.Add(config);
            UpdatedDate = DateTime.Now;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        public bool UpdateConfiguration(DeploymentConfig config) {
            var existingConfig = Configurations.FirstOrDefault(c => c.Id == config.Id);
            if (existingConfig != null) {
                Configurations.Remove(existingConfig);
                Configurations.Add(config);
                UpdatedDate = DateTime.Now;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除配置
        /// </summary>
        public bool RemoveConfiguration(Guid configId) {
            var configToRemove = Configurations.FirstOrDefault(c => c.Id == configId);
            if (configToRemove != null) {
                Configurations.Remove(configToRemove);
                UpdatedDate = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}
