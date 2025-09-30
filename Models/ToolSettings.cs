using System;

namespace SimpleDeploymentTool.Models {
    [Serializable]
    public class ToolSettings {
        public string PuTTYPath { get; set; }
        public DateTime LastUpdated { get; set; }

        public ToolSettings() {
            LastUpdated = DateTime.Now;
        }
    }
}
