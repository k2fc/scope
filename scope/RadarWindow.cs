using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Drawing.Design;
using DGScope.Receivers;
using System.Threading;

namespace DGScope
{
    public class RadarWindow
    {
        [XmlIgnore]
        [DisplayName("Background Color"),Description("Radar Background Color"), Category("Colors")] 
        public Color BackColor { get; set; } = Color.Black;
        [XmlIgnore]
        [DisplayName("Range Ring Color"), Category("Colors")]
        public Color RangeRingColor { get; set; } = Color.FromArgb(0,25,0);
        [XmlIgnore]
        [DisplayName("Video Map Line Color"), Category("Colors")]
        public Color VideoMapLineColor { get; set; } = Color.Brown;
        [XmlIgnore]
        [DisplayName("Primary Target Color"), Description("Primary Radar Target color"), Category("Colors")]
        public Color ReturnColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Data Block Color"), Description("Color of aircraft data blocks"), Category("Colors")]
        public Color DataBlockColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Data Block Emergency Color"), Description("Color of emergency aircraft data blocks"), Category("Colors")]
        public Color DataBlockEmergencyColor { get; set; } = Color.Red;
        [XmlIgnore]
        [DisplayName("History Color"), Description("Color of aircraft history targets"), Category("Colors")]
        public Color HistoryColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Connecting Line Color"), Description("Color of the lines which connect the primary returns to their associated data blocks"), Category("Colors")]
        public Color ConnectingLineColor { get; set; } = Color.Lime;

        [XmlElement("BackColor")]
        [Browsable(false)]
        public int BackColorAsArgb
        {
            get { return BackColor.ToArgb(); }
            set { BackColor = Color.FromArgb(value); }
        }
        [XmlElement("RangeRingColor")]
        [Browsable(false)]
        public int RangeRingColorAsArgb
        {
            get { return RangeRingColor.ToArgb(); }
            set { RangeRingColor = Color.FromArgb(value); }
        }
        [XmlElement("VideoMapLineColor")]
        [Browsable(false)]
        public int VideoMapLineColorAsArgb
        {
            get { return VideoMapLineColor.ToArgb(); }
            set { VideoMapLineColor = Color.FromArgb(value); }
        }
        [XmlElement("ReturnColor")]
        [Browsable(false)]
        public int ReturnColorAsArgb
        {
            get { return ReturnColor.ToArgb(); }
            set { ReturnColor = Color.FromArgb(value); }
        }
        [XmlElement("DataBlockColor")]
        [Browsable(false)]
        public int DataBlockColorAsArgb
        {
            get { return DataBlockColor.ToArgb(); }
            set { DataBlockColor = Color.FromArgb(value); }
        }
        [XmlElement("DataBlockEmergencyColor")]
        [Browsable(false)]
        public int DataBlockEmergencyColorAsArgb
        {
            get { return DataBlockEmergencyColor.ToArgb(); }
            set { DataBlockEmergencyColor = Color.FromArgb(value); }
        }
        [XmlElement("HistoryColor")]
        [Browsable(false)]
        public int HistoryColorAsArgb
        {
            get { return HistoryColor.ToArgb(); }
            set { HistoryColor = Color.FromArgb(value); }
        }

        [DisplayName("Fade Time"), Description("The number of seconds the target is faded out over.  A higher number is a slower fade."), Category("Display Properties")]
        public double FadeTime { get; set; } = 30;
        //public int FadeReps { get; set; } = 6;
        [DisplayName("Lost Target Seconds"), Description("The number of seconds before a target's data block is removed from the scope."), Category("Display Properties")]
        public int LostTargetSeconds { get; set; } = 10;
        [DisplayName("Screen Rotation"), Description("The number of degrees to rotate the image"), Category("Display Properties")]
        public double ScreenRotation { get; set; } = 0;
        [DisplayName("Hide Data Tags"), Category("Display Properties")]
        public bool HideDataTags { get; set; } = false;
        [DisplayName("History Fade"), Description("Whether or not the history returns fade out"), Category("Display Properties")]
        public bool HistoryFade { get; set; } = true;
        [DisplayName("History Direction Angle"), Description("Determines direction of drawing history returns.  If true, they are drawn with respect to the aircraft's track.  " +
            "If false, they retain their direction with respect to the receiving radar site."), Category("Display Properties")]
        public bool HistoryDirectionAngle { get; set; } = false;
        [DisplayName("Window State"), Category("Display Properties")]
        public WindowState WindowState
        {
            get
            {
                return window.WindowState;
            }
            set
            {
                window.WindowState = value;
            }
        }
        [DisplayName("Target Frame Rate"), Category("Display Properties")]
        public int TargetFrameRate
        {
            get
            {
                return (int)window.TargetRenderFrequency;
            }
            set
            {
                window.TargetRenderFrequency = value;
            }
        }

        [Browsable(false)]
        public Size WindowSize
        {
            get
            {
                return window.Size;
            }
            set
            {
                window.Size = value;
            }
        }

