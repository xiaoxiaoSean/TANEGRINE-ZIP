using TANGERINE_ZIP.Tools.TControls1;
using TANGERINE_ZIP.Resources;
namespace TANGERINE_ZIP
{
    partial class FreeFilePickerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FreeFilePickerForm));
            tableLayoutPanel1 = new TableLayoutPanel();
            formTipText = new TextBox();
            fileListBox = new FlickerFreeListBox();
            confirmButton = new Button();
            logoBox = new PictureBox();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoBox).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 83.77224F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.2277584F));
            tableLayoutPanel1.Controls.Add(formTipText, 0, 0);
            tableLayoutPanel1.Controls.Add(fileListBox, 0, 1);
            tableLayoutPanel1.Controls.Add(confirmButton, 0, 2);
            tableLayoutPanel1.Controls.Add(logoBox, 1, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 22.8624535F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 77.13754F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 154F));
            tableLayoutPanel1.Size = new Size(1405, 693);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // formTipText
            // 
            formTipText.BackColor = Color.Black;
            formTipText.Dock = DockStyle.Fill;
            formTipText.ForeColor = Color.White;
            formTipText.Location = new Point(3, 3);
            formTipText.Multiline = true;
            formTipText.Name = "formTipText";
            formTipText.Size = new Size(1171, 117);
            formTipText.TabIndex = 0;
            // 
            // fileListBox
            // 
            fileListBox.Dock = DockStyle.Fill;
            fileListBox.FormattingEnabled = true;
            fileListBox.Location = new Point(3, 126);
            fileListBox.Name = "fileListBox";
            fileListBox.SelectionMode = SelectionMode.MultiSimple;
            fileListBox.Size = new Size(1171, 409);
            fileListBox.TabIndex = 1;
            // 
            // confirmButton
            // 
            confirmButton.BackColor = Color.Black;
            confirmButton.Dock = DockStyle.Fill;
            confirmButton.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            confirmButton.ForeColor = Color.White;
            confirmButton.Location = new Point(3, 541);
            confirmButton.Name = "confirmButton";
            confirmButton.Size = new Size(1171, 149);
            confirmButton.TabIndex = 2;
            confirmButton.Text = "Confirm";
            confirmButton.UseVisualStyleBackColor = false;
            // 
            // logoBox
            // 
            logoBox.Dock = DockStyle.Fill;
            logoBox.Image = TZIPResource.TZIP;
            logoBox.Location = new Point(1180, 3);
            logoBox.Name = "logoBox";
            logoBox.Size = new Size(222, 117);
            logoBox.SizeMode = PictureBoxSizeMode.Zoom;
            logoBox.TabIndex = 3;
            logoBox.TabStop = false;
            // 
            // FreeFilePickerForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1405, 693);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FreeFilePickerForm";
            Text = "FreeFilePickerForm";
            Load += FreeFilePickerForm_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)logoBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TextBox formTipText;
        private Button confirmButton;
        private PictureBox logoBox;
        private FlickerFreeListBox fileListBox;
    }
}