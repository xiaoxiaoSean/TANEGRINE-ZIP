using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TANGERINE_ZIP.Tools.LightTool
{
    public sealed class TangerineLightOverlay : Form
    {
        private readonly Form _targetForm;

        /*
         * Render driver
         *
         * System.Threading.Timer does not directly render.
         * It only posts a render request into the WinForms UI message queue.
         *
         * Only one posted tick is allowed to exist at a time.
         * This prevents the render queue from growing when the UI thread
         * is temporarily busy.
         */
        private readonly System.Threading.Timer _renderDriver;
        private readonly SynchronizationContext _syncContext;

        private volatile bool _tickPosted;

        private POINT _mouseScreenPosition;

        private bool _running;
        private bool _mouseInside;

        private float _presentationDelayMs = 0f;

        private int _targetFps = 120;

        private float _radius = 180f;

        private Color _lightColor = Color.White;

        private float _lightStrength = 0.18f;

        private float _edgeStrength = 1.0f;

        private float _edgeWidth = 2.0f;

        private bool _isMouseLight = true;

        private readonly Stopwatch _frameStopwatch = new Stopwatch();

        private double _minFrameMilliseconds = 1000.0 / 120.0;

        private readonly int _processorCount = Environment.ProcessorCount;

        #region Win32

        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private const int WM_NCHITTEST = 0x0084;
        private const int WM_MOUSEACTIVATE = 0x0021;

        /*
         * WM_PRINT asks a window to paint itself into the supplied DC.
         */
        private const int WM_PRINT = 0x0317;

        private const int HTTRANSPARENT = -1;
        private const int MA_NOACTIVATE = 3;

        private const int ULW_ALPHA = 0x00000002;

        private const byte AC_SRC_OVER = 0;
        private const byte AC_SRC_ALPHA = 1;

        private const int PRF_CHECKVISIBLE = 0x00000001;
        private const int PRF_NONCLIENT = 0x00000002;
        private const int PRF_CLIENT = 0x00000004;
        private const int PRF_ERASEBKGND = 0x00000008;
        private const int PRF_CHILDREN = 0x00000010;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(int width, int height)
            {
                cx = width;
                cy = height;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RGBQUAD
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public RGBQUAD bmiColors;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hWnd,
            IntPtr hdcDst,
            ref POINT pptDst,
            ref SIZE psize,
            IntPtr hdcSrc,
            ref POINT pptSrc,
            int crKey,
            ref BLENDFUNCTION pblend,
            int dwFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(
            IntPtr hWnd,
            ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(
            IntPtr hWnd,
            ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(
            IntPtr hWnd,
            out RECT lpRect);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(
            IntPtr hdc,
            IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr ho);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            ref BITMAPINFO pbmi,
            uint iUsage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint dwOffset);

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);

        #endregion

        #region Native Resources

        private IntPtr _sourceDc;
        private IntPtr _sourceBitmap;
        private IntPtr _sourceBits;

        private int _sourceWidth;
        private int _sourceHeight;

        private IntPtr _outputDc;
        private IntPtr _outputBitmap;
        private IntPtr _outputBits;

        private int _outputWidth;
        private int _outputHeight;

        #endregion

        #region Managed Buffers

        private byte[] _sourceBuffer = Array.Empty<byte>();

        private byte[] _outputBuffer = Array.Empty<byte>();

        private float[] _lumaBuffer = Array.Empty<float>();

        private float[] _lightLut = Array.Empty<float>();

        private int _lutRadius;

        #endregion

        #region Frame State

        private bool _hasValidFrame;

        private int _lastOffsetX;
        private int _lastOffsetY;
        private int _lastWidth;
        private int _lastHeight;

        #endregion

        private bool _timerResolutionRaised;

        public TangerineLightOverlay(Form targetForm)
        {
            _targetForm =
                targetForm
                ?? throw new ArgumentNullException(nameof(targetForm));

            _syncContext =
                SynchronizationContext.Current
                ?? new WindowsFormsSynchronizationContext();

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = false;
            TabStop = false;

            _targetForm.Move += TargetForm_Move;
            _targetForm.Resize += TargetForm_Resize;
            _targetForm.FormClosed += TargetForm_FormClosed;

            GetCursorPos(out _mouseScreenPosition);

            CreateNativeResources();

            RebuildLightLut();

            RaiseTimerResolution();

            _frameStopwatch.Start();

            _renderDriver = new System.Threading.Timer(
                PostRenderTick,
                null,
                0,
                1);

            _running = true;
        }

        #region Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TargetFps
        {
            get => _targetFps;

            set
            {
                _targetFps = Math.Clamp(value, 15, 240);

                _minFrameMilliseconds =
                    1000.0 / _targetFps;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Radius
        {
            get => _radius;

            set
            {
                float newValue =
                    Math.Clamp(value, 20f, 1024f);

                if (Math.Abs(_radius - newValue) < 0.001f)
                    return;

                _radius = newValue;

                RebuildLightLut();
                EnsureOutputDib();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color LightColor
        {
            get => _lightColor;
            set => _lightColor = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float LightStrength
        {
            get => _lightStrength;

            set =>
                _lightStrength =
                    Math.Clamp(value, 0f, 1f);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float EdgeStrength
        {
            get => _edgeStrength;

            set =>
                _edgeStrength =
                    Math.Clamp(value, 0f, 2f);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float EdgeWidth
        {
            get => _edgeWidth;

            set =>
                _edgeWidth =
                    Math.Clamp(value, 0.5f, 6f);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMouseLight
        {
            get => _isMouseLight;
            set => _isMouseLight = value;
        }

        /*
         * 保留这个属性以兼容你原来的调用代码。
         *
         * 这里不再用于鼠标坐标预测。
         * 因为预测本身不能解决动态控件捕获时序造成的画面错位，
         * 反而会制造另外一种光效偏移。
         */
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float PresentationDelayMs
        {
            get => _presentationDelayMs;

            set =>
                _presentationDelayMs =
                    Math.Max(0f, value);
        }

        #endregion

        #region Window

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                cp.ExStyle |=
                    WS_EX_LAYERED |
                    WS_EX_TRANSPARENT |
                    WS_EX_TOOLWINDOW |
                    WS_EX_NOACTIVATE;

                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result =
                    (IntPtr)HTTRANSPARENT;

                return;
            }

            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result =
                    (IntPtr)MA_NOACTIVATE;

                return;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Render Driver

        private void PostRenderTick(object? state)
        {
            if (!_running)
                return;

            if (_tickPosted)
                return;

            _tickPosted = true;

            _syncContext.Post(
                RunTick,
                null);
        }

        private void RunTick(object? state)
        {
            _tickPosted = false;

            if (IsDisposed)
                return;

            Timer_TickCore();
        }

        private void Timer_TickCore()
        {
            if (!_running)
                return;

            if (_targetForm.IsDisposed)
                return;

            if (!_targetForm.IsHandleCreated)
                return;

            if (!GetCursorPos(out _mouseScreenPosition))
                return;

            /*
             * 鼠标坐标永远只做一次转换：
             *
             * screen
             *   ↓
             * target client
             *
             * 后面的 Render 全部使用这个 client 坐标。
             */
            POINT mouseClient =
                _mouseScreenPosition;

            if (!ScreenToClient(
                    _targetForm.Handle,
                    ref mouseClient))
            {
                return;
            }

            if (!GetClientRect(
                    _targetForm.Handle,
                    out RECT rc))
            {
                return;
            }

            int clientWidth =
                rc.Right - rc.Left;

            int clientHeight =
                rc.Bottom - rc.Top;

            if (clientWidth <= 0 ||
                clientHeight <= 0)
            {
                return;
            }

            bool inside =
                mouseClient.X >= 0 &&
                mouseClient.Y >= 0 &&
                mouseClient.X < clientWidth &&
                mouseClient.Y < clientHeight;

            if (!inside)
            {
                if (_mouseInside)
                {
                    _mouseInside = false;

                    ClearOverlay();
                }

                return;
            }

            _mouseInside = true;

            if (_frameStopwatch.Elapsed.TotalMilliseconds <
                _minFrameMilliseconds)
            {
                return;
            }

            _frameStopwatch.Restart();

            if (!Visible)
                Show(_targetForm);

            /*
             * 这里故意不再进行 PredictMouseClient。
             *
             * 光效必须使用“本次捕获对应的鼠标位置”。
             * 否则动态控件移动时，预测坐标和 WM_PRINT 捕获帧
             * 不属于同一个时间点，会形成额外的空间偏移。
             */
            Render(
                mouseClient,
                clientWidth,
                clientHeight);
        }

        #endregion

        #region Native Resource Creation

        private void CreateNativeResources()
        {
            if (_sourceDc == IntPtr.Zero)
                _sourceDc =
                    CreateCompatibleDC(IntPtr.Zero);

            if (_outputDc == IntPtr.Zero)
                _outputDc =
                    CreateCompatibleDC(IntPtr.Zero);

            EnsureOutputDib();
        }

        private int GetOutputBufferSize()
        {
            int radius =
                (int)Math.Ceiling(_radius);

            return radius * 2 + 1;
        }

        private void EnsureOutputDib()
        {
            int size =
                GetOutputBufferSize();

            if (_outputBitmap != IntPtr.Zero &&
                _outputWidth == size &&
                _outputHeight == size)
            {
                EnsureManagedBuffers(size);
                return;
            }

            if (!ReplaceDib(
                    ref _outputBitmap,
                    ref _outputBits,
                    _outputDc,
                    size,
                    size))
            {
                return;
            }

            _outputWidth = size;
            _outputHeight = size;

            EnsureManagedBuffers(size);
        }

        private void EnsureSourceDib(
            int width,
            int height)
        {
            if (width <= 0 ||
                height <= 0)
            {
                return;
            }

            if (_sourceBitmap != IntPtr.Zero &&
                _sourceWidth == width &&
                _sourceHeight == height)
            {
                return;
            }

            if (!ReplaceDib(
                    ref _sourceBitmap,
                    ref _sourceBits,
                    _sourceDc,
                    width,
                    height))
            {
                return;
            }

            _sourceWidth = width;
            _sourceHeight = height;

            int required =
                checked(width * height * 4);

            if (_sourceBuffer.Length < required)
                _sourceBuffer =
                    new byte[required];

            if (_lumaBuffer.Length <
                width * height)
            {
                _lumaBuffer =
                    new float[width * height];
            }
        }

        private void EnsureManagedBuffers(int size)
        {
            int required =
                checked(size * size * 4);

            if (_outputBuffer.Length < required)
                _outputBuffer =
                    new byte[required];

            if (_sourceBuffer.Length < required)
                _sourceBuffer =
                    new byte[required];

            int lumaRequired =
                checked(size * size);

            if (_lumaBuffer.Length <
                lumaRequired)
            {
                _lumaBuffer =
                    new float[lumaRequired];
            }
        }

        private bool ReplaceDib(
            ref IntPtr bitmap,
            ref IntPtr bits,
            IntPtr dc,
            int width,
            int height)
        {
            if (dc == IntPtr.Zero)
                return false;

            BITMAPINFO info =
                new BITMAPINFO();

            info.bmiHeader.biSize =
                (uint)Marshal.SizeOf<BITMAPINFOHEADER>();

            info.bmiHeader.biWidth =
                width;

            info.bmiHeader.biHeight =
                -height;

            info.bmiHeader.biPlanes = 1;
            info.bmiHeader.biBitCount = 32;
            info.bmiHeader.biCompression = 0;

            info.bmiHeader.biSizeImage =
                (uint)(width * height * 4);

            IntPtr newBitmap =
                CreateDIBSection(
                    IntPtr.Zero,
                    ref info,
                    0,
                    out IntPtr newBits,
                    IntPtr.Zero,
                    0);

            if (newBitmap == IntPtr.Zero)
                return false;

            /*
             * 先把新 bitmap 选进 DC。
             *
             * 旧 bitmap 可以安全删除。
             */
            IntPtr oldBitmap =
                SelectObject(
                    dc,
                    newBitmap);

            if (oldBitmap != IntPtr.Zero &&
                oldBitmap != newBitmap)
            {
                DeleteObject(oldBitmap);
            }

            bitmap = newBitmap;
            bits = newBits;

            return true;
        }

        private void RebuildLightLut()
        {
            int radius =
                (int)Math.Ceiling(_radius);

            _lutRadius = radius;

            int radiusSquared =
                checked(radius * radius);

            _lightLut =
                new float[radiusSquared + 1];

            for (int d2 = 0;
                 d2 <= radiusSquared;
                 d2++)
            {
                float normalized =
                    MathF.Sqrt(d2) / _radius;

                _lightLut[d2] =
                    SmoothStep(
                        1f - normalized);
            }
        }

        private void RaiseTimerResolution()
        {
            if (_timerResolutionRaised)
                return;

            timeBeginPeriod(1);

            _timerResolutionRaised = true;
        }

        private void LowerTimerResolution()
        {
            if (!_timerResolutionRaised)
                return;

            timeEndPeriod(1);

            _timerResolutionRaised = false;
        }

        #endregion

        #region Target Form

        private void TargetForm_Move(
            object? sender,
            EventArgs e)
        {
            if (!_hasValidFrame)
                return;

            if (!IsHandleCreated)
                return;

            /*
             * 这里不重新捕获、不重新计算。
             *
             * 旧 frame 的坐标是 target client 坐标。
             * target 移动后，只需要重新求 client (0,0)
             * 对应的 screen 坐标。
             */
            UpdateLayeredWindowImage(
                _lastOffsetX,
                _lastOffsetY,
                _lastWidth,
                _lastHeight);
        }

        private void TargetForm_Resize(
            object? sender,
            EventArgs e)
        {
            /*
             * 下一帧自动根据新的 client size 重建 source DIB。
             */
        }

        private void TargetForm_FormClosed(
            object? sender,
            FormClosedEventArgs e)
        {
            _running = false;

            if (!IsDisposed)
                Close();
        }

        #endregion

        #region Render

        private void Render(
            POINT mouse,
            int clientWidth,
            int clientHeight)
        {
            EnsureOutputDib();

            EnsureSourceDib(
                clientWidth,
                clientHeight);

            if (_sourceBits == IntPtr.Zero ||
                _outputBits == IntPtr.Zero)
            {
                return;
            }

            if (clientWidth > _sourceWidth ||
                clientHeight > _sourceHeight)
            {
                return;
            }

            int radius = _lutRadius;

            int left =
                Math.Max(
                    0,
                    mouse.X - radius);

            int top =
                Math.Max(
                    0,
                    mouse.Y - radius);

            int right =
                Math.Min(
                    clientWidth - 1,
                    mouse.X + radius);

            int bottom =
                Math.Min(
                    clientHeight - 1,
                    mouse.Y + radius);

            int width =
                right - left + 1;

            int height =
                bottom - top + 1;

            if (width < 3 ||
                height < 3)
            {
                ClearOverlay();
                return;
            }

            if (width > _outputWidth ||
                height > _outputHeight)
            {
                return;
            }

            /*
             * ---------------------------------------------------------
             * 关键点：
             *
             * WM_PRINT 的 source DC 是完整 target client DC。
             *
             * 不对 source DC 做任何额外 TranslateViewportOrgEx、
             * SetWindowOrgEx 或屏幕坐标转换。
             *
             * 因此：
             *
             * source pixel (x,y)
             * =
             * target client pixel (x,y)
             *
             * 后面的 left/top 只是从这个完整 client 坐标系中
             * 截取光效区域。
             * ---------------------------------------------------------
             */
            CaptureTarget();

            int pixelCount =
                width * height;

            int requiredBytes =
                pixelCount * 4;

            int sourceStride =
                _sourceWidth * 4;

            /*
             * source DIB:
             *
             * [0,0] = target client [0,0]
             *
             * 所以：
             *
             * source[(top+y),(left+x)]
             * =
             * target client[(top+y),(left+x)]
             */
            for (int row = 0;
                 row < height;
                 row++)
            {
                IntPtr sourceRow =
                    IntPtr.Add(
                        _sourceBits,
                        (top + row) *
                            sourceStride +
                        left * 4);

                Marshal.Copy(
                    sourceRow,
                    _sourceBuffer,
                    row * width * 4,
                    width * 4);
            }

            void FillLumaStrip(
                int yStart,
                int yEndExclusive)
            {
                for (int y = yStart;
                     y < yEndExclusive;
                     y++)
                {
                    int pixelRow =
                        y * width;

                    int byteRow =
                        pixelRow * 4;

                    for (int x = 0;
                         x < width;
                         x++)
                    {
                        int p =
                            byteRow +
                            x * 4;

                        _lumaBuffer[
                            pixelRow + x] =
                            _sourceBuffer[p + 2] *
                                0.299f +
                            _sourceBuffer[p + 1] *
                                0.587f +
                            _sourceBuffer[p] *
                                0.114f;
                    }
                }
            }

            RunInStrips(
                0,
                height,
                FillLumaStrip);

            Array.Clear(
                _outputBuffer,
                0,
                requiredBytes);

            int mouseX =
                mouse.X - left;

            int mouseY =
                mouse.Y - top;

            int radiusSquared =
                radius * radius;

            byte colorB =
                _lightColor.B;

            byte colorG =
                _lightColor.G;

            byte colorR =
                _lightColor.R;

            bool useEdges =
                _edgeStrength > 0.0001f;

            float edgeWidthFactor = 1f;

            if (_edgeWidth > 1f)
            {
                edgeWidthFactor =
                    Math.Clamp(
                        1f / _edgeWidth + 0.5f,
                        0.5f,
                        1f);
            }

            void RenderRowStrip(
                int yStart,
                int yEndExclusive)
            {
                for (int y = yStart;
                     y < yEndExclusive;
                     y++)
                {
                    int dy =
                        y - mouseY;

                    int dySquared =
                        dy * dy;

                    if (dySquared >
                        radiusSquared)
                    {
                        continue;
                    }

                    int span =
                        (int)MathF.Sqrt(
                            radiusSquared -
                            dySquared);

                    int startX =
                        Math.Max(
                            1,
                            mouseX - span);

                    int endX =
                        Math.Min(
                            width - 2,
                            mouseX + span);

                    int rowBase =
                        y * width;

                    for (int x = startX;
                         x <= endX;
                         x++)
                    {
                        int dx =
                            x - mouseX;

                        int distanceSquared =
                            dx * dx +
                            dySquared;

                        if (distanceSquared >
                            radiusSquared)
                        {
                            continue;
                        }

                        float light =
                            _lightLut[
                                distanceSquared];

                        if (light <= 0.0001f)
                            continue;

                        float alpha = 0f;

                        if (_isMouseLight)
                        {
                            alpha =
                                light *
                                _lightStrength;
                        }

                        if (useEdges)
                        {
                            int index =
                                rowBase + x;

                            float edge =
                                CalculateSobelEdge(
                                    _lumaBuffer,
                                    width,
                                    index);

                            if (edge > 0f)
                            {
                                float edgeAlpha =
                                    edge *
                                    _edgeStrength *
                                    light;

                                edgeAlpha *=
                                    edgeWidthFactor;

                                alpha += edgeAlpha;
                            }
                        }

                        if (alpha <= 0.001f)
                            continue;

                        if (alpha > 1f)
                            alpha = 1f;

                        int outputIndex =
                            (rowBase + x) * 4;

                        /*
                         * Premultiplied alpha.
                         */
                        _outputBuffer[
                            outputIndex] =
                            (byte)(
                                colorB *
                                alpha);

                        _outputBuffer[
                            outputIndex + 1] =
                            (byte)(
                                colorG *
                                alpha);

                        _outputBuffer[
                            outputIndex + 2] =
                            (byte)(
                                colorR *
                                alpha);

                        _outputBuffer[
                            outputIndex + 3] =
                            (byte)(
                                alpha * 255f);
                    }
                }
            }

            RunInStrips(
                1,
                height - 1,
                RenderRowStrip);

            int outputStride =
                _outputWidth * 4;

            for (int row = 0;
                 row < height;
                 row++)
            {
                IntPtr destinationRow =
                    IntPtr.Add(
                        _outputBits,
                        row * outputStride);

                Marshal.Copy(
                    _outputBuffer,
                    row * width * 4,
                    destinationRow,
                    width * 4);
            }

            /*
             * 这里保存的是 target CLIENT 坐标。
             *
             * 不是 screen 坐标。
             * 不是 overlay 坐标。
             */
            _lastOffsetX = left;
            _lastOffsetY = top;

            _lastWidth = width;
            _lastHeight = height;

            _hasValidFrame = true;

            UpdateLayeredWindowImage(
                left,
                top,
                width,
                height);
        }

        private void RunInStrips(
            int begin,
            int endExclusive,
            Action<int, int> stripAction)
        {
            int rowCount =
                endExclusive - begin;

            if (rowCount <= 0)
                return;

            int strips =
                Math.Min(
                    _processorCount,
                    Math.Max(
                        1,
                        rowCount / 16));

            if (strips <= 1)
            {
                stripAction(
                    begin,
                    endExclusive);

                return;
            }

            int rowsPerStrip =
                (rowCount + strips - 1) /
                strips;

            Parallel.For(
                0,
                strips,
                strip =>
                {
                    int start =
                        begin +
                        strip * rowsPerStrip;

                    int end =
                        Math.Min(
                            endExclusive,
                            start + rowsPerStrip);

                    if (start < end)
                    {
                        stripAction(
                            start,
                            end);
                    }
                });
        }

        #endregion

        #region Capture

        private void CaptureTarget()
        {
            /*
             * 直接让 targetForm 将当前 client 内容绘制到 source DC。
             *
             * 这里绝对不调用：
             *
             * RedrawWindow
             * Invalidate
             * Update
             * Refresh
             *
             * 否则动态 ListBox / 自绘控件可能出现闪烁。
             *
             * WM_PRINT 是同步调用：
             * SendMessage 返回以后，source DIB 才被认为是本帧
             * 可以使用的数据。
             */
            SendMessage(
                _targetForm.Handle,
                WM_PRINT,
                _sourceDc,
                (IntPtr)(
                    PRF_CHECKVISIBLE |
                    PRF_CLIENT |
                    PRF_ERASEBKGND |
                    PRF_CHILDREN));
        }

        #endregion

        #region Sobel

        private static float CalculateSobelEdge(
            float[] luma,
            int width,
            int index)
        {
            float gx =
                luma[index - width + 1] -
                luma[index - width - 1] +

                2f *
                (
                    luma[index + 1] -
                    luma[index - 1]
                ) +

                luma[index + width + 1] -
                luma[index + width - 1];

            float gy =
                luma[index + width - 1] -
                luma[index - width - 1] +

                2f *
                (
                    luma[index + width] -
                    luma[index - width]
                ) +

                luma[index + width + 1] -
                luma[index - width + 1];

            float magnitudeSquared =
                gx * gx +
                gy * gy;

            const float threshold = 0.10f;
            const float thresholdSquared = 10404f;

            if (magnitudeSquared <=
                thresholdSquared)
            {
                return 0f;
            }

            float magnitude =
                MathF.Sqrt(
                    magnitudeSquared);

            float edge =
                magnitude / 1020f;

            edge =
                (edge - threshold) /
                (1f - threshold);

            edge *= edge;

            return Math.Clamp(
                edge,
                0f,
                1f);
        }

        #endregion

        #region Math

        private static float SmoothStep(
            float value)
        {
            value =
                Math.Clamp(
                    value,
                    0f,
                    1f);

            return
                value *
                value *
                (3f - 2f * value);
        }

        #endregion

        #region Layered Window

        private void ClearOverlay()
        {
            if (!IsHandleCreated)
                return;

            if (_outputBits == IntPtr.Zero ||
                _outputBuffer.Length < 4)
            {
                return;
            }

            _outputBuffer[0] = 0;
            _outputBuffer[1] = 0;
            _outputBuffer[2] = 0;
            _outputBuffer[3] = 0;

            Marshal.Copy(
                _outputBuffer,
                0,
                _outputBits,
                4);

            _hasValidFrame = false;

            UpdateLayeredWindowImage(
                0,
                0,
                1,
                1);
        }

        private void UpdateLayeredWindowImage(
            int offsetX,
            int offsetY,
            int width,
            int height)
        {
            if (!IsHandleCreated)
                return;

            if (_outputDc == IntPtr.Zero ||
                _outputBitmap == IntPtr.Zero)
            {
                return;
            }

            /*
             * 每次更新时重新获取 target client (0,0)
             * 的屏幕坐标。
             *
             * 这样 targetForm 移动以后不会继续使用旧的
             * screen 坐标。
             */
            POINT targetClientOrigin =
                new POINT(0, 0);

            if (!ClientToScreen(
                    _targetForm.Handle,
                    ref targetClientOrigin))
            {
                return;
            }

            POINT destination =
                new POINT(
                    targetClientOrigin.X +
                    offsetX,

                    targetClientOrigin.Y +
                    offsetY);

            POINT source =
                new POINT(0, 0);

            SIZE size =
                new SIZE(
                    width,
                    height);

            BLENDFUNCTION blend =
                new BLENDFUNCTION
                {
                    BlendOp =
                        AC_SRC_OVER,

                    BlendFlags = 0,

                    SourceConstantAlpha = 255,

                    AlphaFormat =
                        AC_SRC_ALPHA
                };

            IntPtr screenDc =
                GetDC(IntPtr.Zero);

            if (screenDc == IntPtr.Zero)
                return;

            try
            {
                UpdateLayeredWindow(
                    Handle,
                    screenDc,
                    ref destination,
                    ref size,
                    _outputDc,
                    ref source,
                    0,
                    ref blend,
                    ULW_ALPHA);
            }
            finally
            {
                ReleaseDC(
                    IntPtr.Zero,
                    screenDc);
            }
        }

        #endregion

        #region Start Stop

        public void Start()
        {
            if (IsDisposed)
                return;

            _running = true;

            RaiseTimerResolution();

            _frameStopwatch.Restart();

            if (!Visible)
                Show(_targetForm);
        }

        public void Stop()
        {
            _running = false;

            LowerTimerResolution();

            ClearOverlay();
        }

        #endregion

        #region Dispose

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _running = false;

                _renderDriver.Dispose();

                _targetForm.Move -=
                    TargetForm_Move;

                _targetForm.Resize -=
                    TargetForm_Resize;

                _targetForm.FormClosed -=
                    TargetForm_FormClosed;

                LowerTimerResolution();

                /*
                 * 这里必须先把 bitmap 从 DC 中换出去，
                 * 再删除 bitmap。
                 *
                 * 原来的代码：
                 *
                 * DeleteDC()
                 * DeleteObject(bitmap)
                 *
                 * 这个资源释放顺序是不严谨的。
                 */

                if (_sourceDc != IntPtr.Zero &&
                    _sourceBitmap != IntPtr.Zero)
                {
                    SelectObject(
                        _sourceDc,
                        IntPtr.Zero);
                }

                if (_outputDc != IntPtr.Zero &&
                    _outputBitmap != IntPtr.Zero)
                {
                    SelectObject(
                        _outputDc,
                        IntPtr.Zero);
                }

                if (_sourceBitmap != IntPtr.Zero)
                {
                    DeleteObject(
                        _sourceBitmap);

                    _sourceBitmap =
                        IntPtr.Zero;

                    _sourceBits =
                        IntPtr.Zero;
                }

                if (_outputBitmap != IntPtr.Zero)
                {
                    DeleteObject(
                        _outputBitmap);

                    _outputBitmap =
                        IntPtr.Zero;

                    _outputBits =
                        IntPtr.Zero;
                }

                if (_sourceDc != IntPtr.Zero)
                {
                    DeleteDC(
                        _sourceDc);

                    _sourceDc =
                        IntPtr.Zero;
                }

                if (_outputDc != IntPtr.Zero)
                {
                    DeleteDC(
                        _outputDc);

                    _outputDc =
                        IntPtr.Zero;
                }

                _sourceBuffer =
                    Array.Empty<byte>();

                _outputBuffer =
                    Array.Empty<byte>();

                _lumaBuffer =
                    Array.Empty<float>();

                _lightLut =
                    Array.Empty<float>();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}