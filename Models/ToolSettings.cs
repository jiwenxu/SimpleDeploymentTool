using System;

namespace SimpleDeploymentTool.Models {
    [Serializable]
    public class ToolSettings {
        public string WinSCPPath { get; set; }
        public string PuTTYPath { get; set; }
        public DateTime LastUpdated { get; set; }

        public ToolSettings() {
            LastUpdated = DateTime.Now;
        }
    }
}