        [Browsable(false)]
        public Point WindowLocation
        {
            get
            {
                return window.Location;
            }
            set
            {
                window.Location = value;
                if (value == new Point(-32000, -32000))
                {
                    window.Location = new Point(0, 0);
                }
            }
        }
        [DisplayName("Video Maps")]
        [Editor(typeof(VideoMapCollectionEditor), typeof(UITypeEditor))]
        public List<VideoMap> VideoMaps { get; set; } = new List<VideoMap>();
        float scale => (float)(radar.Range / Math.Sqrt(2));
        float xPixelScale => 2f / window.ClientRectangle.Width;
        float yPixelScale => 2f / window.ClientRectangle.Height;
        float aspect_ratio => (float)window.ClientRectangle.Width / (float)window.ClientRectangle.Height;
        float oldar;
        [DisplayName("Range Ring Interval"), Category("Display Properties")]
        public int RangeRingInterval { get; set; } = 5;
        private Radar radar = new Radar();
        [Browsable(false)]
        public bool isRadarRunning  => radar.isRunning;
        [Editor(typeof(ReceiverCollectionEditor), typeof(UITypeEditor))]
        [DisplayName("Receivers"), Description("The collection of data receivers for the Radar scope")]
        public ListOfIReceiver Receivers
        {
            get => radar.Receivers; 
            set
            {
                radar.Receivers = value;
            }
        }
        private GeoPoint _homeLocation = new GeoPoint();
        [DisplayName("Screen Center Point"), Category("Radar Properties")]
        public GeoPoint ScreenCenterPoint
        {
            get => _homeLocation;
            set
            {
                radar.Location = value;
                _homeLocation = value;
            }
        }
        private double _startingRange = 20;
        [DisplayName("Radar Range"), Category("Radar Properties"), Description("About two more minutes, chief!")]
        public double Range
        {
            get => _startingRange;
            set
            {
                radar.Range = value;
                _startingRange = value;
            }
        }
        /*
        [DisplayName("Rotation Period"), Category("Radar Properties"), Description("The number of seconds the radar takes to make one revolution")]
        public double RotationPeriod
        {
            get => radar.RotationPeriod;
            set
            {
                radar.RotationPeriod = value;
            }
        }
        */
        [DisplayName("Max Altitude"), Category("Radar Properties"), Description("The maximum altitude of displayed aircraft.")]
        public int MaxAltitude
        {
            get => radar.MaxAltitude;
            set
            {
                radar.MaxAltitude = value;
            }
        }

        [DisplayName("Minimum Altitude"), Category("Radar Properties"), Description("The minimum altitude of displayed aircraft.")]
        public int MinAltitude
        {
            get => radar.MinAltitude;
            set
            {
                radar.MinAltitude = value;
            }
        }


        [DisplayName("Vertical Sync"),Description("Limit FPS to refresh rate of monitor"), Category("Display Properties")]
        public VSyncMode VSync
        { 
            get
            {
                return window.VSync;
            }
            set
            {
                window.VSync = value;
            }
        }
        [DisplayName("Primary Target Width"), Description("Width of primary targets, in pixels"), Category("Display Properties")]
        public float TargetWidth { get; set; } = 5;
        [DisplayName("Primary Target Height"), Description("Height of primary targets, in pixels"), Category("Display Properties")]
        public float TargetHeight { get; set; } = 15;
        [DisplayName("History Target Width"), Description("Width of history targets, in pixels"), Category("Display Properties")]
        public float HistoryWidth { get; set; } = 5;
        [DisplayName("History Target Height"), Description("Height of history targets, in pixels"), Category("Display Properties")]
        public float HistoryHeight { get; set; } = 15;
        [DisplayName("Nexrad Weather Radars")]
        public List<NexradDisplay> Nexrads { get; set; } = new List<NexradDisplay>();
        [DisplayName("Data Block Font")]
        [XmlIgnore]
        public Font Font { get; set; } = new Font("Consolas", 10);
        [XmlElement("FontName")]
        [Browsable(false)]
        public string FontName { get { return Font.FontFamily.Name; } set { Font = new Font(value, Font.Size); } }
        [XmlElement("FontSize")]
        [Browsable(false)] 
        public int FontSize { get { return (int)Font.Size; } set { Font = new Font(Font.FontFamily, value); } }
        [DisplayName("Speed"), Description("The number of degrees to shift the data block per try"), Category("Data block deconflicting")]
        public double DeconflictCircleSpeed { get; set; } = 5;
        [DisplayName("Deconflict Enabled"), Description("Attempt to deconflict overlapping data blocks"), Category("Data block deconflicting")]
        public bool DeconflictEnabled { get; set; } = true;
        [DisplayName("Max Size"), Description("The maximum number of pixels to move the data block before giving up"), Category("Data block deconflicting")]
        public double DeconflictMaxSize { get; set; } = 100;
        [DisplayName("Max Time"), Description("The maximum number of milliseconds to spend on one data block before giving up"), Category("Data block deconflicting")]
        public double DeconflictMaxTime { get; set; } = 100;
        [DisplayName("Grow Size"), Description("The number of pixels to move the data block per revolution"), Category("Data block deconflicting")]
        public double DeconflictPixelsPerRev { get; set; } = 10;
        [DisplayName("Label Weight"), Description("Weight of label conflicts in determining the best place for the data block"), Category("Data block deconflicting")]
        public int DeconflictLabelWeight { get; set; } = 4;
        [DisplayName("Line Weight"), Description("Weight of line conflicts in determining the best place for the data block"), Category("Data block deconflicting")]
        public int DeconflictLineWeight { get; set; } = 1;
        [DisplayName("Starting Size"), Description("The starting number of pixels to offset the data block from the target"), Category("Data block deconflicting")]
        public float DeconflictStartingSize { get; set; } = 10;
        [DisplayName("Starting Angle"), Description("The starting angle to offset the data block from the target"), Category("Data block deconflicting")]
        public double DeconflictStartingAngle { get; set; } = 45;
        List<PrimaryReturn> PrimaryReturns = new List<PrimaryReturn>();

