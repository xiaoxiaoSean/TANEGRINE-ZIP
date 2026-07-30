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
            extractToolStripMenuItem = new ToolStripMenuItem();
            compressToolStripMenuItem = new ToolStripMenuItem();
            SettingsToolStripMenuItem = new ToolStripMenuItem();
            uninstallFileToolStripMenuItem = new ToolStripMenuItem();
            mainTabControl = new TabControl();
            mainTab = new TabPage();
            fileBox = new ListBox();
            statusStrip1 = new StatusStrip();
            statusProgressBar = new ToolStripProgressBar();
            statusLabel = new ToolStripStatusLabel();
            mainOpenFileDialog = new OpenFileDialog();
            mainMenu.SuspendLayout();
            mainTabControl.SuspendLayout();
            mainTab.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenu
            // 
            mainMenu.ImageScalingSize = new Size(24, 24);
            mainMenu.Items.AddRange(new ToolStripItem[] { TZIPToolStripMenuItem, OpenToolStripMenuItem, extractToolStripMenuItem, compressToolStripMenuItem, SettingsToolStripMenuItem, uninstallFileToolStripMenuItem });
            mainMenu.Location = new Point(0, 0);
            mainMenu.Name = "mainMenu";
            mainMenu.Size = new Size(1374, 32);
            mainMenu.TabIndex = 1;
            // 
            // TZIPToolStripMenuItem
            // 
            TZIPToolStripMenuItem.Name = "TZIPToolStripMenuItem";
            TZIPToolStripMenuItem.Size = new Size(63, 28);
            TZIPToolStripMenuItem.Text = "TZIP";
            // 
            // OpenToolStripMenuItem
            // 
            OpenToolStripMenuItem.Name = "OpenToolStripMenuItem";
            OpenToolStripMenuItem.Size = new Size(16, 28);
            OpenToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
            // 
            // extractToolStripMenuItem
            // 
            extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            extractToolStripMenuItem.Size = new Size(74, 28);
            extractToolStripMenuItem.Text = "解压...";
            // 
            // compressToolStripMenuItem
            // 
            compressToolStripMenuItem.Name = "compressToolStripMenuItem";
            compressToolStripMenuItem.Size = new Size(74, 28);
            compressToolStripMenuItem.Text = "压缩...";
            // 
            // SettingsToolStripMenuItem
            // 
            SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
            SettingsToolStripMenuItem.Size = new Size(62, 28);
            SettingsToolStripMenuItem.Text = "设置";
            // 
            // uninstallFileToolStripMenuItem
            // 
            uninstallFileToolStripMenuItem.Name = "uninstallFileToolStripMenuItem";
            uninstallFileToolStripMenuItem.Size = new Size(182, 28);
            uninstallFileToolStripMenuItem.Text = "移除文件(不是删除)";
            uninstallFileToolStripMenuItem.Click += uninstallFileToolStripMenuItem_Click;
            // 
            // mainTabControl
            // 
            mainTabControl.Controls.Add(mainTab);
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Location = new Point(0, 32);
            mainTabControl.Name = "mainTabControl";
            mainTabControl.SelectedIndex = 0;
            mainTabControl.Size = new Size(1374, 544);
            mainTabControl.TabIndex = 0;
            // 
            // mainTab
            // 
            mainTab.Controls.Add(fileBox);
            mainTab.Location = new Point(4, 33);
            mainTab.Name = "mainTab";
            mainTab.Size = new Size(1366, 507);
            mainTab.TabIndex = 0;
            mainTab.UseVisualStyleBackColor = true;
            // 
            // fileBox
            // 
            fileBox.Dock = DockStyle.Fill;
            fileBox.FormattingEnabled = true;
            fileBox.Location = new Point(0, 0);
            fileBox.Name = "fileBox";
            fileBox.Size = new Size(1366, 507);
            fileBox.TabIndex = 1;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(24, 24);
            statusStrip1.Items.AddRange(new ToolStripItem[] { statusProgressBar, statusLabel });
            statusStrip1.Location = new Point(0, 545);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1374, 31);
            statusStrip1.TabIndex = 0;
            // 
            // statusProgressBar
            // 
            statusProgressBar.Name = "statusProgressBar";
            statusProgressBar.Size = new Size(100, 23);
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(118, 24);
            statusLabel.Text = "软件正在准备";
            // 
            // mainOpenFileDialog
            // 
            mainOpenFileDialog.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1374, 576);
            Controls.Add(statusStrip1);
            Controls.Add(mainTabControl);
            Controls.Add(mainMenu);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "TZIP";
            Load += Form1_Load;
            mainMenu.ResumeLayout(false);
            mainMenu.PerformLayout();
            mainTabControl.ResumeLayout(false);
            mainTab.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip mainMenu;
        private ToolStripMenuItem TZIPToolStripMenuItem;
        private ToolStripMenuItem extractToolStripMenuItem;
        private ToolStripMenuItem compressToolStripMenuItem;
        private ToolStripMenuItem SettingsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private TabControl mainTabControl;
        private TabPage mainTab;
        private ListBox fileBox;
        private ToolStripMenuItem OpenToolStripMenuItem;
        private OpenFileDialog mainOpenFileDialog;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar statusProgressBar;
        private ToolStripStatusLabel statusLabel;
        private ToolStripMenuItem uninstallFileToolStripMenuItem;
    }
}
