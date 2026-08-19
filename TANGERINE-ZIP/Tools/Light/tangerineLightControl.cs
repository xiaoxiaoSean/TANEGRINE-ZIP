using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TANGERINE_ZIP.Tools.LightTool
{
    public class TangerineLightControl : Control
    {
        private Bitmap? _image;
        private Bitmap? _alphaMask;
        private Bitmap? _edgeMask;

        private Point _mousePosition;
        private bool _mouseInside;

        #region Properties

        [Category("Tangerine Light")]
        [Description("鼠标光照影响半径")]
        [DefaultValue(180f)]
        public float LightRadius { get; set; } = 180f;

        [Category("Tangerine Light")]
        [Description("边缘光强度")]
        [DefaultValue(2.5f)]
        public float GlowStrength { get; set; } = 2.5f;

        [Category("Tangerine Light")]
        [Description("光照颜色")]
        [DefaultValue(typeof(Color), "White")]
        public Color GlowColor { get; set; } = Color.White;

        [Category("Tangerine Light")]
        [Description("外部光晕强度")]
        [DefaultValue(1.5f)]
        public float OuterGlowStrength { get; set; } = 1.5f;

        #endregion

        public TangerineLightControl()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );

            UpdateStyles();
        }

        #region Image

        public void SetImage(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            _image?.Dispose();
            _alphaMask?.Dispose();
            _edgeMask?.Dispose();

            _image = new Bitmap(image);

            GenerateMasks();

            Invalidate();
        }

        public void SetImage(string path)
        {
            using Image image = Image.FromFile(path);
            SetImage(image);
        }

        #endregion

        #region Mouse

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            _mouseInside = true;

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _mouseInside = false;

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            _mousePosition = e.Location;

            Invalidate();
        }

        #endregion

        #region Resize

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_image != null &&
                Width > 0 &&
                Height > 0)
            {
                GenerateMasks();
                Invalidate();
            }
        }

        #endregion

        #region Painting

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_image == null)
                return;

            if (Width <= 0 || Height <= 0)
                return;

            Graphics g = e.Graphics;

            g.SmoothingMode =
                SmoothingMode.HighQuality;

            g.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            g.PixelOffsetMode =
                PixelOffsetMode.HighQuality;

            // 先绘制原始图片
            g.DrawImage(
                _image,
                new Rectangle(
                    0,
                    0,
                    Width,
                    Height
                )
            );

            if (!_mouseInside)
                return;

            if (_alphaMask == null ||
                _edgeMask == null)
                return;

            DrawLighting(g);
        }

        #endregion

        #region Lighting

        private void DrawLighting(Graphics g)
        {
            int width = Width;
            int height = Height;

            using Bitmap light =
                new Bitmap(
                    width,
                    height,
                    PixelFormat.Format32bppArgb
                );

            Rectangle rect =
                new Rectangle(
                    0,
                    0,
                    width,
                    height
                );

            BitmapData alphaData =
                _alphaMask!.LockBits(
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb
                );

            BitmapData edgeData =
                _edgeMask!.LockBits(
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb
                );

            BitmapData lightData =
                light.LockBits(
                    rect,
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb
                );

            try
            {
                int alphaStride =
                    alphaData.Stride;

                int edgeStride =
                    edgeData.Stride;

                int lightStride =
                    lightData.Stride;

                int alphaLength =
                    Math.Abs(alphaStride) * height;

                int edgeLength =
                    Math.Abs(edgeStride) * height;

                int lightLength =
                    Math.Abs(lightStride) * height;

                byte[] alphaBuffer =
                    new byte[alphaLength];

                byte[] edgeBuffer =
                    new byte[edgeLength];

                byte[] lightBuffer =
                    new byte[lightLength];

                Marshal.Copy(
                    alphaData.Scan0,
                    alphaBuffer,
                    0,
                    alphaBuffer.Length
                );

                Marshal.Copy(
                    edgeData.Scan0,
                    edgeBuffer,
                    0,
                    edgeBuffer.Length
                );

                float radius =
                    Math.Max(
                        1f,
                        LightRadius
                    );

                float strength =
                    Math.Max(
                        0f,
                        GlowStrength
                    );

                float outerStrength =
                    Math.Max(
                        0f,
                        OuterGlowStrength
                    );

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float dx =
                            x - _mousePosition.X;

                        float dy =
                            y - _mousePosition.Y;

                        float distance =
                            MathF.Sqrt(
                                dx * dx +
                                dy * dy
                            );

                        if (distance > radius)
                            continue;

                        float radial =
                            1f -
                            distance / radius;

                        // 柔和衰减
                        radial *= radial;

                        int index =
                            y * alphaStride +
                            x * 4;

                        byte alpha =
                            alphaBuffer[index + 3];

                        byte edge =
                            edgeBuffer[index + 3];

                        /*
                         * ① Logo 内部光照
                         *
                         * 鼠标照到 Logo 表面时，
                         * Logo 本身会产生柔和高光。
                         */

                        if (alpha > 0)
                        {
                            float surface =
                                radial *
                                0.35f;

                            /*
                             * 边缘像素额外增强
                             */
                            if (edge > 0)
                            {
                                surface +=
                                    radial *
                                    strength;
                            }

                            surface =
                                Math.Clamp(
                                    surface,
                                    0f,
                                    1f
                                );

                            byte a =
                                (byte)(
                                    alpha *
                                    surface
                                );

                            if (a > lightBuffer[index + 3])
                            {
                                lightBuffer[index + 0] =
                                    GlowColor.B;

                                lightBuffer[index + 1] =
                                    GlowColor.G;

                                lightBuffer[index + 2] =
                                    GlowColor.R;

                                lightBuffer[index + 3] =
                                    a;
                            }
                        }
                        else
                        {
                            /*
                             * ② Logo 外部光晕
                             *
                             * 鼠标靠近 Logo 边缘时，
                             * 边缘外面也会出现光。
                             */

                            if (edge == 0)
                                continue;

                            float glow =
                                radial *
                                outerStrength;

                            glow =
                                Math.Clamp(
                                    glow,
                                    0f,
                                    1f
                                );

                            byte a =
                                (byte)(
                                    255 *
                                    glow
                                );

                            lightBuffer[index + 0] =
                                GlowColor.B;

                            lightBuffer[index + 1] =
                                GlowColor.G;

                            lightBuffer[index + 2] =
                                GlowColor.R;

                            lightBuffer[index + 3] =
                                a;
                        }
                    }
                }

                Marshal.Copy(
                    lightBuffer,
                    0,
                    lightData.Scan0,
                    lightBuffer.Length
                );
            }
            finally
            {
                _alphaMask.UnlockBits(
                    alphaData
                );

                _edgeMask.UnlockBits(
                    edgeData
                );

                light.UnlockBits(
                    lightData
                );
            }
            g.CompositingMode =
                CompositingMode.SourceOver;

            g.DrawImage(
                light,
                0,
                0,
                width,
                height
            );
        }

        #endregion

        #region Generate Masks

        private void GenerateMasks()
        {
            _alphaMask?.Dispose();
            _edgeMask?.Dispose();

            _alphaMask = null;
            _edgeMask = null;

            if (_image == null)
                return;

            if (Width <= 0 ||
                Height <= 0)
                return;

            using Bitmap resized =
                new Bitmap(
                    Width,
                    Height,
                    PixelFormat.Format32bppArgb
                );

            using (Graphics g =
                   Graphics.FromImage(resized))
            {
                g.Clear(
                    Color.Transparent
                );

                g.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

                g.PixelOffsetMode =
                    PixelOffsetMode.HighQuality;

                g.DrawImage(
                    _image,
                    new Rectangle(
                        0,
                        0,
                        Width,
                        Height
                    )
                );
            }

            _alphaMask =
                new Bitmap(
                    Width,
                    Height,
                    PixelFormat.Format32bppArgb
                );

            _edgeMask =
                new Bitmap(
                    Width,
                    Height,
                    PixelFormat.Format32bppArgb
                );

            Rectangle rect =
                new Rectangle(
                    0,
                    0,
                    Width,
                    Height
                );

            BitmapData sourceData =
                resized.LockBits(
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb
                );

            BitmapData alphaData =
                _alphaMask.LockBits(
                    rect,
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb
                );

            BitmapData edgeData =
                _edgeMask.LockBits(
                    rect,
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb
                );

            try
            {
                int sourceStride =
                    sourceData.Stride;

                int alphaStride =
                    alphaData.Stride;

                int edgeStride =
                    edgeData.Stride;

                int sourceLength =
                    Math.Abs(sourceStride) *
                    Height;

                int alphaLength =
                    Math.Abs(alphaStride) *
                    Height;

                int edgeLength =
                    Math.Abs(edgeStride) *
                    Height;

                byte[] source =
                    new byte[sourceLength];

                byte[] alphaBuffer =
                    new byte[alphaLength];

                byte[] edgeBuffer =
                    new byte[edgeLength];

                Marshal.Copy(
                    sourceData.Scan0,
                    source,
                    0,
                    source.Length
                );

                /*
                 * 先保存完整 Alpha。
                 */

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int index =
                            y * sourceStride +
                            x * 4;

                        byte alpha =
                            source[index + 3];

                        alphaBuffer[index + 3] =
                            alpha;
                    }
                }

                /*
                 * 提取轮廓。
                 *
                 * 不再要求邻居必须 == 0。
                 *
                 * 只要邻居明显比当前像素透明，
                 * 就认为接近边缘。
                 *
                 * 这样可以正确处理 PNG 抗锯齿。
                 */

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int index =
                            y * sourceStride +
                            x * 4;

                        byte current =
                            source[index + 3];

                        if (current < 10)
                            continue;

                        byte minNeighbor =
                            255;

                        // 左
                        if (x > 0)
                        {
                            int i =
                                y * sourceStride +
                                (x - 1) * 4;

                            minNeighbor =
                                Math.Min(
                                    minNeighbor,
                                    source[i + 3]
                                );
                        }
                        else
                        {
                            minNeighbor = 0;
                        }

                        // 右
                        if (x < Width - 1)
                        {
                            int i =
                                y * sourceStride +
                                (x + 1) * 4;

                            minNeighbor =
                                Math.Min(
                                    minNeighbor,
                                    source[i + 3]
                                );
                        }
                        else
                        {
                            minNeighbor = 0;
                        }

                        // 上
                        if (y > 0)
                        {
                            int i =
                                (y - 1) *
                                sourceStride +
                                x * 4;

                            minNeighbor =
                                Math.Min(
                                    minNeighbor,
                                    source[i + 3]
                                );
                        }
                        else
                        {
                            minNeighbor = 0;
                        }

                        // 下
                        if (y < Height - 1)
                        {
                            int i =
                                (y + 1) *
                                sourceStride +
                                x * 4;

                            minNeighbor =
                                Math.Min(
                                    minNeighbor,
                                    source[i + 3]
                                );
                        }
                        else
                        {
                            minNeighbor = 0;
                        }

                        int difference =
                            current -
                            minNeighbor;

                        if (difference > 30)
                        {
                            edgeBuffer[index + 3] =
                                (byte)
                                Math.Min(
                                    255,
                                    difference * 4
                                );
                        }
                    }
                }

                Marshal.Copy(
                    alphaBuffer,
                    0,
                    alphaData.Scan0,
                    alphaBuffer.Length
                );

                Marshal.Copy(
                    edgeBuffer,
                    0,
                    edgeData.Scan0,
                    edgeBuffer.Length
                );
            }
            finally
            {
                resized.UnlockBits(
                    sourceData
                );

                _alphaMask.UnlockBits(
                    alphaData
                );

                _edgeMask.UnlockBits(
                    edgeData
                );
            }
        }

        #endregion

        #region Dispose

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _image?.Dispose();
                _alphaMask?.Dispose();
                _edgeMask?.Dispose();

                _image = null;
                _alphaMask = null;
                _edgeMask = null;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}