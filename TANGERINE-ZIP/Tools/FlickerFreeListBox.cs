using System;
using System.Windows.Forms;

namespace TANGERINE_ZIP
{
    internal sealed class FlickerFreeListBox : ListBox
    {
        private const int WM_ERASEBKGND = 0x0014;

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
        }
    }
}
