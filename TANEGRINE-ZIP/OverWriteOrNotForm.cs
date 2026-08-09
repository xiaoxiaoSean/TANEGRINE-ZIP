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
        private void OverWriteOrNotForm_Load(object sender, EventArgs e)
        {
            this.Text = LanguageManager.Get("OverWriteOrNot");
            label1.Text = LanguageManager.Get("OverWriteText");
        }
        void syncBool(ref bool allOverWrite)
        {
            allOverWrite = aow;
        }
    }
}
