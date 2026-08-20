using System;
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

        public event EventHandler? ViewChanged;

        public FlickerFreeListBox()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
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
