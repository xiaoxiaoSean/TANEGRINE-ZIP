using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TANGERINE_ZIP.Tools.LightTool;
using TANGERINE_ZIP.Resources;
namespace TANGERINE_ZIP
{
    public partial class TZIPForm : Form
    {
        private TangerineLightControl? tangerineLightControl;
        public TZIPForm()
        {
            InitializeComponent();
        }
        private async void TZIPForm_Load(object sender, EventArgs e)
        {
            this.WindowState= FormWindowState.Maximized;
            tangerineLightControl =
               new TangerineLightControl();
            tangerineLightControl.BackColor = Color.Black;
            tangerineLightControl.Dock =
                DockStyle.Fill;
            tangerineLightControl.LightRadius =
               500f;

            tangerineLightControl.GlowStrength =
                7.0f;
            tangerineLightControl.EdgeThickness = 10;
            tangerineLightControl.GlowColor =
                Color.White;

            tangerineLightControl.SetImage(
                TZIPResource.Kiro
            );

            Controls.Add(
                tangerineLightControl
            );

            tangerineLightControl.BringToFront();
            await Task.Run( () => switchColor());
        }
        private async Task switchColor()
        {
            await Task.Run(() =>
            {
                
                while (true)
                {
                    tangerineLightControl.GlowColor = Color.FromArgb(
                        Random.Shared.Next(256),
                        Random.Shared.Next(256),
                        Random.Shared.Next(256)
                    );
                    Thread.Sleep(100);
                }
            });
        }
    }
}
