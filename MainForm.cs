using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SimpleDeploymentTool.Models;
using SimpleDeploymentTool.Services;

namespace SimpleDeploymentTool {
    public partial class MainForm : Form {
        private readonly FileDataService _dataService;
        private ToolSettings _toolSettings;
        private List<Project> _projects;
        private Project _selectedProject;

        public MainForm() {
            InitializeComponent();
            _dataService = new FileDataService();
            _toolSettings = _dataService.LoadToolSettings();
            LoadProjects();
        }

        /// <summary>
        /// 加载项目列表
        /// </summary>
        private void LoadProjects() {
            try {
                _projects = _dataService.LoadProjects();
                lstProjects.Items.Clear();
                foreach (var project in _projects) {
                    lstProjects.Items.Add(project.Name);
                }
                ClearProjectDetails();
            } catch (Exception ex) {
                MessageBox.Show($"加载项目失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 清除项目详情显示
        /// </summary>
        private void ClearProjectDetails() {
            lstConfigurations.Items.Clear();
            _selectedProject = null;
            btnAddConfig.Enabled = false;
            btnEditProject.Enabled = false;
            btnDeleteProject.Enabled = false;
            btnEditConfig.Enabled = false;
            btnDeleteConfig.Enabled = false;
            btnExecuteUpload.Enabled = false;
            btnExecuteBuild.Enabled = false;
            btnBackup.Enabled = false;
            btnList.Enabled = false;
        }

        /// <summary>
        /// 刷新配置列表
        /// </summary>
        private void RefreshConfigurations() {
            lstConfigurations.Items.Clear();
            if (_selectedProject != null) {
                foreach (var config in _selectedProject.Configurations) {
                    lstConfigurations.Items.Add($"{config.Alias} ({config.IpAddress}:{config.Port})");
                }
            }
        }

        private void btnAddProject_Click(object sender, EventArgs e) {
            using (var inputBox = new InputBoxForm("新建项目", "请输入项目名称:")) {
                if (inputBox.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(inputBox.InputText)) {
                    string projectName = inputBox.InputText.Trim();

                    // 检查项目名称是否已存在
                    if (_projects.Exists(p => p.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase))) {
                        MessageBox.Show("项目名称已存在，请使用其他名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 创建新项目
                    var newProject = new Project(projectName);
                    _projects.Add(newProject);

                    // 保存并刷新列表
                    _dataService.SaveProjects(_projects);
                    LoadProjects();

                    // 选中新创建的项目
                    int index = lstProjects.FindStringExact(projectName);
                    if (index != -1) {
                        lstProjects.SelectedIndex = index;
                    }
                }
            }
        }

        private void btnEditProject_Click(object sender, EventArgs e) {
            if (_selectedProject == null) return;

            using (var inputBox = new InputBoxForm("编辑项目", "请输入项目名称:", _selectedProject.Name)) {
                if (inputBox.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(inputBox.InputText)) {
                    string newName = inputBox.InputText.Trim();

                    // 检查项目名称是否已存在（排除当前项目）
                    if (_projects.Exists(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) &&
                                            !p.Name.Equals(_selectedProject.Name, StringComparison.OrdinalIgnoreCase))) {
                        MessageBox.Show("项目名称已存在，请使用其他名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 更新项目名称
                    _selectedProject.Name = newName;
                    _selectedProject.UpdatedDate = DateTime.Now;

                    // 保存并刷新列表
                    _dataService.SaveProjects(_projects);
                    LoadProjects();

                    // 选中修改后的项目
                    int index = lstProjects.FindStringExact(newName);
                    if (index != -1) {
                        lstProjects.SelectedIndex = index;
                    }
                }
            }
        }

        private void btnDeleteProject_Click(object sender, EventArgs e) {
            if (_selectedProject == null) return;

            if (MessageBox.Show($"确定要删除项目「{_selectedProject.Name}」吗？\n此操作将删除该项目的所有配置。",
                               "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                // 从列表中移除项目
                _projects.Remove(_selectedProject);

                // 保存并刷新列表
                _dataService.SaveProjects(_projects);
                LoadProjects();
            }
        }

        private void btnAddConfig_Click(object sender, EventArgs e) {
            if (_selectedProject == null) return;

            using (var configForm = new DeploymentConfigForm()) {
                if (configForm.ShowDialog() == DialogResult.OK) {
                    // 添加新配置
                    _selectedProject.AddConfiguration(configForm.DeploymentConfig);

                    // 保存并刷新配置列表
                    _dataService.SaveProjects(_projects);
                    RefreshConfigurations();
                }
            }
        }

        private void btnEditConfig_Click(object sender, EventArgs e) {
            if (_selectedProject == null || lstConfigurations.SelectedIndex < 0) return;

            var selectedConfig = _selectedProject.Configurations[lstConfigurations.SelectedIndex];

            using (var configForm = new DeploymentConfigForm(selectedConfig)) {
                if (configForm.ShowDialog() == DialogResult.OK) {
                    // 更新配置
                    _selectedProject.UpdateConfiguration(configForm.DeploymentConfig);

                    // 保存并刷新配置列表
                    _dataService.SaveProjects(_projects);
                    RefreshConfigurations();
                }
            }
        }

        private void btnDeleteConfig_Click(object sender, EventArgs e) {
            if (_selectedProject == null || lstConfigurations.SelectedIndex < 0) return;

            var selectedConfig = _selectedProject.Configurations[lstConfigurations.SelectedIndex];

            if (MessageBox.Show($"确定要删除配置「{selectedConfig.Alias}」吗？",
                               "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                // 从项目中移除配置
                _selectedProject.RemoveConfiguration(selectedConfig.Id);

                // 保存并刷新配置列表
                _dataService.SaveProjects(_projects);
                RefreshConfigurations();
            }
        }

        private void btnExecuteUpload_Click(object sender, EventArgs e) {
            if (_selectedProject == null || lstConfigurations.SelectedIndex < 0) return;

            var selectedConfig = _selectedProject.Configurations[lstConfigurations.SelectedIndex];
            ExecuteCommand(selectedConfig, CommandEnum.Upload);
        }

        private void btnExecuteBuild_Click(object sender, EventArgs e) {
            if (_selectedProject == null || lstConfigurations.SelectedIndex < 0) return;

            var selectedConfig = _selectedProject.Configurations[lstConfigurations.SelectedIndex];
            ExecuteCommand(selectedConfig, CommandEnum.Build);
        }

        // 备份按钮点击事件
        private void btnBackup_Click(object sender, EventArgs e) {
            if (_selectedProject == null || lstConfigurations.SelectedIndex < 0) return;

            var selectedConfig = _selectedProject.Configurations[lstConfigurations.SelectedIndex];
            ExecuteCommand(selectedConfig, CommandEnum.Backup);
        }
        private void btnList_Click(object sender, EventArgs e) {
            if (_selectedProject == null || lstConfigurations.SelectedIndex < 0) return;

            var selectedConfig = _selectedProject.Configurations[lstConfigurations.SelectedIndex];
            using (var executionForm = new FileManagerForm(selectedConfig)) {
                executionForm.ShowDialog();
            }
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        private void ExecuteCommand(DeploymentConfig config, CommandEnum commandType) {
            using (var executionForm = new CommandExecutionForm(_toolSettings, config, commandType)) {
                executionForm.ShowDialog();
            }
        }

        private void lstProjects_SelectedIndexChanged(object sender, EventArgs e) {
            if (lstProjects.SelectedIndex >= 0 && lstProjects.SelectedIndex < _projects.Count) {
                _selectedProject = _projects[lstProjects.SelectedIndex];
                RefreshConfigurations();

                // 启用项目操作按钮
                btnEditProject.Enabled = true;
                btnDeleteProject.Enabled = true;
                btnAddConfig.Enabled = true;
            } else {
                ClearProjectDetails();
            }
        }

        private void lstConfigurations_SelectedIndexChanged(object sender, EventArgs e) {
            bool hasSelected = lstConfigurations.SelectedIndex >= 0;
            btnEditConfig.Enabled = hasSelected;
            btnDeleteConfig.Enabled = hasSelected;
            btnExecuteUpload.Enabled = hasSelected;
            btnExecuteBuild.Enabled = hasSelected;
            btnBackup.Enabled = hasSelected;
            btnList.Enabled = hasSelected;
        }

        private void btnSettings_Click(object sender, EventArgs e) {
            using (var configForm = new ToolPathConfigForm()) {
                configForm.ShowDialog();
                // 重新加载工具设置
                _toolSettings = _dataService.LoadToolSettings();
            }
        }

        #region Windows 窗体设计器生成的代码
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lstProjects = new System.Windows.Forms.ListBox();
            this.btnDeleteProject = new System.Windows.Forms.Button();
            this.btnEditProject = new System.Windows.Forms.Button();
            this.btnAddProject = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lstConfigurations = new System.Windows.Forms.ListBox();
            this.btnExecuteBuild = new System.Windows.Forms.Button();
            this.btnExecuteUpload = new System.Windows.Forms.Button();
            this.btnDeleteConfig = new System.Windows.Forms.Button();
            this.btnEditConfig = new System.Windows.Forms.Button();
            this.btnAddConfig = new System.Windows.Forms.Button();
            this.btnBackup = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnList = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(915, 795);
            this.splitContainer1.SplitterDistance = 304;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstProjects);
            this.groupBox1.Controls.Add(this.btnDeleteProject);
            this.groupBox1.Controls.Add(this.btnEditProject);
            this.groupBox1.Controls.Add(this.btnAddProject);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(304, 795);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "项目列表";
            // 
            // lstProjects
            // 
            this.lstProjects.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstProjects.FormattingEnabled = true;
            this.lstProjects.ItemHeight = 17;
            this.lstProjects.Location = new System.Drawing.Point(4, 20);
            this.lstProjects.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstProjects.Name = "lstProjects";
            this.lstProjects.Size = new System.Drawing.Size(296, 701);
            this.lstProjects.TabIndex = 3;
            this.lstProjects.SelectedIndexChanged += new System.EventHandler(this.lstProjects_SelectedIndexChanged);
            // 
            // btnDeleteProject
            // 
            this.btnDeleteProject.Location = new System.Drawing.Point(204, 741);
            this.btnDeleteProject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnDeleteProject.Name = "btnDeleteProject";
            this.btnDeleteProject.Size = new System.Drawing.Size(88, 33);
            this.btnDeleteProject.TabIndex = 2;
            this.btnDeleteProject.Text = "删除项目";
            this.btnDeleteProject.UseVisualStyleBackColor = true;
            this.btnDeleteProject.Click += new System.EventHandler(this.btnDeleteProject_Click);
            // 
            // btnEditProject
            // 
            this.btnEditProject.Location = new System.Drawing.Point(110, 741);
            this.btnEditProject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnEditProject.Name = "btnEditProject";
            this.btnEditProject.Size = new System.Drawing.Size(88, 33);
            this.btnEditProject.TabIndex = 1;
            this.btnEditProject.Text = "编辑项目";
            this.btnEditProject.UseVisualStyleBackColor = true;
            this.btnEditProject.Click += new System.EventHandler(this.btnEditProject_Click);
            // 
            // btnAddProject
            // 
            this.btnAddProject.Location = new System.Drawing.Point(15, 741);
            this.btnAddProject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAddProject.Name = "btnAddProject";
            this.btnAddProject.Size = new System.Drawing.Size(88, 33);
            this.btnAddProject.TabIndex = 0;
            this.btnAddProject.Text = "新建项目";
            this.btnAddProject.UseVisualStyleBackColor = true;
            this.btnAddProject.Click += new System.EventHandler(this.btnAddProject_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lstConfigurations);
            this.groupBox2.Controls.Add(this.btnExecuteBuild);
            this.groupBox2.Controls.Add(this.btnExecuteUpload);
            this.groupBox2.Controls.Add(this.btnDeleteConfig);
            this.groupBox2.Controls.Add(this.btnEditConfig);
            this.groupBox2.Controls.Add(this.btnAddConfig);
            this.groupBox2.Controls.Add(this.btnBackup);
            this.groupBox2.Controls.Add(this.btnList);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(606, 795);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "配置列表";
            // 
            // lstConfigurations
            // 
            this.lstConfigurations.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstConfigurations.FormattingEnabled = true;
            this.lstConfigurations.ItemHeight = 17;
            this.lstConfigurations.Location = new System.Drawing.Point(4, 20);
            this.lstConfigurations.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstConfigurations.Name = "lstConfigurations";
            this.lstConfigurations.Size = new System.Drawing.Size(598, 701);
            this.lstConfigurations.TabIndex = 5;
            this.lstConfigurations.SelectedIndexChanged += new System.EventHandler(this.lstConfigurations_SelectedIndexChanged);
            // 
            // btnExecuteBuild
            // 
            this.btnExecuteBuild.Location = new System.Drawing.Point(320, 741);
            this.btnExecuteBuild.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnExecuteBuild.Name = "btnExecuteBuild";
            this.btnExecuteBuild.Size = new System.Drawing.Size(70, 33);
            this.btnExecuteBuild.TabIndex = 4;
            this.btnExecuteBuild.Text = "执行编译";
            this.btnExecuteBuild.UseVisualStyleBackColor = true;
            this.btnExecuteBuild.Click += new System.EventHandler(this.btnExecuteBuild_Click);
            // 
            // btnExecuteUpload
            // 
            this.btnExecuteUpload.Location = new System.Drawing.Point(242, 741);
            this.btnExecuteUpload.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnExecuteUpload.Name = "btnExecuteUpload";
            this.btnExecuteUpload.Size = new System.Drawing.Size(70, 33);
            this.btnExecuteUpload.TabIndex = 3;
            this.btnExecuteUpload.Text = "执行上传";
            this.btnExecuteUpload.UseVisualStyleBackColor = true;
            this.btnExecuteUpload.Click += new System.EventHandler(this.btnExecuteUpload_Click);
            // 
            // btnDeleteConfig
            // 
            this.btnDeleteConfig.Location = new System.Drawing.Point(164, 741);
            this.btnDeleteConfig.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnDeleteConfig.Name = "btnDeleteConfig";
            this.btnDeleteConfig.Size = new System.Drawing.Size(70, 33);
            this.btnDeleteConfig.TabIndex = 2;
            this.btnDeleteConfig.Text = "删除配置";
            this.btnDeleteConfig.UseVisualStyleBackColor = true;
            this.btnDeleteConfig.Click += new System.EventHandler(this.btnDeleteConfig_Click);
            // 
            // btnEditConfig
            // 
            this.btnEditConfig.Location = new System.Drawing.Point(86, 741);
            this.btnEditConfig.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnEditConfig.Name = "btnEditConfig";
            this.btnEditConfig.Size = new System.Drawing.Size(70, 33);
            this.btnEditConfig.TabIndex = 1;
            this.btnEditConfig.Text = "编辑配置";
            this.btnEditConfig.UseVisualStyleBackColor = true;
            this.btnEditConfig.Click += new System.EventHandler(this.btnEditConfig_Click);
            // 
            // btnAddConfig
            // 
            this.btnAddConfig.Location = new System.Drawing.Point(8, 741);
            this.btnAddConfig.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAddConfig.Name = "btnAddConfig";
            this.btnAddConfig.Size = new System.Drawing.Size(70, 33);
            this.btnAddConfig.TabIndex = 0;
            this.btnAddConfig.Text = "添加配置";
            this.btnAddConfig.UseVisualStyleBackColor = true;
            this.btnAddConfig.Click += new System.EventHandler(this.btnAddConfig_Click);
            // 
            // btnBackup
            // 
            this.btnBackup.Location = new System.Drawing.Point(398, 741);
            this.btnBackup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(70, 33);
            this.btnBackup.TabIndex = 7;
            this.btnBackup.Text = "执行备份";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // btnList
            // 
            this.btnList.Location = new System.Drawing.Point(476, 741);
            this.btnList.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnList.Name = "btnList";
            this.btnList.Size = new System.Drawing.Size(70, 33);
            this.btnList.TabIndex = 8;
            this.btnList.Text = "文件列表";
            this.btnList.UseVisualStyleBackColor = true;
            this.btnList.Click += new System.EventHandler(this.btnList_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(14, 17);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(70, 33);
            this.btnSettings.TabIndex = 1;
            this.btnSettings.Text = "工具设置";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 795);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.btnSettings);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple Deployment Tool";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private SplitContainer splitContainer1;
        private GroupBox groupBox1;
        private ListBox lstProjects;
        private Button btnDeleteProject;
        private Button btnEditProject;
        private Button btnAddProject;
        private GroupBox groupBox2;
        private ListBox lstConfigurations;
        private Button btnExecuteBuild;
        private Button btnExecuteUpload;
        private Button btnDeleteConfig;
        private Button btnEditConfig;
        private Button btnAddConfig;
        private Button btnBackup;
        private Button btnSettings;
        private Button btnList;
        #endregion
    }
}
