using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TANGERINE_ZIP.Tools;
//first 5 letters of stage code:FFPCF
namespace TANGERINE_ZIP
{
    public partial class FreeFilePickerForm : Form
    {
        public FreeFilePickerForm()
        {
            InitializeComponent();
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
                fileListBox.Items.AddRange(allItems);
            }
            finally
            {
                fileListBox.EndUpdate();
            }
        }
        private void FreeFilePickerForm_Load(object sender, EventArgs e)
        {
            confirmButton.Text=LanguageManager.Get("Confirm");
        }
    }
}
