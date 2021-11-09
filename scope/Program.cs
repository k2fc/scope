using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Linq;
using DGScope.Receivers;

namespace DGScope
{
    static class Program
    {
        static void Start(bool screensaver = false)
        {
            string settingsPath = screensaver? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml") :
               Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DGScope.xml");
            RadarWindow radarWindow;
            if (File.Exists(settingsPath))
            {
                radarWindow = TryLoad(settingsPath);
            }
            else
            {
                radarWindow = new RadarWindow();
                if (!screensaver)
                {
                    MessageBox.Show("No config file found. Starting a new config.");
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    radarWindow.SaveSettings(settingsPath);
                }
            }
            radarWindow.Run(screensaver);
        }
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            LoadReceiverPlugins();
            if (args.Length > 0)
            {
                string arg = args[0].ToUpper().Trim();
                if (arg.Contains("/C")) 
                {
                    string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DGScope.xml");
                    RadarWindow radarWindow;
                    if (File.Exists(settingsPath))
                    {
                        radarWindow = TryLoad(settingsPath);
                    }
                    else
                    {
                        radarWindow = new RadarWindow();
                    }
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    radarWindow.SaveSettings(settingsPath);
                }
                else if (arg.Contains("/S"))
                {
                    Start(true);
                }
                else if (arg.Contains("/P"))
                {
                    //do nothing
                }
                else
                {
                    Start(false);
                }
            }
            else
            {
                Start(false);
            }
        }
        static RadarWindow TryLoad(string settingsPath)
        {
            try
            {
                return XmlSerializer<RadarWindow>.DeserializeFromFile(settingsPath);
            }
            catch (Exception ex)
            {
                RadarWindow radarWindow = new RadarWindow();
                Console.WriteLine(ex.StackTrace);
                var mboxresult = MessageBox.Show("Error reading settings file.\n" + ex.Message + "\nPress Abort to exit, Retry to try again, or Ignore to destroy the file and start a new config.", "Error reading settings file", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                if (mboxresult == DialogResult.Abort)
                    Environment.Exit(1);
                else if (mboxresult == DialogResult.Retry)
                    return TryLoad(settingsPath);
                else
                {
                    radarWindow = new RadarWindow();
                    PropertyForm propertyForm = new PropertyForm(radarWindow);
                    propertyForm.ShowDialog();
                    return radarWindow;
                }
            }
            return new RadarWindow();
        }

        static void LoadReceiverPlugins()
        {
            String path = Application.StartupPath;
            string[] pluginFiles = Directory.GetFiles(path, "DGScope.*.dll");
            var ipi = (from file in pluginFiles let asm = Assembly.LoadFile(file)
                      from plugintype in asm.GetExportedTypes()
                      where typeof(Receiver).IsAssignableFrom(plugintype)
                      select (Receiver)Activator.CreateInstance(plugintype)).ToArray();
        }
        
    }
    
    
    

    
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
                    Location = new System.Drawing.Point((int)(((Parent.ClientSize.Width / 2) * _location.X) + 1) - Size.Width/2, Parent.ClientSize.Height - (int)(((Parent.ClientSize.Height / 2) * _location.Y) + 1)-Size.Height/2);
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
                    return initialColor;
                if (Intensity >= 1)
                    return initialColor;
                if (Intensity <= 0)
                    return Color.Transparent;
                //return Color.FromArgb((int)(initialColor.R * Intensity * Intensity), (int)(initialColor.G * Intensity * Intensity), (int)(initialColor.B * Intensity * Intensity));
                var newcolor = Color.FromArgb((int)(Math.Pow(initialColor.A,Intensity)), (int)(initialColor.R), (int)(initialColor.G), (int)(initialColor.B));
                return newcolor;
            }

            set
            {
                initialColor = value;
            }

        }
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
            Bitmap backBuffer = new Bitmap(_width,_height);
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

    public class NewPrimaryReturn : IScreenObject
    {
        public bool InScreenBounds => LocationF.X > -1 && LocationF.X < 1 && LocationF.Y > -1 && LocationF.Y < 1;
        Color initialColor;
        public PointF NewLocation { get; set; }
        public Color ForeColor 
        { 
            get
            {
                if (Intensity > 1)
                    return initialColor;
                if (Intensity < 0)
                    return Color.Black;
                return Color.FromArgb((int)(initialColor.R * Intensity), (int)(initialColor.G * Intensity), (int)(initialColor.B * Intensity));
            }

            set
            {
                initialColor = value;
            }

        }
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

        public double Angle { get; set; }

        public NewPrimaryReturn(Aircraft ParentAircraft, Color ForeColor, double FadeTime)
        {
            this.FadeTime = FadeTime;
            this.ForeColor = ForeColor;
            this.ParentAircraft = ParentAircraft;
            ResetIntensity(0);
        }

        void ResetIntensity(double Intensity = 1)
        {
            intensity = Intensity;
            Stopwatch.Restart();
        }
        double FadeTime { get; set; }
        public PointF LocationF { get; set; }
        public SizeF SizeF { get; set; }
        public RectangleF BoundsF
        {
            get
            {
                return new RectangleF(LocationF.X - (SizeF.Width / 2), LocationF.Y - (SizeF.Height / 2), SizeF.Width, SizeF.Height);
            }
        }

        public Aircraft ParentAircraft { get; set; }
    }
    public class ConnectingLine : Control
    {
        System.Drawing.Point _start;
        System.Drawing.Point _end;
        public System.Drawing.Point Start 
        { 
            get
            { 
                return _start; 
            }
            set
            {
                bool changed = false;
                if (value != _start)
                    changed = true;
                _start = value;
                Left = _end.X > _start.X ? _start.X : _end.X;
                Top = _end.Y > _start.Y ? _start.Y : _end.Y;
                this.Width = Math.Abs(Start.X - End.X) + 2;
                this.Height = Math.Abs(Start.Y - End.Y) + 2;
                if (changed)
                    PaintLine();
            }

        }
        public System.Drawing.Point End
        {
            get
            {
                return _end;
            }
            set
            {
                bool changed = false;
                if (value != _end)
                    changed = true;
                _end = value;
                if (_end.X > _start.X)
                    Left = _start.X;
                else
                    Left = _end.X;
                if (_end.Y > _start.Y)
                    Top = _start.Y;
                else
                    Top = _end.Y;
                this.Width = Math.Abs(Start.X - End.X) + 2;
                this.Height = Math.Abs(Start.Y - End.Y) + 2;
                if (changed)
                    PaintLine();
            }

        }
        public ConnectingLine()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(
                //ControlStyles.AllPaintingInWmPaint |
  ControlStyles.UserPaint |
  ControlStyles.DoubleBuffer, false);
            BackColor = Color.Transparent;
            base.BackColor = Color.Transparent;
            LocationChanged += ConnectingLine_LocationChanged;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            PaintLine();
        }
        private void ConnectingLine_LocationChanged(object sender, EventArgs e)
        {
            PaintLine();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            PaintLine();
        }


        public void PaintLine()
        {
            if (IsDisposed)
                return;
            if (Start == new System.Drawing.Point() || End == new System.Drawing.Point())
                return;
            if (!Visible)
                return;

            
            _backBuffer = LineImage();
            if (_backBuffer == null)
                return;
            RecreateHandle();
            //Width = _backBuffer.Width;
            //Height = _backBuffer.Height;

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

        private Bitmap _backBuffer;
        private Bitmap LineImage()
        {
            if (Width == 0 || Height == 0)
                return null;
            Bitmap backBuffer = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(backBuffer))
            {
                using (Pen pen = new Pen(ForeColor, 1))
                {
                    PointF start = new PointF(Math.Abs(Start.X - Left), Math.Abs(Start.Y - Top));
                    PointF end = new PointF(Math.Abs(End.X - Left), Math.Abs(End.Y - Top));

                    g.DrawLine(pen, start.X, start.Y, end.X, end.Y);

                }
            }
            return backBuffer;
        }
    }

    public class RangeBearingLine
    {
        private PointF _start;
        public PointF Start
        {
            get { return _start; }
            set
            {
                _start = value;
                
            }
        }
        private PointF _end;
        public PointF End 
        { 
            get { return _end; } 
            set {
                _end = value;
            }     
        }

        public GeoPoint? StartGeo { get; set; }
        public GeoPoint? EndGeo { get; set; }

        public Aircraft? StartPlane { get; set; } = null;
        public Aircraft? EndPlane { get; set; } = null;
        public PointF LocationF { get { return new PointF(Start.X > End.X ? End.X : Start.X, Start.Y > End.Y ? End.Y : Start.Y); } set { } }
        public SizeF SizeF { get { return new SizeF(Math.Abs(Start.X - End.X), Math.Abs(Start.Y - End.Y)); } set { } }

        public RectangleF BoundsF => new RectangleF(LocationF, SizeF);
        public Line Line { get; set; } = new Line();
        public TransparentLabel Label = new TransparentLabel() { AutoSize = true };

    }
    public class ConnectingLineF : IScreenObject
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }
        public PointF LocationF { get { return new PointF(Start.X > End.X ? End.X : Start.X, Start.Y > End.Y ? End.Y : Start.Y); }set { } }
        public PointF NewLocation { get; set; }
        public SizeF SizeF { get { return new SizeF(Math.Abs(Start.X - End.X), Math.Abs(Start.Y - End.Y)); } set { } }

        public RectangleF BoundsF => new RectangleF(LocationF, SizeF);

        public bool IntersectsWith (RectangleF Rectangle)
        {
            return LineIntersectsRect(Start, End, Rectangle);
        }
        public bool IntersectsWith (ConnectingLineF Line)
        {
            return LineIntersectsLine(Line.Start, Line.End, Start, End);
        }
        private static bool LineIntersectsRect(PointF p1, PointF p2, RectangleF r)
        {
            return LineIntersectsLine(p1, p2, new PointF(r.Left, r.Top), new PointF(r.Right, r.Top)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.Right, r.Y), new PointF(r.Right, r.Bottom)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.Right, r.Bottom), new PointF(r.Left, r.Bottom)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.Left, r.Bottom), new PointF(r.Left, r.Top)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }

        private static bool LineIntersectsLine(PointF l1p1, PointF l1p2, PointF l2p1, PointF l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }
        public Aircraft ParentAircraft { get; set; }
    }
    public class FadingLabel : Label
    {
        private double intensity = 1;
        private double angle = 0;
        public double Angle { get { return angle - 90; } set { angle = value + 90; } }
        private Color initialcolor;
        public double Intensity 
        {
            get
            {
                return intensity;
            }
            set
            {
                intensity = value;
                base.ForeColor = Color.FromArgb((int)(initialcolor.R * intensity), (int)(initialcolor.G * intensity), (int)(initialcolor.B * intensity));
            } 
        }
        public override Color ForeColor
        {
            set
            {
                intensity = 1;
                base.ForeColor = value;
                initialcolor = value;
            }
            get { return base.ForeColor; }

        }
        
    }
    
    

    
    
}
