using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TANGERINE_ZIP
{
    internal sealed class DarkTabControl : TabControl
    {
        private const int WM_ERASEBKGND = 0x0014;

        private const int WM_PAINT = 0x000F;

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(
            IntPtr hWnd,
            string pszSubAppName,
            string pszSubIdList);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int dwAttribute,
            ref int pvAttribute,
            int cbAttribute);

        public DarkTabControl()
        {
            DrawMode = TabDrawMode.OwnerDrawFixed;
            BackColor = Color.Black;
            ForeColor = Color.White;
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnHandleCreated(
            EventArgs e)
        {
            base.OnHandleCreated(e);

            int darkMode = 1;

            DwmSetWindowAttribute(
                Handle,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref darkMode,
                sizeof(int));

            SetWindowTheme(
                Handle,
                "DarkMode_Explorer",
                null!);
        }

        protected override void OnControlAdded(
            ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e.Control is TabPage page)
            {
                page.BackColor = Color.Black;
                page.ForeColor = Color.White;
                page.UseVisualStyleBackColor = false;
            }
        }

        protected override void OnDrawItem(
            DrawItemEventArgs e)
        {
            if (e.Index < 0 ||
                e.Index >= TabPages.Count)
            {
                return;
            }

            Rectangle bounds =
                GetTabRect(e.Index);

            bool selected =
                e.Index == SelectedIndex;

            using Brush background =
                new SolidBrush(
                    selected
                        ? Color.FromArgb(64, 64, 64)
                        : Color.Black);

            e.Graphics.FillRectangle(
                background,
                bounds);

            TextRenderer.DrawText(
                e.Graphics,
                TabPages[e.Index].Text,
                Font,
                bounds,
                Color.White,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }

        protected override void OnPaintBackground(
            PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
        }

        protected override void WndProc(
            ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_PAINT &&
                IsHandleCreated)
            {
                using Graphics graphics =
                    Graphics.FromHwnd(Handle);

                Rectangle display =
                    GetDisplayRect();

                display.Inflate(1, 1);

                using Pen border =
                    new Pen(Color.Black, 2f);

                graphics.DrawRectangle(
                    border,
                    display);
            }
        }
    }
}
