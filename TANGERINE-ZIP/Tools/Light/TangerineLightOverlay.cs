using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace TANGERINE_ZIP.Tools.LightTool
{
    public sealed class TangerineLightOverlay : Form
    {
        private readonly Form _targetForm;
        private readonly SynchronizationContext _syncContext;
        private readonly System.Threading.Timer _renderDriver;

        private volatile bool _tickPosted;
        private volatile bool _running;

        private POINT _mouseScreenPosition;

        private bool _mouseInside;

        private float _presentationDelayMs = 0f;

        private int _targetFps = 120;

        private float _radius = 180f;

        private Color _lightColor = Color.White;

        private float _lightStrength = 0.18f;

        private float _edgeStrength = 1.0f;

        private float _edgeWidth = 2.0f;

        private bool _isMouseLight = true;

        private readonly Stopwatch _frameStopwatch =
            new Stopwatch();

        private double _minFrameMilliseconds =
            1000.0 / 120.0;

        private bool _timerResolutionRaised;

        #region Win32 Constants

        private const int WS_EX_LAYERED =
            0x00080000;

        private const int WS_EX_TRANSPARENT =
            0x00000020;

        private const int WS_EX_TOOLWINDOW =
            0x00000080;

        private const int WS_EX_NOACTIVATE =
            0x08000000;

        private const int WM_NCHITTEST =
            0x0084;

        private const int WM_MOUSEACTIVATE =
            0x0021;

        private const int WM_PRINT =
            0x0317;

        private const int HTTRANSPARENT =
            -1;

        private const int MA_NOACTIVATE =
            3;

        private const int ULW_ALPHA =
            0x00000002;

        private const byte AC_SRC_OVER =
            0;

        private const byte AC_SRC_ALPHA =
            1;

        private const int PRF_CHECKVISIBLE =
            0x00000001;

        private const int PRF_NONCLIENT =
            0x00000002;

        private const int PRF_CLIENT =
            0x00000004;

        private const int PRF_ERASEBKGND =
            0x00000008;

        private const int PRF_CHILDREN =
            0x00000010;

        private const uint DIB_RGB_COLORS =
            0;

        #endregion

        #region Structures

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(
                int x,
                int y)
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

            public SIZE(
                int width,
                int height)
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

        #endregion

        #region Win32 Imports

        [DllImport(
            "user32.dll",
            SetLastError = true)]
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
        private static extern IntPtr GetDC(
            IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(
            IntPtr hWnd,
            IntPtr hDC);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(
            out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(
            IntPtr hWnd,
            ref POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(
            IntPtr hWnd,
            out RECT lpRect);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(
            IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(
            IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(
            IntPtr hdc,
            IntPtr h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(
            IntPtr ho);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDIBSection(
            IntPtr hdc,
            ref BITMAPINFO pbmi,
            uint iUsage,
            out IntPtr ppvBits,
            IntPtr hSection,
            uint dwOffset);

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(
            uint uPeriod);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(
            uint uPeriod);

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

        private byte[] _sourceBuffer =
            Array.Empty<byte>();

        private byte[] _outputBuffer =
            Array.Empty<byte>();

        private float[] _lumaBuffer =
            Array.Empty<float>();

        private float[] _lightLut =
            Array.Empty<float>();

        private int _lutRadius;

        #endregion

        #region Frame State

        private bool _hasValidFrame;

        private int _lastOffsetX;

        private int _lastOffsetY;

        private int _lastWidth;

        private int _lastHeight;

        #endregion

        public TangerineLightOverlay(
            Form targetForm)
        {
            _targetForm =
                targetForm
                ?? throw new ArgumentNullException(
                    nameof(targetForm));

            _syncContext =
                SynchronizationContext.Current
                ?? new WindowsFormsSynchronizationContext();

            FormBorderStyle =
                FormBorderStyle.None;

            ShowInTaskbar = false;

            StartPosition =
                FormStartPosition.Manual;

            TopMost = false;

            TabStop = false;

            _targetForm.Move +=
                TargetForm_Move;

            _targetForm.Resize +=
                TargetForm_Resize;

            _targetForm.FormClosed +=
                TargetForm_FormClosed;

            GetCursorPos(
                out _mouseScreenPosition);

            CreateNativeResources();

            RebuildLightLut();

            RaiseTimerResolution();

            _frameStopwatch.Start();

            _running = true;

            _renderDriver =
                new System.Threading.Timer(
                    PostRenderTick,
                    null,
                    0,
                    1);
        }

        #region Properties

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public int TargetFps
        {
            get => _targetFps;

            set
            {
                _targetFps =
                    Math.Clamp(
                        value,
                        15,
                        240);

                _minFrameMilliseconds =
                    1000.0 /
                    _targetFps;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public float Radius
        {
            get => _radius;

            set
            {
                float newValue =
                    Math.Clamp(
                        value,
                        20f,
                        1024f);

                if (Math.Abs(
                        _radius -
                        newValue) < 0.001f)
                {
                    return;
                }

                _radius =
                    newValue;

                RebuildLightLut();

                EnsureOutputDib();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public Color LightColor
        {
            get => _lightColor;

            set =>
                _lightColor = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public float LightStrength
        {
            get => _lightStrength;

            set =>
                _lightStrength =
                    Math.Clamp(
                        value,
                        0f,
                        1f);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public float EdgeStrength
        {
            get => _edgeStrength;

            set =>
                _edgeStrength =
                    Math.Clamp(
                        value,
                        0f,
                        2f);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public float EdgeWidth
        {
            get => _edgeWidth;

            set =>
                _edgeWidth =
                    Math.Clamp(
                        value,
                        0.5f,
                        6f);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public bool IsMouseLight
        {
            get => _isMouseLight;

            set =>
                _isMouseLight = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(
            DesignerSerializationVisibility.Hidden)]
        public float PresentationDelayMs
        {
            get => _presentationDelayMs;

            set =>
                _presentationDelayMs =
                    Math.Max(
                        0f,
                        value);
        }

        #endregion

        #region Window

        protected override bool ShowWithoutActivation =>
            true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp =
                    base.CreateParams;

                cp.ExStyle |=
                    WS_EX_LAYERED |
                    WS_EX_TRANSPARENT |
                    WS_EX_TOOLWINDOW |
                    WS_EX_NOACTIVATE;

                return cp;
            }
        }

        protected override void WndProc(
            ref Message m)
        {
            if (m.Msg ==
                WM_NCHITTEST)
            {
                m.Result =
                    (IntPtr)HTTRANSPARENT;

                return;
            }

            if (m.Msg ==
                WM_MOUSEACTIVATE)
            {
                m.Result =
                    (IntPtr)MA_NOACTIVATE;

                return;
            }

            base.WndProc(ref m);
        }

        #endregion

        #region Render Driver

        private void PostRenderTick(
            object? state)
        {
            if (!_running)
                return;

            if (_tickPosted)
                return;

            _tickPosted = true;

            try
            {
                _syncContext.Post(
                    RunTick,
                    null);
            }
            catch
            {
                _tickPosted = false;
            }
        }

        private void RunTick(
            object? state)
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

            /*
             * =====================================================
             * 关键修改：
             *
             * 鼠标始终保留为 SCREEN 坐标。
             *
             * 不再：
             *
             * ScreenToClient()
             *
             * 因为后面我们可以直接用：
             *
             * MouseScreen - ClientOriginScreen
             *
             * 得到 Client 坐标。
             * =====================================================
             */
            if (!GetCursorPos(
                    out POINT mouseScreen))
            {
                return;
            }

            _mouseScreenPosition =
                mouseScreen;

            /*
             * Target Client (0,0)
             * 在屏幕上的实际坐标。
             */
            POINT clientOrigin =
                new POINT(
                    0,
                    0);

            if (!ClientToScreen(
                    _targetForm.Handle,
                    ref clientOrigin))
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
                rc.Right -
                rc.Left;

            int clientHeight =
                rc.Bottom -
                rc.Top;

            if (clientWidth <= 0 ||
                clientHeight <= 0)
            {
                return;
            }

            /*
             * =====================================================
             * 不再使用 ScreenToClient。
             *
             * 两个值都是 SCREEN 坐标：
             *
             * mouseScreen
             * clientOrigin
             *
             * 相减得到 Client 坐标。
             * =====================================================
             */
            int mouseClientX =
                mouseScreen.X -
                clientOrigin.X;

            int mouseClientY =
                mouseScreen.Y -
                clientOrigin.Y;

            bool inside =
                mouseClientX >= 0 &&
                mouseClientY >= 0 &&
                mouseClientX < clientWidth &&
                mouseClientY < clientHeight;

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
            {
                Show(_targetForm);
            }

            Render(
                new POINT(
                    mouseClientX,
                    mouseClientY),
                mouseScreen,
                clientOrigin,
                clientWidth,
                clientHeight);
        }

        #endregion

        #region Native Resource Creation

        private void CreateNativeResources()
        {
            if (_sourceDc == IntPtr.Zero)
            {
                _sourceDc =
                    CreateCompatibleDC(
                        IntPtr.Zero);
            }

            if (_outputDc == IntPtr.Zero)
            {
                _outputDc =
                    CreateCompatibleDC(
                        IntPtr.Zero);
            }

            EnsureOutputDib();
        }

        private int GetOutputBufferSize()
        {
            int radius =
                (int)Math.Ceiling(
                    _radius);

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
                EnsureOutputBuffer(size);

                return;
            }

            ReplaceDib(
                ref _outputBitmap,
                ref _outputBits,
                _outputDc,
                size,
                size);

            _outputWidth =
                size;

            _outputHeight =
                size;

            EnsureOutputBuffer(size);
        }

        private void EnsureOutputBuffer(
            int size)
        {
            int required =
                checked(
                    size *
                    size *
                    4);

            if (_outputBuffer.Length <
                required)
            {
                _outputBuffer =
                    new byte[required];
            }
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
                EnsureSourceBuffers(
                    width,
                    height);

                return;
            }

            ReplaceDib(
                ref _sourceBitmap,
                ref _sourceBits,
                _sourceDc,
                width,
                height);

            _sourceWidth =
                width;

            _sourceHeight =
                height;

            EnsureSourceBuffers(
                width,
                height);
        }

        private void EnsureSourceBuffers(
            int width,
            int height)
        {
            int required =
                checked(
                    width *
                    height *
                    4);

            if (_sourceBuffer.Length <
                required)
            {
                _sourceBuffer =
                    new byte[required];
            }

            int lumaRequired =
                checked(
                    width *
                    height);

            if (_lumaBuffer.Length <
                lumaRequired)
            {
                _lumaBuffer =
                    new float[
                        lumaRequired];
            }
        }

        private bool ReplaceDib(
            ref IntPtr bitmap,
            ref IntPtr bits,
            IntPtr dc,
            int width,
            int height)
        {
            if (dc == IntPtr.Zero ||
                width <= 0 ||
                height <= 0)
            {
                return false;
            }

            BITMAPINFO info =
                new BITMAPINFO();

            info.bmiHeader.biSize =
                (uint)Marshal.SizeOf<
                    BITMAPINFOHEADER>();

            info.bmiHeader.biWidth =
                width;

            /*
             * Top-down DIB。
             *
             * 第 0 行就是视觉上的第一行。
             */
            info.bmiHeader.biHeight =
                -height;

            info.bmiHeader.biPlanes =
                1;

            info.bmiHeader.biBitCount =
                32;

            info.bmiHeader.biCompression =
                0;

            info.bmiHeader.biSizeImage =
                (uint)(
                    width *
                    height *
                    4);

            IntPtr newBitmap =
                CreateDIBSection(
                    IntPtr.Zero,
                    ref info,
                    DIB_RGB_COLORS,
                    out IntPtr newBits,
                    IntPtr.Zero,
                    0);

            if (newBitmap == IntPtr.Zero)
            {
                return false;
            }

            IntPtr oldBitmap =
                SelectObject(
                    dc,
                    newBitmap);

            /*
             * 旧 bitmap 必须是我们自己之前创建的 bitmap
             * 才能删除。
             */
            if (bitmap != IntPtr.Zero &&
                oldBitmap != IntPtr.Zero)
            {
                DeleteObject(
                    oldBitmap);
            }

            bitmap =
                newBitmap;

            bits =
                newBits;

            return true;
        }

        private void RebuildLightLut()
        {
            int radius =
                (int)Math.Ceiling(
                    _radius);

            _lutRadius =
                radius;

            int radiusSquared =
                checked(
                    radius *
                    radius);

            _lightLut =
                new float[
                    radiusSquared + 1];

            for (int d2 = 0;
                 d2 <= radiusSquared;
                 d2++)
            {
                float normalized =
                    MathF.Sqrt(d2) /
                    _radius;

                _lightLut[d2] =
                    SmoothStep(
                        1f -
                        normalized);
            }
        }

        private void RaiseTimerResolution()
        {
            if (_timerResolutionRaised)
                return;

            timeBeginPeriod(1);

            _timerResolutionRaised =
                true;
        }

        private void LowerTimerResolution()
        {
            if (!_timerResolutionRaised)
                return;

            timeEndPeriod(1);

            _timerResolutionRaised =
                false;
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
             * 当前 frame 的 offset 是 Target Client 坐标。
             *
             * Form 移动以后，只重新取得：
             *
             * Target Client (0,0) -> Screen
             *
             * 不重新计算鼠标。
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
             * 下一帧重新获取 Client Size。
             */
        }

        private void TargetForm_FormClosed(
            object? sender,
            FormClosedEventArgs e)
        {
            _running = false;

            if (!IsDisposed)
            {
                Close();
            }
        }

        #endregion

        #region Render

        private void Render(
            POINT mouseClient,
            POINT mouseScreen,
            POINT clientOrigin,
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

            int radius =
                _lutRadius;

            /*
             * 光效区域仍然使用 Client 坐标计算。
             */
            int left =
                Math.Max(
                    0,
                    mouseClient.X -
                    radius);

            int top =
                Math.Max(
                    0,
                    mouseClient.Y -
                    radius);

            int right =
                Math.Min(
                    clientWidth - 1,
                    mouseClient.X +
                    radius);

            int bottom =
                Math.Min(
                    clientHeight - 1,
                    mouseClient.Y +
                    radius);

            int width =
                right -
                left +
                1;

            int height =
                bottom -
                top +
                1;

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
             * =====================================================
             * Capture
             *
             * source DIB：
             *
             * source[0,0]
             * =
             * target client[0,0]
             *
             * 所以 source[left,top]
             * 就是目标区域左上角。
             * =====================================================
             */
            CaptureTarget();

            int sourceStride =
                _sourceWidth *
                4;

            int rowBytes =
                width *
                4;

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
                    row * rowBytes,
                    rowBytes);
            }

            /*
             * Luma。
             */
            for (int y = 0;
                 y < height;
                 y++)
            {
                int sourceRow =
                    y *
                    rowBytes;

                int lumaRow =
                    y *
                    width;

                for (int x = 0;
                     x < width;
                     x++)
                {
                    int p =
                        sourceRow +
                        x * 4;

                    _lumaBuffer[
                        lumaRow + x] =
                        _sourceBuffer[p + 2] *
                            0.299f +
                        _sourceBuffer[p + 1] *
                            0.587f +
                        _sourceBuffer[p] *
                            0.114f;
                }
            }

            int outputBytes =
                width *
                height *
                4;

            Array.Clear(
                _outputBuffer,
                0,
                outputBytes);

            /*
             * 鼠标在当前光效 bitmap 中的位置。
             */
            int mouseX =
                mouseClient.X -
                left;

            int mouseY =
                mouseClient.Y -
                top;

            int radiusSquared =
                radius *
                radius;

            byte colorB =
                _lightColor.B;

            byte colorG =
                _lightColor.G;

            byte colorR =
                _lightColor.R;

            bool useEdges =
                _edgeStrength >
                0.0001f;

            float edgeWidthFactor =
                1f;

            if (_edgeWidth > 1f)
            {
                edgeWidthFactor =
                    Math.Clamp(
                        1f /
                            _edgeWidth +
                        0.5f,
                        0.5f,
                        1f);
            }

            /*
             * 单线程处理。
             *
             * 对半径 180 的区域，
             * Parallel.For 的调度开销没有必要。
             */
            for (int y = 1;
                 y < height - 1;
                 y++)
            {
                int dy =
                    y -
                    mouseY;

                int dySquared =
                    dy *
                    dy;

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
                        mouseX -
                        span);

                int endX =
                    Math.Min(
                        width - 2,
                        mouseX +
                        span);

                int rowBase =
                    y *
                    width;

                for (int x = startX;
                     x <= endX;
                     x++)
                {
                    int dx =
                        x -
                        mouseX;

                    int distanceSquared =
                        dx *
                            dx +
                        dySquared;

                    if (distanceSquared >
                        radiusSquared)
                    {
                        continue;
                    }

                    float light =
                        _lightLut[
                            distanceSquared];

                    if (light <=
                        0.0001f)
                    {
                        continue;
                    }

                    float alpha =
                        0f;

                    if (_isMouseLight)
                    {
                        alpha =
                            light *
                            _lightStrength;
                    }

                    if (useEdges)
                    {
                        int index =
                            rowBase +
                            x;

                        float edge =
                            CalculateSobelEdge(
                                _lumaBuffer,
                                width,
                                index);

                        if (edge > 0f)
                        {
                            alpha +=
                                edge *
                                _edgeStrength *
                                light *
                                edgeWidthFactor;
                        }
                    }

                    if (alpha <=
                        0.001f)
                    {
                        continue;
                    }

                    if (alpha > 1f)
                    {
                        alpha = 1f;
                    }

                    int outputIndex =
                        (rowBase + x) *
                        4;

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
                            alpha *
                            255f);
                }
            }

            /*
             * 写入 Output DIB。
             */
            int outputStride =
                _outputWidth *
                4;

            for (int row = 0;
                 row < height;
                 row++)
            {
                IntPtr destinationRow =
                    IntPtr.Add(
                        _outputBits,
                        row *
                        outputStride);

                Marshal.Copy(
                    _outputBuffer,
                    row * rowBytes,
                    destinationRow,
                    rowBytes);
            }

            /*
             * 保存 Client 坐标。
             */
            _lastOffsetX =
                left;

            _lastOffsetY =
                top;

            _lastWidth =
                width;

            _lastHeight =
                height;

            _hasValidFrame =
                true;

            /*
             * =====================================================
             * 最终屏幕坐标。
             *
             * clientOrigin 已经是 SCREEN 坐标。
             *
             * 所以：
             *
             * overlayScreen =
             *     clientOrigin +
             *     clientOffset
             *
             * 完全没有 ScreenToClient / ClientToScreen 往返。
             * =====================================================
             */
            int screenLeft =
                clientOrigin.X +
                left;

            int screenTop =
                clientOrigin.Y +
                top;

            /*
             * 这里专门做一次数学验证。
             *
             * 如果 mouseScreen 和
             * clientOrigin + mouseClient
             * 不一致，说明系统 DPI/坐标虚拟化
             * 本身就在制造误差。
             */
            int roundTripX =
                clientOrigin.X +
                mouseClient.X;

            int roundTripY =
                clientOrigin.Y +
                mouseClient.Y;

            int errorX =
                roundTripX -
                mouseScreen.X;

            int errorY =
                roundTripY -
                mouseScreen.Y;

            if (errorX != 0 ||
                errorY != 0)
            {
                Debug.WriteLine(
                    $"TangerineLight coordinate error: " +
                    $"MouseScreen=({mouseScreen.X}," +
                    $"{mouseScreen.Y}) " +
                    $"ClientOrigin=({clientOrigin.X}," +
                    $"{clientOrigin.Y}) " +
                    $"MouseClient=({mouseClient.X}," +
                    $"{mouseClient.Y}) " +
                    $"RoundTrip=({roundTripX}," +
                    $"{roundTripY}) " +
                    $"Error=({errorX}," +
                    $"{errorY})");
            }

            UpdateLayeredWindowImage(
                screenLeft,
                screenTop,
                width,
                height);
        }

        #endregion

        #region Capture

        private void CaptureTarget()
        {
            /*
             * source DC 的坐标原点固定为：
             *
             * Target Client (0,0)
             *
             * 不设置：
             *
             * SetWindowOrgEx
             * SetViewportOrgEx
             *
             * 不进行任何坐标平移。
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

            if (magnitudeSquared <=
                0f)
            {
                return 0f;
            }

            float magnitude =
                MathF.Sqrt(
                    magnitudeSquared);

            const float maxMagnitude =
                1020f;

            const float threshold =
                0.10f;

            float normalized =
                magnitude /
                maxMagnitude;

            if (normalized <=
                threshold)
            {
                return 0f;
            }

            float edge =
                (normalized -
                    threshold) /
                (1f -
                    threshold);

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
                (3f -
                 2f *
                 value);
        }

        #endregion

        #region Layered Window

        private void ClearOverlay()
        {
            if (!IsHandleCreated)
                return;

            if (_outputBits == IntPtr.Zero)
                return;

            /*
             * 只显示 1x1 的完全透明像素。
             */
            Marshal.WriteInt32(
                _outputBits,
                0);

            _hasValidFrame =
                false;

            /*
             * Clear 时直接使用当前 Target Client
             * 左上角的屏幕坐标。
             */
            POINT origin =
                new POINT(
                    0,
                    0);

            if (!ClientToScreen(
                    _targetForm.Handle,
                    ref origin))
            {
                return;
            }

            UpdateLayeredWindowScreen(
                origin.X,
                origin.Y,
                1,
                1);
        }

        private void UpdateLayeredWindowImage(
            int screenX,
            int screenY,
            int width,
            int height)
        {
            UpdateLayeredWindowScreen(
                screenX,
                screenY,
                width,
                height);
        }

        private void UpdateLayeredWindowScreen(
            int screenX,
            int screenY,
            int width,
            int height)
        {
            if (!IsHandleCreated)
                return;

            if (_targetForm.IsDisposed)
                return;

            if (_outputDc == IntPtr.Zero ||
                _outputBitmap == IntPtr.Zero)
            {
                return;
            }

            POINT destination =
                new POINT(
                    screenX,
                    screenY);

            POINT source =
                new POINT(
                    0,
                    0);

            SIZE size =
                new SIZE(
                    width,
                    height);

            BLENDFUNCTION blend =
                new BLENDFUNCTION
                {
                    BlendOp =
                        AC_SRC_OVER,

                    BlendFlags =
                        0,

                    SourceConstantAlpha =
                        255,

                    AlphaFormat =
                        AC_SRC_ALPHA
                };

            IntPtr screenDc =
                GetDC(
                    IntPtr.Zero);

            if (screenDc == IntPtr.Zero)
                return;

            try
            {
                bool success =
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

                if (!success)
                {
                    Debug.WriteLine(
                        "UpdateLayeredWindow failed: " +
                        Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                ReleaseDC(
                    IntPtr.Zero,
                    screenDc);
            }
        }

        #endregion

        #region Start / Stop

        public void Start()
        {
            if (IsDisposed)
                return;

            _running =
                true;

            RaiseTimerResolution();

            _frameStopwatch.Restart();

            if (!Visible)
            {
                Show(_targetForm);
            }
        }

        public void Stop()
        {
            _running =
                false;

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
                _running =
                    false;

                _renderDriver.Dispose();

                _targetForm.Move -=
                    TargetForm_Move;

                _targetForm.Resize -=
                    TargetForm_Resize;

                _targetForm.FormClosed -=
                    TargetForm_FormClosed;

                LowerTimerResolution();

                /*
                 * 先把 bitmap 从 DC 中移除。
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