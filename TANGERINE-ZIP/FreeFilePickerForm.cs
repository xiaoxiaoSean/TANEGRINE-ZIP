using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using TANGERINE_ZIP.Tools;
//first 5 letters of stage code:FFPCF
namespace TANGERINE_ZIP
{
    public partial class FreeFilePickerForm : Form
    {
        private string? currentPath;

        public FreeFilePickerForm()
        {
            InitializeComponent();
            fileListBox.DoubleClick += FileListBox_DoubleClick;
        }
        public void ShowTipText(string inputTip)
        {
            formTipText.Text = inputTip;
        }
        public void InputPath(string inputPath)
        {
            LoadPath(inputPath);
        }
        void LoadPath(string inputPath)
        {
            fileListBox.Items.Clear();            
string[] files = null;
            string[] folders=null;
            try
            {
                files = Directory.GetFiles(inputPath);
            }
            catch (Exception ex)//FFPCF01
            {
                MessageBox.Show(MessageTipGenerator.GenerateTip("FFPCF01", ex.Message));
                return;
            }
            try
            {
                folders = Directory.GetDirectories(inputPath);
            }
            catch (Exception ex)//FFPCF02
            {
                MessageBox.Show(MessageTipGenerator.GenerateTip("FFPCF02", ex.Message));
                return;
            }           
            string[] allItems = PathSorter.MergeAndSort(files, folders);

            fileListBox.BeginUpdate();

            try
            {
                fileListBox.Items.Clear();
                fileListBox.Items.Add(LanguageManager.Get("goToParentDirectoryText") + "...");
                foreach (string item in allItems)
                {
                    fileListBox.Items.Add(Path.GetFileName(item));
                }
            }
            finally
            {
                fileListBox.EndUpdate();
            }

            currentPath = inputPath;
        }

        private void LoadDrives()
        {
            fileListBox.Items.Clear();            
DriveInfo[] drives;
            try
            {
                drives = DriveInfo.GetDrives();
            }
            catch (Exception ex)//FFPCF01
            {
                MessageBox.Show(MessageTipGenerator.GenerateTip("FFPCF01", ex.Message));
                return;
            }

            fileListBox.BeginUpdate();
            try
            {
                fileListBox.Items.Clear();
                foreach (DriveInfo drive in drives)
                {
                    fileListBox.Items.Add(drive.Name);
                }
            }
            finally
            {
                fileListBox.EndUpdate();
            }

            currentPath = null;
        }

        private void FileListBox_DoubleClick(object? sender, EventArgs e)
        {
            if (fileListBox.SelectedItem is not string selectedItem)
            {
                return;
            }

            if (currentPath == null)
            {
                LoadPath(selectedItem);
                return;
            }

            string parentItem = LanguageManager.Get("goToParentDirectoryText") + "...";
            if (selectedItem == parentItem)
            {
                string? parentPath = Directory.GetParent(currentPath)?.FullName;
                if (parentPath == null)
                {
                    LoadDrives();
                }
                else
                {
                    LoadPath(parentPath);
                }
                return;
            }

            string selectedPath = Path.Combine(currentPath, selectedItem);
            if (Directory.Exists(selectedPath))
            {
                LoadPath(selectedPath);
            }
        }
        private void FreeFilePickerForm_Load(object sender, EventArgs e)
        {
            confirmButton.Text=LanguageManager.Get("Confirm");
            LoadDrives();
        }
    }
}
