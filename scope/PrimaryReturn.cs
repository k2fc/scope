using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DGScope
{
    public class PrimaryReturn : Control, IScreenObject
    {
        PointF _location;
        public PointF NewLocation { get { return LocationF; } set { } }
        public PointF LocationF
        {
            set
            {
                _location = value;
                if (Parent != null)
                    Location = new System.Drawing.Point((int)(((Parent.ClientSize.Width / 2) * _location.X) + 1) - Size.Width / 2, Parent.ClientSize.Height - (int)(((Parent.ClientSize.Height / 2) * _location.Y) + 1) - Size.Height / 2);
            }
            get
            {
                return _location;
            }
        }

        public float ShapeWidth { get; set; }
        public float ShapeHeight { get; set; }

        public new Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                if (Parent != null)
                    SizeF = new SizeF((Size.Width / (Parent.ClientSize.Width / 2f)), (Size.Height / (Parent.ClientSize.Height / 2f)));
            }
        }
        private SizeF _sizef;
        public SizeF SizeF
        {
            get
            {
                return _sizef;
            }
            set
            {
                _sizef = value;
                if (Parent != null)
                    base.Size = new Size((int)(SizeF.Width * (Parent.ClientSize.Width / 2)), (int)(SizeF.Height * (Parent.ClientSize.Height / 2)));
            }
        }
        public RectangleF BoundsF
        {
            get
            {
                return new RectangleF(LocationF.X - (SizeF.Width / 2), LocationF.Y - (SizeF.Height / 2), SizeF.Width, SizeF.Height);
            }
        }

        public Aircraft ParentAircraft { get; set; }
        public double Angle { get; set; } = 0;
        private Color initialColor = Color.Lime;
        public int Length { get; set; } = 20;
        public bool Fading { get; set; } = true;
        public override Color ForeColor
        {
            get
            {
                if (!Fading)
                {
                    if (Colors == null)
                        return initialColor;
                    var color = Colors[currentColor];
                    
                    return color;
                }
                if (Intensity >= 1)
                    return initialColor;
                if (Intensity <= 0)
                    return Color.Transparent;
                //return Color.FromArgb((int)(initialColor.R * Intensity * Intensity), (int)(initialColor.G * Intensity * Intensity), (int)(initialColor.B * Intensity * Intensity));
                var newcolor = Color.FromArgb((int)(Math.Pow(initialColor.A, Intensity)), (int)(initialColor.R), (int)(initialColor.G), (int)(initialColor.B));
                return newcolor;
            }

            set
            {
                initialColor = value;
            }

        }
        public Color[] Colors
        {
            get; set;
        }
        private int currentColor = 0;
        Stopwatch Stopwatch = new Stopwatch();
        double intensity;
        public double Intensity
        {
            get
            {
                return (intensity - Stopwatch.ElapsedMilliseconds / (FadeTime * 1000));
            }
            set
            {
                ResetIntensity(value);
            }
        }

        public int IncrementColor()
        {
            if (currentColor < Colors.Length - 1)
                currentColor++;
            return currentColor;
        }
        public PrimaryReturn()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(
  //ControlStyles.AllPaintingInWmPaint |
  ControlStyles.UserPaint |
  ControlStyles.DoubleBuffer, false);
            BackColor = Color.Transparent;
            base.BackColor = Color.Transparent;
            LocationChanged += PrimaryReturn_LocationChanged;
            ResetIntensity(0);
        }

        void ResetIntensity(double Intensity = 1)
        {
            intensity = Intensity;
            Stopwatch.Restart();
        }
        public double FadeTime { get; set; }



        private void PrimaryReturn_LocationChanged(object sender, EventArgs e)
        {
            PaintTarget();
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // do nothing
        }

        public void Fade(int milliseconds)
        {

        }



        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            PaintTarget();
        }
        private Bitmap _backBuffer;

        public new int Width
        {
            get { return TargetImage().Width; }
            private set { base.Width = value; }
        }
        public new int Height
        {
            get { return TargetImage().Height; }
            private set { base.Height = value; }
        }

        Bitmap TargetImage()
        {
            if (IsDisposed)
                return null;
            double angle = Angle * Math.PI / 180;
            double len = Length;
            double x = Math.Cos(angle) * len / 2;
            double y = Math.Sin(angle) * len / 2;
            int _width = (int)Math.Abs(x) + 6;
            int _height = (int)Math.Abs(y) + 6;
            Bitmap backBuffer = new Bitmap(_width, _height);
            using (Graphics g = Graphics.FromImage(backBuffer))
            {
                using (Pen pen = new Pen(ForeColor, 3))
                {


                    float x1 = (float)((_width / 2) + x);
                    float x2 = (float)((_width / 2) - x);
                    float y1 = (float)((_height / 2) + y);
                    float y2 = (float)((_height / 2) - y);

                    g.DrawLine(pen, x1, y1, x2, y2);

                }
            }
            return backBuffer;
        }

        public void PaintTarget()
        {
            if (IsDisposed)
                return;
            _backBuffer = TargetImage();
            Width = _backBuffer.Width;
            Height = _backBuffer.Height;

            using (Graphics g = CreateGraphics())
            {
                if (Visible)
                    g.DrawImageUnscaled(_backBuffer, 0, 0);
            }
        }



        protected override void Dispose(bool disposing)
        {
            if (_backBuffer != null)
                _backBuffer.Dispose();
            base.Dispose(disposing);
        }
    }
    public enum TargetShape
    {
        Rectangle, Circle
    }
}