        private GameWindow window;
        private bool isScreenSaver = false;
        
        public RadarWindow(GameWindow Window)
        {
            window = Window;
            Initialize();
        }
        public RadarWindow()
        {
            window = new GameWindow(1000, 1000);
            Initialize();
        }
        //Thread deconflictThread;
        Timer deconflictTimer;
        private void Initialize()
        {
            window.Load += Window_Load;
            window.Closing += Window_Closing;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Resize += Window_Resize;
            window.WindowStateChanged += Window_WindowStateChanged;
            window.KeyDown += Window_KeyDown;
            window.MouseWheel += Window_MouseWheel;
            window.MouseMove += Window_MouseMove;
            aircraftGCTimer = new Timer(new TimerCallback(cbAircraftGarbageCollectorTimer), null, 60000, 60000);
            deconflictTimer = new Timer(new TimerCallback(Deconflict), null, 100, 1000);
            GL.ClearColor(BackColor);
            string settingsstring = XmlSerializer<RadarWindow>.Serialize(this);
            if (settingsstring != null)
            {
                using (MD5 md5 = MD5.Create())
                {
                    md5.Initialize();
                    md5.ComputeHash(Encoding.UTF8.GetBytes(settingsstring));
                    settingshash = md5.Hash;
                }
            }
            else
            {
                settingshash = new byte[0];
            }
            //deconflictThread = new Thread(new ThreadStart(Deconflict));
            //deconflictThread.IsBackground = true;
        }

        byte[] settingshash;
        public void Run(bool isScreenSaver)
        {
            this.isScreenSaver = isScreenSaver;
            window.Run();
        }

        private void cbAircraftGarbageCollectorTimer(object state)
        {
            List<Aircraft> delplane;
            lock(radar.Aircraft)
                delplane = radar.Aircraft.Where(x => x.LastMessageTime < DateTime.UtcNow.AddMinutes(-1)).ToList();
            foreach (var plane in delplane)
            {
                GL.DeleteTexture(plane.DataBlock.TextureID);
                plane.DataBlock.TextureID = 0;
                //plane.Dispose();
                lock(radar.Aircraft)
                    radar.Aircraft.Remove(plane);
                Debug.WriteLine("Deleted airplane " + plane.ModeSCode.ToString("X"));
            }
        }
        private void Window_WindowStateChanged(object sender, EventArgs e)
        {
            window.CursorVisible = window.WindowState != WindowState.Fullscreen;
        }

        bool _mousesettled = false;
        private void Window_MouseMove(object sender, MouseMoveEventArgs e)
        {
            if (!e.Mouse.IsAnyButtonDown)
            {
                double move = Math.Sqrt(Math.Pow(e.XDelta,2) + Math.Pow(e.YDelta,2));

                if (move > 10 && isScreenSaver && _mousesettled)
                    Environment.Exit(0);
                _mousesettled = true;
               
            }
            else if (e.Mouse.RightButton == ButtonState.Pressed)
            {
                double xMove = e.XDelta * xPixelScale;
                double yMove = e.YDelta * xPixelScale;
                radar.Location = radar.Location.FromPoint(xMove * scale, 270);
                radar.Location = radar.Location.FromPoint(yMove * scale, 0);
                MoveTargets((float)xMove, (float)yMove);
            }

        }
        bool hidewx = false;

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var oldscale = scale;
            if (e.Delta > 0 && radar.Range > 5)
                radar.Range -= 5;
            else if (e.Delta < 0)
                radar.Range += 5;
            RescaleTargets((oldscale / scale), (oldar / aspect_ratio));
            
        }

