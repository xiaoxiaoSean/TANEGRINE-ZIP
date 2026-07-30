namespace TANEGRINE_ZIP
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            mainMenu = new MenuStrip();
            TZIPToolStripMenuItem = new ToolStripMenuItem();
            OpenToolStripMenuItem = new ToolStripMenuItem();
            UnzipToolStripMenuItem = new ToolStripMenuItem();
            ZipToolStripMenuItem = new ToolStripMenuItem();
            SettingsToolStripMenuItem = new ToolStripMenuItem();
            mainTabControl = new TabControl();
            mainTab = new TabPage();
            statusStrip1 = new StatusStrip();
            toolStripProgressBar1 = new ToolStripProgressBar();
            statusLabel = new ToolStripStatusLabel();
            fileBox = new ListBox();
            mainOpenFileDialog = new OpenFileDialog();
            uninstallFileToolStripMenuItem = new ToolStripMenuItem();
            mainMenu.SuspendLayout();
            mainTabControl.SuspendLayout();
            mainTab.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenu
            // 
            mainMenu.ImageScalingSize = new Size(24, 24);
            mainMenu.Items.AddRange(new ToolStripItem[] { TZIPToolStripMenuItem, OpenToolStripMenuItem, UnzipToolStripMenuItem, ZipToolStripMenuItem, SettingsToolStripMenuItem, uninstallFileToolStripMenuItem });
            resources.ApplyResources(mainMenu, "mainMenu");
            mainMenu.Name = "mainMenu";
            // 
            // TZIPToolStripMenuItem
            // 
            TZIPToolStripMenuItem.Name = "TZIPToolStripMenuItem";
            resources.ApplyResources(TZIPToolStripMenuItem, "TZIPToolStripMenuItem");
            // 
            // OpenToolStripMenuItem
            // 
            OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
            resources.ApplyResources(OpenToolStripMenuItem, "OpenToolStripMenuItem");
            OpenToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // UnzipToolStripMenuItem
            // 
            UnzipToolStripMenuItem.Name = "UnzipToolStripMenuItem";
            resources.ApplyResources(UnzipToolStripMenuItem, "UnzipToolStripMenuItem");
            // 
            // ZipToolStripMenuItem
            // 
            ZipToolStripMenuItem.Name = "ZipToolStripMenuItem";
            resources.ApplyResources(ZipToolStripMenuItem, "ZipToolStripMenuItem");
            // 
            // SettingsToolStripMenuItem
            // 
            SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
            resources.ApplyResources(SettingsToolStripMenuItem, "SettingsToolStripMenuItem");
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(mainTab);
            resources.ApplyResources(mainTabControl, "mainTabControl");
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            // 
            // mainTab
            // 
            mainTab.Controls.Add(statusStrip1);
            mainTab.Controls.Add(fileBox);
            resources.ApplyResources(mainTab, "mainTab");
            mainTab.Name = "mainTab";
            mainTab.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(24, 24);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripProgressBar1, statusLabel });
            resources.ApplyResources(statusStrip1, "statusStrip1");
            statusStrip1.Name = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            resources.ApplyResources(toolStripProgressBar1, "toolStripProgressBar1");
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            resources.ApplyResources(statusLabel, "statusLabel");
            // 
            // fileBox
            // 
            resources.ApplyResources(fileBox, "fileBox");
            fileBox.FormattingEnabled = true;
            fileBox.Name = "fileBox";
            // 
            // mainOpenFileDialog
            // 
            mainOpenFileDialog.FileName = "openFileDialog1";
            // 
            // uninstallFileToolStripMenuItem
            // 
            uninstallFileToolStripMenuItem.Name = "uninstallFileToolStripMenuItem";
            resources.ApplyResources(uninstallFileToolStripMenuItem, "uninstallFileToolStripMenuItem");
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainTabControl);
            Controls.Add(mainMenu);
            Name = "Form1";
            Load += Form1_Load;
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            mainTabControl.ResumeLayout(false);
            mainTab.ResumeLayout(false);
            mainTab.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip mainMenu;
        private ToolStripMenuItem TZIPToolStripMenuItem;
        private ToolStripMenuItem UnzipToolStripMenuItem;
        private ToolStripMenuItem ZipToolStripMenuItem;
        private ToolStripMenuItem SettingsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private TabControl mainTabControl;
        private TabPage mainTab;
        private ListBox fileBox;
        private ToolStripMenuItem OpenToolStripMenuItem;
        private OpenFileDialog mainOpenFileDialog;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripStatusLabel statusLabel;
        private ToolStripMenuItem uninstallFileToolStripMenuItem;
    }
}
