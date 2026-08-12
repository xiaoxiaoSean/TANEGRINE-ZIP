using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TANEGRINE_ZIP
{
    public partial class OverWriteOrNotForm : Form
    {
        public OverWriteOrNotForm()
        {
            InitializeComponent();
        }
        bool aow = false;
        bool askip = false;
        int output = -1;
        private void OverWriteOrNotForm_Load(object sender, EventArgs e)
        {
            this.Text = LanguageManager.Get("OverWriteOrNot");
            label1.Text = LanguageManager.Get("OverWriteText");
            button1.Text = LanguageManager.Get("Execute");
            listBox1.Items.Clear();
            listBox1.Items.Add(LanguageManager.Get("OverWrite1"));
            listBox1.Items.Add(LanguageManager.Get("OverWrite2"));
            listBox1.Items.Add(LanguageManager.Get("OverWrite3"));
            listBox1.Items.Add(LanguageManager.Get("OverWrite4"));
        }
        public void SyncBool(ref bool allOverWrite)
        {
            allOverWrite = aow;
        }
        public void SyncBool2(ref bool allSkip)
        {
            allSkip = askip;
        }
        public void GetResult(ref int input)
        {
            input = output;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            switch (listBox1.SelectedIndex)
            {
                default:
                    MessageBox.Show(LanguageManager.Get("Suggestion1") + LanguageManager.Get("OverWriteSuggestion1") + LanguageManager.Get("ErrorCode1") + "OWFSIES" + listBox1.SelectedIndex);
                    return;
                case 0://yes
                    output = 1;
                    aow = false;
                    askip = false;
                    break;
                case 1://no
                    output = 2;
                    aow = false;
                    askip = false;
                    break;
                case 2://yes to all
                    output = -99;
                    aow = true;
                    askip = false;
                    break;
                case 3://no to all
                    output = -99;
                    aow = false;
                    askip = true;
                    break;
            }
            this.Close();
        }
    }
}
