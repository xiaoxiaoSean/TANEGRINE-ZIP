using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TANGERINE_ZIP.Tools.LightTool;

namespace TANGERINE_ZIP
{
    public partial class TZIPForm : Form
    {
        private TangerineLightControl? tangerineLightControl;
        public TZIPForm()
        {
            InitializeComponent();
        }
        private void TZIPForm_Load(object sender, EventArgs e)
        {
            tangerineLightControl =
               new TangerineLightControl();

            tangerineLightControl.Dock =
                DockStyle.Fill;

            tangerineLightControl.LightRadius =
                150f;

            tangerineLightControl.GlowStrength =
                2.0f;

            tangerineLightControl.GlowColor =
                Color.White;

            tangerineLightControl.SetImage(
                TZIPResource.Kiro
            );

            Controls.Add(
                tangerineLightControl
            );

            tangerineLightControl.BringToFront();
        }
    }
}
