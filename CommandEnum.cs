using System.ComponentModel;

namespace SimpleDeploymentTool {
    public enum CommandEnum {
        [Description("上传")]
        Upload,
        [Description("编译")]
        Build,
        [Description("备份")]
        Backup
    }
}
