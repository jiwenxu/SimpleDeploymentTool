using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleDeploymentTool.Models;

namespace SimpleDeploymentTool.Services {
    public class FileDataService {
        private readonly string _appDataPath;
        private readonly string _projectsFilePath;
        private readonly string _toolSettingsFilePath;

        public FileDataService() {
            // 获取应用程序数据目录
            _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleDeploymentTool");
            if (!Directory.Exists(_appDataPath)) {
                Directory.CreateDirectory(_appDataPath);
            }

            _projectsFilePath = Path.Combine(_appDataPath, "projects.dat");
            _toolSettingsFilePath = Path.Combine(_appDataPath, "toolsettings.dat");
        }

        /// <summary>
        /// 保存项目列表
        /// </summary>
        public void SaveProjects(List<Project> projects) {
            try {
                using (var fileStream = new FileStream(_projectsFilePath, FileMode.Create)) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(fileStream, projects);
                }
            } catch (Exception ex) {
                throw new Exception("保存项目数据失败: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 加载项目列表
        /// </summary>
        public List<Project> LoadProjects() {
            try {
                if (File.Exists(_projectsFilePath)) {
                    using (var fileStream = new FileStream(_projectsFilePath, FileMode.Open)) {
                        var formatter = new BinaryFormatter();
                        return (List<Project>)formatter.Deserialize(fileStream);
                    }
                }
                return new List<Project>();
            } catch (Exception ex) {
                throw new Exception("加载项目数据失败: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 保存工具路径设置
        /// </summary>
        public void SaveToolSettings(ToolSettings settings) {
            try {
                settings.LastUpdated = DateTime.Now;
                using (var fileStream = new FileStream(_toolSettingsFilePath, FileMode.Create)) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(fileStream, settings);
                }
            } catch (Exception ex) {
                throw new Exception("保存工具设置失败: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// 加载工具路径设置
        /// </summary>
        public ToolSettings LoadToolSettings() {
            try {
                if (File.Exists(_toolSettingsFilePath)) {
                    using (var fileStream = new FileStream(_toolSettingsFilePath, FileMode.Open)) {
                        var formatter = new BinaryFormatter();
                        return (ToolSettings)formatter.Deserialize(fileStream);
                    }
                }
                return new ToolSettings();
            } catch (Exception ex) {
                throw new Exception("加载工具设置失败: " + ex.Message, ex);
            }
        }
    }
}
