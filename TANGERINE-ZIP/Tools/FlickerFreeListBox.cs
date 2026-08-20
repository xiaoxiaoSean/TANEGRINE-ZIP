using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TANGERINE_ZIP
{
    internal sealed class FlickerFreeListBox : ListBox
    {
        private const int WM_ERASEBKGND = 0x0014;

        private const int WM_VSCROLL = 0x0115;

        private const int WM_HSCROLL = 0x0114;

        private const int WM_MOUSEWHEEL = 0x020A;

        private const int WM_KEYDOWN = 0x0100;

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

        public event EventHandler? ViewChanged;

        public FlickerFreeListBox()
        {
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

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);

            if (m.Msg == WM_VSCROLL ||
                m.Msg == WM_HSCROLL ||
                m.Msg == WM_MOUSEWHEEL ||
                m.Msg == WM_KEYDOWN)
            {
                ViewChanged?.Invoke(
                    this,
                    EventArgs.Empty);
            }
        }
    }
}