        private void Window_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            var oldscale = scale;
            switch (e.Key)
            {
                case Key.D:
                    HideDataTags = !HideDataTags;
                    break;
                case Key.W:
                    hidewx = !hidewx;
                    break;
                case Key.F11:
                    if (isScreenSaver)
                        Environment.Exit(0);
                    else
                        window.WindowState = window.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
                    break;
                case Key.KeypadPlus:
                    if (radar.Range > 5)
                        radar.Range -= 5;
                    RescaleTargets((oldscale / scale), (oldar / aspect_ratio));
                    break;
                case Key.KeypadMinus:
                    radar.Range += 5;
                    RescaleTargets((oldscale / scale), (oldar / aspect_ratio));
                    break;
                case Key.R:
                    if (e.Modifiers == KeyModifiers.Shift && RangeRingInterval < radar.Range)
                        RangeRingInterval += 5;
                    else if (RangeRingInterval > 0)
                        RangeRingInterval -= 5;
                    else
                        RangeRingInterval = (int)radar.Range;
                    break;
                case Key.C:
                    dataBlocks.Clear();
                    radar.Aircraft.Clear();
                    break;
                case Key.P:
                    PropertyForm properties = new PropertyForm(this);
                    properties.Show();
                    break;
                case Key.Escape:
                    if (window.WindowState == WindowState.Fullscreen)
                        window.Close();
                    break;
                case Key.ShiftLeft:
                    break;
                case Key.ShiftRight:
                    break;
                case Key.M:
                    VideoMapSelector selector = new VideoMapSelector(VideoMaps);
                    selector.Show();
                    selector.BringToFront();
                    selector.Focus();
                    break;
                default:
                    if (isScreenSaver)
                        Environment.Exit(0);
                    break;
            }
        }


        private void Window_Resize(object sender, EventArgs e)
        {
            
            var oldscale = scale;
            GL.Viewport(0, 0, window.Width, window.Height);
            RescaleTargets((oldscale / scale), (oldar/aspect_ratio));
            lock (dataBlocks)
            {
                dataBlocks.ForEach(x => x.Redraw = true);
            }
            oldar = aspect_ratio;
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
        }

        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            if (window.WindowState == WindowState.Minimized)
                return;

            GL.ClearColor(BackColor);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstAlpha);
            DrawRangeRings();
            if(!hidewx)
                DrawNexrad();
            DrawReceiverLocations();
            DrawLines();
            GenerateTargets();
            DrawTargets();
            GL.Flush();
            window.SwapBuffers();
            window.Title = $"(Vsync: {window.VSync}) FPS: {1f / e.Time:0} Aircraft: {radar.Aircraft.Count}";
        }

        private void Window_Load(object sender, EventArgs e)
        {
            if (isScreenSaver)
            {
                window.WindowState = WindowState.Fullscreen;
                window.CursorVisible = false;
            }
            radar.Start();
            //deconflictThread.Start();
            oldar = aspect_ratio;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            byte[] newhash;
            if (isScreenSaver)
                return;
            using (MD5 md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(XmlSerializer<RadarWindow>.Serialize(this)));
                newhash = md5.Hash;
            }
            bool changed = false;
            if (settingshash.Length != newhash.Length)
            {
                changed = true;
            }
            else
            {
                for (int i = 0; i < settingshash.Length; i++)
                {
                    if (newhash[i] != settingshash[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }
            
            if (changed && !isScreenSaver)
            {
                SaveSettings(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DGScope.xml"));
            }
        }

        public void SaveSettings(string path)
        {
            XmlSerializer<RadarWindow>.SerializeToFile(this, path);
        }
        
        private void DrawRangeRings()
        {
            double bearing = radar.Location.BearingTo(ScreenCenterPoint) - ScreenRotation;
            double distance = radar.Location.DistanceTo(ScreenCenterPoint);
            float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
            float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale) * aspect_ratio);
            for (int i = RangeRingInterval; i <= radar.Range && RangeRingInterval > 0; i += RangeRingInterval)
            {
                DrawCircle(x,y, (float)(i / scale), aspect_ratio, 1000, RangeRingColor);
            }
        }

        private void DrawReceiverLocations()
        {
            foreach (Receiver receiver in radar.Receivers)
            {
                double bearing = radar.Location.BearingTo(receiver.Location) - ScreenRotation;
                double distance = radar.Location.DistanceTo(receiver.Location);
                float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale) * aspect_ratio);
                DrawCircle(x, y, .0025f, aspect_ratio, 100, VideoMapLineColor, true);
            }
        }

        private void DrawCircle (float cx, float cy, float r, double aspect_ratio, int num_segments, Color color, bool fill = false)
        {
            GL.Begin(fill ? PrimitiveType.Polygon : PrimitiveType.LineLoop);
            GL.Color4(color);
            for (int ii = 0; ii < num_segments; ii++)
            {
                float theta = 2.0f * (float)Math.PI * ii / num_segments;
                float x = r * (float)Math.Cos(theta);
                float y = r * (float)Math.Sin(theta) * (float)aspect_ratio;

                GL.Vertex2(x + cx, y + cy);
            }
            GL.End();
        }

        private void DrawLines()
        {
            List<Line> lines = new List<Line>();
            foreach (var map in VideoMaps)
            {
                if (map.Visible)
                    lines.AddRange(map.Lines);
            }
            DrawLines(lines);
        }

        private void DrawLines (List<Line> lines)
        {
            double scale = radar.Range / Math.Sqrt(2); 
            double yscale = (double)window.Width / (double)window.Height;
            foreach (Line line in lines)
            {
                double bearing1 = radar.Location.BearingTo(line.End1) - ScreenRotation;
                double distance1 = radar.Location.DistanceTo(line.End1);
                float x1 = (float)(Math.Sin(bearing1 * (Math.PI / 180)) * (distance1 / scale));
                float y1 = (float)(Math.Cos(bearing1 * (Math.PI / 180)) * (distance1 / scale) * yscale);

                double bearing2 = radar.Location.BearingTo(line.End2) - ScreenRotation;
                double distance2 = radar.Location.DistanceTo(line.End2);
                float x2 = (float)(Math.Sin(bearing2 * (Math.PI / 180)) * (distance2 / scale));
                float y2 = (float)(Math.Cos(bearing2 * (Math.PI / 180)) * (distance2 / scale) * yscale);

                DrawLine(x1, y1, x2, y2, VideoMapLineColor);
            }
        }

        private void DrawPolygon (Polygon polygon)
        {
            double scale = radar.Range / Math.Sqrt(2);
            double yscale = (double)window.Width / (double)window.Height;
            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(polygon.Color);
            for (int i = 0; i < polygon.vertices.Length; i++)
            {
                GL.Vertex2(polygon.vertices[i].X, polygon.vertices[i].Y * yscale);
            }
            GL.End();
        }

        private void DrawNexrad()
        {
            foreach (var nexrad in Nexrads)
            {
                var polygons = nexrad.Polygons(radar.Location, scale, ScreenRotation);
                for (int i = 0; i < polygons.Length; i++)
                {
                    if(polygons[i].Color.A > 0)
                        DrawPolygon(polygons[i]);
                }
            }
        }
        private void DrawLine (float x1, float y1, float x2, float y2, Color color, float width = 1)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(width);
            GL.Color4(color);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);
            GL.End();
        }
        private List<TransparentLabel> dataBlocks = new List<TransparentLabel>();
        private void GenerateTargets()
        {
            List<Aircraft> targets = radar.Scan();
            foreach (Aircraft aircraft in targets)
            {
                double bearing = radar.Location.BearingTo(aircraft.Location) - ScreenRotation; 
                double distance = radar.Location.DistanceTo(aircraft.Location);
                float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale) * aspect_ratio);
                var location = new PointF(x, y);
                if (aircraft.Altitude <= radar.MaxAltitude && aircraft.Altitude >= MinAltitude)
                {
                    aircraft.TargetReturn.ForeColor = HistoryColor;
                    aircraft.TargetReturn.ShapeHeight = HistoryHeight;
                    aircraft.TargetReturn.ShapeWidth = HistoryWidth;
                    if (!HistoryFade)
                    {
                        aircraft.TargetReturn.Fading = false;
                        aircraft.TargetReturn.Intensity = 1;
                    }
                    if (HistoryDirectionAngle)
                    {
                        aircraft.TargetReturn.Angle = (Math.Atan((location.X - aircraft.TargetReturn.LocationF.X) / (location.Y - aircraft.TargetReturn.LocationF.Y)) * (180/Math.PI));
                    }
                    PrimaryReturn newreturn = new PrimaryReturn();
                    aircraft.TargetReturn = newreturn;
                    newreturn.ParentAircraft = aircraft;
                    newreturn.FadeTime = FadeTime;
                    aircraft.RedrawTarget(location);
                    newreturn.NewLocation = location;
                    newreturn.Intensity = 1;
                    newreturn.ForeColor = ReturnColor;
                    newreturn.ShapeHeight = TargetHeight;
                    newreturn.ShapeWidth = TargetWidth;
                    if (!aircraft.Emergency)
                    {
                        aircraft.DataBlock.ForeColor = DataBlockColor;
                    }
                    else
                    {
                        aircraft.DataBlock.ForeColor = DataBlockEmergencyColor;
                    }
                    lock(PrimaryReturns)
                        PrimaryReturns.Add(newreturn);
                    Bitmap text_bmp = aircraft.DataBlock.TextBitmap();
                    var realWidth = text_bmp.Width * xPixelScale;
                    var realHeight = text_bmp.Height * yPixelScale;
                    aircraft.DataBlock.SizeF = new SizeF(realWidth, realHeight);
                    aircraft.DataBlock.ParentAircraft = aircraft;
                    if (!dataBlocks.Contains(aircraft.DataBlock))
                    {
                        aircraft.DataBlock.LocationF = ShiftedLabelLocation(aircraft.LocationF, DeconflictStartingSize * xPixelScale, DeconflictStartingAngle * (Math.PI / 180), aircraft.DataBlock.SizeF);
                        lock (dataBlocks)
                            dataBlocks.Add(aircraft.DataBlock);
                    }
                    else if (!DeconflictEnabled)
                    {
                        aircraft.DataBlock.LocationF = ShiftedLabelLocation(aircraft.LocationF, DeconflictStartingSize * xPixelScale, DeconflictStartingAngle * (Math.PI / 180), aircraft.DataBlock.SizeF);
                        aircraft.DataBlock.NewLocation = aircraft.DataBlock.LocationF;
                    }
                }
                
                else if (aircraft.LastPositionTime < DateTime.UtcNow.AddSeconds(-LostTargetSeconds))
                {
                    lock(dataBlocks)
                        dataBlocks.Remove(aircraft.DataBlock);
                }
                if (aircraft.Altitude > radar.MaxAltitude || aircraft.Altitude < radar.MinAltitude)
                {
                    lock(dataBlocks)
                        dataBlocks.Remove(aircraft.DataBlock);
                }
            }
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                if (target.Intensity < .001)
                {
                    lock(PrimaryReturns)
                        PrimaryReturns.Remove(target);
                }
            }
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    
                }
            }
            
        }

        Timer aircraftGCTimer;

        private void RescaleTargets(float scalechange, float ar_change)
        {
            
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                PointF newLocation = new PointF(target.LocationF.X * scalechange, (target.LocationF.Y * scalechange) / ar_change);
                target.LocationF = newLocation;
            }
            foreach (TransparentLabel block in dataBlocks.ToList())
            {
                PointF newLocation = new PointF(block.LocationF.X * scalechange, (block.LocationF.Y * scalechange) / ar_change);
                block.LocationF = newLocation;
                block.NewLocation = newLocation;
                block.ParentAircraft.ConnectingLine.Start = block.ParentAircraft.TargetReturn.LocationF;
                block.ParentAircraft.ConnectingLine.End = block.LocationF;
            }
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    PointF newLocation = new PointF(plane.LocationF.X * scalechange, (plane.LocationF.Y * scalechange) / ar_change);
                    plane.LocationF = newLocation;
                    plane.DataBlock.Redraw = true;
                }
            }
        }

        private void MoveTargets(float xChange, float yChange)
        {
            yChange = yChange * aspect_ratio;
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                lock (target.ParentAircraft.DeconflictLockObject)
                {
                    target.LocationF = new PointF(target.LocationF.X + xChange, target.LocationF.Y - yChange);
                }
            }
            foreach (TransparentLabel block in dataBlocks.ToList())
            {
                lock (block.ParentAircraft.DeconflictLockObject)
                {
                    block.LocationF = new PointF(block.LocationF.X + xChange, block.LocationF.Y - yChange);
                    block.ParentAircraft.ConnectingLine.Start = block.ParentAircraft.TargetReturn.LocationF;
                    block.ParentAircraft.ConnectingLine.End = block.LocationF;
                    block.NewLocation = block.LocationF;
                }
            }
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    lock (plane.DeconflictLockObject)
                    {
                        plane.LocationF = new PointF(plane.LocationF.X + xChange, plane.LocationF.Y - yChange);
                    }
                }
            }
        }

        private void DrawTarget(PrimaryReturn target)
        {
            float targetHeight = target.ShapeHeight * xPixelScale;// (window.ClientRectangle.Height/2);
            float targetWidth = target.ShapeWidth * yPixelScale;// (window.ClientRectangle.Width/2);
            float atan = (float)Math.Atan(targetHeight / targetWidth);
            float targetHypotenuse = (float)(Math.Sqrt((targetHeight*targetHeight) + (targetWidth * targetWidth))/2);
            float x1 = (float)(Math.Sin(atan) * targetHypotenuse);
            float y1 = (float)(Math.Cos(atan) * targetHypotenuse);
            float circleradius = 4f * xPixelScale;
            //target.SizeF = new SizeF(2 * circleradius, 2 * circleradius * aspect_ratio);
            //DrawCircle(target.LocationF.X, target.LocationF.Y, circleradius, aspect_ratio, 25, target.ForeColor, true);
            
            target.SizeF = new SizeF(targetHypotenuse * 2, targetHypotenuse * 2 * aspect_ratio);
            
            GL.LoadIdentity();
            GL.PushMatrix();

            float angle = (float)(-(target.Angle + 360) % 360) + (float)ScreenRotation;
            GL.Translate(target.LocationF.X, target.LocationF.Y, 0.0f);
            GL.Scale(1.0f, aspect_ratio, 1.0f);
            GL.Rotate(angle, 0.0f, 0.0f, 1.0f);
            GL.Ortho(-1.0f, 1.0f, -aspect_ratio, aspect_ratio, 0.1f, 0.0f);
            GL.Begin(PrimitiveType.Polygon);
            
            GL.Color4(target.ForeColor);
            GL.Vertex2(x1, y1);
            GL.Vertex2(-x1, y1);
            GL.Vertex2(-x1, -y1);
            GL.Vertex2(x1, -y1);
            

            GL.End();
            GL.Translate(-target.LocationF.X, -target.LocationF.Y , 0.0f);
            

            GL.PopMatrix();
            
        }

        private void DrawTargets()
        {
            foreach (var target in PrimaryReturns.OrderBy(x => x.Intensity).ToList())
            {
                DrawTarget(target);
            }
            foreach (var block in dataBlocks.ToList().OrderBy(x=>x.ParentAircraft.ModeSCode))
            {
                if (!HideDataTags)
                {
                    DrawLabel(block);
                }
            }
        }

        private void DrawLabel(TransparentLabel Label)
        {
            if (!radar.Aircraft.Contains(Label.ParentAircraft))
                return;
            GL.Enable(EnableCap.Texture2D);
            if (Label.TextureID == 0)
                Label.TextureID = GL.GenTexture();
            var text_texture = Label.TextureID;
            if (Label.Redraw && Label.Text.Trim() != "")
            {
                Label.Font = Font;
                Bitmap text_bmp = Label.TextBitmap();
                var realWidth = (float)text_bmp.Width * xPixelScale;
                var realHeight = (float)text_bmp.Height * yPixelScale;
                Label.SizeF = new SizeF(realWidth, realHeight);
                GL.BindTexture(TextureTarget.Texture2D, text_texture);
                BitmapData data = text_bmp.LockBits(new Rectangle(0, 0, text_bmp.Width, text_bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                text_bmp.UnlockBits(data);
                Label.Redraw = false;
                //text_bmp.Save($"{text_texture}.bmp");
            }
            
            GL.BindTexture(TextureTarget.Texture2D, text_texture);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(Label.ForeColor);
            
            //Deconflict(Label);
            var Location = Label.LocationF;
            GL.TexCoord2(0,0);
            GL.Vertex2(Location.X, Label.SizeF.Height + Location.Y);
            GL.TexCoord2(1, 0); 
            GL.Vertex2(Label.SizeF.Width + Location.X, Label.SizeF.Height + Location.Y);
            GL.TexCoord2(1, 1); 
            GL.Vertex2(Label.SizeF.Width + Location.X, Location.Y);
            GL.TexCoord2(0, 1); 
            GL.Vertex2(Location.X, Location.Y);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);

            
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(ConnectingLineColor);
            
            ConnectingLineF line = new ConnectingLineF();
            line.ParentAircraft = Label.ParentAircraft;
            line.End = ConnectingLinePoint(Label.ParentAircraft.TargetReturn.BoundsF, Label.BoundsF);
            line.Start = ConnectingLinePoint(Label.BoundsF, Label.ParentAircraft.TargetReturn.BoundsF);
            GL.Vertex2(line.Start.X, line.Start.Y);
            GL.Vertex2(line.End.X, line.End.Y);
            GL.End();
            
            
        }

        private void DrawAllScreenObjectBounds()
        {
            List<IScreenObject> screenObjects = new List<IScreenObject>();
            lock (dataBlocks)
                screenObjects.AddRange(dataBlocks);
            
            foreach (var item in screenObjects)
            {
                var bounds = new RectangleF(item.NewLocation, item.SizeF);
                if (hasConflicts(item))
                    DrawRectangle(bounds, Color.Red);
                else
                    DrawRectangle(bounds, Color.Gray);
            }
        }

        private bool hasConflicts (IScreenObject screenObject)
        {
            return conflictScore(screenObject) > 0;
        }

        private int conflictScore(IScreenObject thisObject)
        {
            List<IScreenObject> screenObjects = new List<IScreenObject>();
            lock (PrimaryReturns)
            {
                screenObjects.AddRange(PrimaryReturns.ToList());
            }
            lock (dataBlocks)
            {
                screenObjects.AddRange(dataBlocks.ToList());
            }
            screenObjects.Remove(thisObject);
            int score = 0;
            RectangleF newBounds = new RectangleF(thisObject.NewLocation, thisObject.SizeF);
            var thisAircraft = thisObject.ParentAircraft;
            foreach (var otherObject in screenObjects)
            {
                var otherAircraft = otherObject.ParentAircraft;
                RectangleF othernewBounds = new RectangleF(otherObject.NewLocation, otherObject.SizeF);
                if (othernewBounds.IntersectsWith(newBounds))
                {
                    score += DeconflictLabelWeight;
                    if (otherObject.GetType() == typeof(ConnectingLineF))
                        score += DeconflictLineWeight;
                    else
                        score += DeconflictLabelWeight;
                }
                if (thisAircraft.ConnectingLine.IntersectsWith(othernewBounds) && otherAircraft != thisAircraft)
                {
                    score += DeconflictLabelWeight;
                }
                if (thisAircraft.ConnectingLine.IntersectsWith(otherAircraft.ConnectingLine) && otherAircraft != thisAircraft)
                {
                    score += DeconflictLineWeight;
                }
                if (otherAircraft.ConnectingLine.IntersectsWith(newBounds) && otherAircraft != thisAircraft)
                {
                    score += DeconflictLabelWeight;
                }
                if (otherAircraft.ConnectingLine.IntersectsWith(thisAircraft.ConnectingLine) && thisAircraft != otherAircraft)
                {
                    score += DeconflictLineWeight;
                }
            }
            return score;
        }

        private void DrawScreenObjectBounds(IScreenObject screenObject, Color color)
        {
            DrawRectangle(screenObject.BoundsF, color, false);
        }
        private void DrawRectangle(RectangleF rectangle, Color color, bool fill = false)
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(color);
            GL.Vertex2(rectangle.Left, rectangle.Top);
            GL.Vertex2(rectangle.Right,rectangle.Top);
            GL.Vertex2(rectangle.Right,rectangle.Top);
            GL.Vertex2(rectangle.Right,rectangle.Bottom);
            GL.Vertex2(rectangle.Right,rectangle.Bottom);
            GL.Vertex2(rectangle.Left, rectangle.Bottom);
            GL.Vertex2(rectangle.Left, rectangle.Bottom);
            GL.Vertex2(rectangle.Left, rectangle.Top);
            GL.End();
        }
        private void Deconflict(object state)
        {
            if (DeconflictEnabled)
            {
                List<TransparentLabel> blocks;
                lock (dataBlocks)
                    blocks = dataBlocks.OrderBy(x => x.ParentAircraft.ModeSCode).ToList();
                foreach (var label in blocks)
                {
                    bool locked = false;
                    try
                    {
                        locked = Monitor.TryEnter(label.ParentAircraft.DeconflictLockObject);
                        if (locked)
                        {
                            Deconflict(label);
                        }
                        else
                        {
                            Debug.WriteLine("{0} is already locked... skipping.", label.ParentAircraft);
                        }
                    }
                    finally
                    {
                        if (locked)
                        {
                            Monitor.Exit(label.ParentAircraft.DeconflictLockObject);
                        }
                    }
                }
                foreach (var label in blocks)
                {
                    label.LocationF = label.NewLocation;
                }
            }
            
        }
        
        private void Deconflict(IScreenObject ThisObject)
        {
            Stopwatch deconflictStopwatch = new Stopwatch();

            if (!DeconflictEnabled)
                return;
            deconflictStopwatch.Restart();
            if (window.WindowState == WindowState.Minimized || window.Width == 0 || window.Height == 0)
                return;
            RectangleF newBounds = ThisObject.BoundsF;
            double circlespeed = DeconflictCircleSpeed * (Math.PI / 180);
            float growsize = (float)(DeconflictPixelsPerRev * DeconflictCircleSpeed  * xPixelScale/360);
            ConnectingLineF connectingLine = new ConnectingLineF();
            connectingLine.Start = ConnectingLinePoint(ThisObject.ParentAircraft.TargetReturn.BoundsF, ThisObject.BoundsF);
            connectingLine.End = ConnectingLinePoint(ThisObject.BoundsF, ThisObject.ParentAircraft.TargetReturn.BoundsF);
            connectingLine.ParentAircraft = ThisObject.ParentAircraft;
            int loopcount = 0;
            
            int conflictcount;
            
            float circleSize = DeconflictStartingSize * xPixelScale;
            double angle = DeconflictStartingAngle * (Math.PI / 180);
            List<IScreenObject> screenObjects = new List<IScreenObject>();
            lock (PrimaryReturns)
            {
                screenObjects.AddRange(PrimaryReturns.ToList());
                screenObjects.AddRange(from x in PrimaryReturns select x.ParentAircraft.ConnectingLine);
            }
            lock (dataBlocks)
                screenObjects.AddRange(dataBlocks.ToList());
            screenObjects.Remove(ThisObject);
            int minConflictCount = int.MaxValue;
            float bestSize = float.MaxValue;
            double bestAngle = 0;

            do
            {
                
                loopcount++;
                ThisObject.NewLocation = ShiftedLabelLocation(ThisObject.ParentAircraft.LocationF, circleSize, angle, ThisObject.SizeF);
                newBounds = new RectangleF(ThisObject.NewLocation, ThisObject.SizeF);
                conflictcount = conflictScore(ThisObject);
                connectingLine.Start = ConnectingLinePoint(ThisObject.ParentAircraft.TargetReturn.BoundsF, newBounds);
                connectingLine.End = ConnectingLinePoint(newBounds, ThisObject.ParentAircraft.TargetReturn.BoundsF);
                ThisObject.ParentAircraft.ConnectingLine = connectingLine;
                angle += circlespeed;
                circleSize += growsize;
                if (conflictcount < minConflictCount)
                {
                    minConflictCount = conflictcount;
                    bestAngle = angle;
                    bestSize = circleSize;
                }
                else if (conflictcount == minConflictCount && circleSize < bestSize)
                {
                    bestSize = circleSize;
                    bestAngle = angle;
                }
                else if (circleSize > DeconflictMaxSize * xPixelScale)
                {
                    Debug.WriteLine("Giving up trying to deconflict {0} after {1} tries.  Leaving it with {2} conflicts and a circle size of {3} pixels.",ThisObject.ParentAircraft,loopcount,conflictcount,bestSize/xPixelScale);
                    ThisObject.NewLocation = ShiftedLabelLocation(ThisObject.ParentAircraft.LocationF, bestSize, bestAngle, ThisObject.SizeF); 
                    newBounds = new RectangleF(ThisObject.NewLocation, ThisObject.SizeF);
                    connectingLine.Start = ConnectingLinePoint(ThisObject.ParentAircraft.TargetReturn.BoundsF, newBounds);
                    connectingLine.End = ConnectingLinePoint(newBounds, ThisObject.ParentAircraft.TargetReturn.BoundsF);
                    return;
                }
            } while (hasConflicts(ThisObject)) ;
        }


        public PointF ConnectingLinePoint(RectangleF Start, RectangleF End)
        {
            PointF EndPoint = new PointF();

            if (End.Right < Start.Left)
                EndPoint.X = End.Right;
            else if (End.Left > Start.Right)
                EndPoint.X = End.Left;
            else
                EndPoint.X = End.Left + (End.Width / 2);

            if (End.Top > Start.Bottom)
                EndPoint.Y = End.Top;
            else if (End.Bottom < Start.Top)
                EndPoint.Y = End.Bottom;
            else
                EndPoint.Y = End.Bottom - (End.Height / 2);
            return EndPoint;
        }
        public PointF ShiftedLabelLocation(PointF StartPoint, float radius, double angle, SizeF Label)
        {
            PointF EndPoint = new PointF();
            double degrees = ((angle * (180 / Math.PI)) + 360) % 360;

            if (degrees > 315 || degrees < 45)
            {
                EndPoint.X = StartPoint.X + (float)Math.Cos(angle) * radius; //X bound to left
            }
            else if (degrees > 135 && degrees < 225)
            {
                EndPoint.X = StartPoint.X + ((float)Math.Cos(angle) * radius) - Label.Width; //X bound to right
            }
            else
            {
                EndPoint.X = StartPoint.X + ((float)Math.Cos(angle) * radius) - (Label.Width / 2);//X bound to mid
            }

            if (degrees > 45 && degrees < 135)
            {
                EndPoint.Y = StartPoint.Y + (float)Math.Sin(angle) * radius; //Y bound to bottom
            }
            else if (degrees > 224 && degrees < 315)
            {
                EndPoint.Y = StartPoint.Y + ((float)Math.Sin(angle) * radius) + Label.Height; //Y bound to top
            }
            else
            {
                EndPoint.Y = StartPoint.Y + ((float)Math.Sin(angle) * radius) + (Label.Height / 2); //Y bound to mid
            }
            return EndPoint;
        }
        
    }

    
    

    
    public interface IScreenObject
    {
        PointF LocationF { get; set; }
        PointF NewLocation { get; set; }
        SizeF SizeF { get;  }
        RectangleF BoundsF { get; }
        Aircraft ParentAircraft { get; set; }
    }
}
