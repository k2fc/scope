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
using libmetar;
using System.Windows.Forms.Design;
using System.Threading.Tasks;

namespace DGScope
{
    public class RadarWindow
    {
        public enum LeaderDirection
        {
            NW = 135, 
            N  = 90,
            NE = 45,
            W  = 180,
            E  = 0,
            SW = 225,
            S  = 270, 
            SE = 315
        }

        public static LeaderDirection ParseLDR(string direction)
        {
            direction = direction.ToUpper().Trim();
            switch (direction)
            {
                case "NW":
                    return LeaderDirection.NW;
                case "N":
                    return LeaderDirection.N;
                case "NE":
                    return LeaderDirection.NE;
                case "W":
                    return LeaderDirection.W;
                case "E":
                    return LeaderDirection.E;
                case "SW":
                    return LeaderDirection.SW;
                case "S":
                    return LeaderDirection.S;
                case "SE":
                    return LeaderDirection.SE;
                default:
                    return LeaderDirection.N;
            }
        }
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
        [DisplayName("FDB Color"), Description("Color of aircraft full data blocks"), Category("Colors")]
        public Color DataBlockColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Owned FDB Color"), Description("Color of aircraft owned full data blocks"), Category("Colors")]
        public Color OwnedColor { get; set; } = Color.White;
        [XmlIgnore]
        [DisplayName("LDB Color"), Description("Color of aircraft limited data blocks"), Category("Colors")]
        public Color LDBColor { get; set; } = Color.Green;
        [XmlIgnore]
        [DisplayName("Selected Data Block Color"), Description("Color of aircraft limited data blocks"), Category("Colors")]
        public Color SelectedColor { get; set; } = Color.Aqua;
        [XmlIgnore]
        [DisplayName("Data Block Emergency Color"), Description("Color of emergency aircraft data blocks"), Category("Colors")]
        public Color DataBlockEmergencyColor { get; set; } = Color.Red;
        [XmlIgnore]
        [DisplayName("History Color"), Description("Color of aircraft history targets"), Category("Colors")]
        public Color HistoryColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("RBL Color"), Description("Color of range bearing lines"), Category("Colors")]
        public Color RBLColor { get; set; } = Color.Silver;

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
        [XmlElement("LDBColor")]
        [Browsable(false)]
        public int LDBColorAsArgb
        {
            get { return LDBColor.ToArgb(); }
            set { LDBColor = Color.FromArgb(value); }
        }
        [XmlElement("SelectedColor")]
        [Browsable(false)]
        public int SelectedColorAsArgb
        {
            get { return SelectedColor.ToArgb(); }
            set { SelectedColor = Color.FromArgb(value); }
        }
        [XmlElement("OwnedColor")]
        [Browsable(false)]
        public int OwnedColorAsArgb
        {
            get { return OwnedColor.ToArgb(); }
            set { OwnedColor = Color.FromArgb(value); }
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
        [XmlElement("RBLColor")]
        [Browsable(false)]
        public int RBLColorAsArgb
        {
            get { return RBLColor.ToArgb(); }
            set { RBLColor = Color.FromArgb(value); }
        }


        [DisplayName("Fade Time"), Description("The number of seconds the target is faded out over.  A higher number is a slower fade."), Category("Display Properties")]
        public double FadeTime { get; set; } = 30;
        [DisplayName("History Drop Interval"), Description("The interval at which history is drawn.  Lower numbers mean more frequent history.  Set to 0 for a history dropped at every location"), Category("Display Properties")]
        public double HistoryInterval { get; set; } = 0;
        [DisplayName("Lost Target Seconds"), Description("The number of seconds before a target's data block is removed from the scope."), Category("Display Properties")]
        public int LostTargetSeconds { get; set; } = 10;
        [DisplayName("Aircraft Database Cleanup Interval"), Description("The number of seconds between removing aircraft from memory."), Category("Display Properties")]
        public int AircraftGCInterval { get; set; } = 600;

        [DisplayName("Screen Rotation"), Description("The number of degrees to rotate the image"), Category("Display Properties")]
        public double ScreenRotation { get; set; } = 0;
        [DisplayName("Hide Data Tags"), Category("Display Properties")]
        public bool HideDataTags { get; set; } = false;
        [DisplayName("Quick Look"), Description("Show FDB on all"), Category("Display Properties")]
        public bool QuickLook { get; set; } = false;
        [DisplayName("Timeshare Interval"), Description("Interval at which to rotate text in data blocks"), Category("Display Properties")]
        public double TimeshareInterval
        {
            get
            {
                if (dataBlockTimeshareTimer == null)
                    return 1;
                return timeshareinterval / 1000d;
            }
            set
            {
                if (value != timeshareinterval && dataBlockTimeshareTimer != null)
                {
                    dataBlockTimeshareTimer.Change(0, (int)(value * 1000));
                }
                timeshareinterval = (int)(value * 1000d);
            }
        }
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
        float xPixelScale => 2f / window.ClientSize.Width;
        float yPixelScale => 2f / window.ClientSize.Height;
        float aspect_ratio => (float)window.ClientSize.Width / (float)window.ClientSize.Height;
        float oldar;
        [DisplayName("Range Ring Interval"), Category("Display Properties")]
        public int RangeRingInterval { get; set; } = 5;
        private string cps = "NONE";
        [DisplayName("This Position Indicator"), Category("Display Properties")]
        public string ThisPositionIndicator 
        {
            get
            {
                return cps;
            }
            set
            {
                if (cps != value)
                {
                    cps = value;
                    PositionChange();
                }
            }
        } 
        [DisplayName("Airports file"), Category("Navigation Data")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string AirportsFileName
        {
            get
            {
                return airportsFileName;
            }
            set
            {
                try
                {
                    radar.Airports = XmlSerializer<Airports>.DeserializeFromFile(value);
                    airportsFileName = value;
                    OrderWaypoints();
                }
                catch
                {
                    radar.Airports = new Airports();
                }
            }
        }
        private string airportsFileName = "";
        [DisplayName("Waypoints file"), Category("Navigation Data")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string WaypointsFileName
        {
            get
            {
                return waypointsFileName;
            }
            set
            {
                try
                {
                    radar.Waypoints = XmlSerializer<Waypoints>.DeserializeFromFile(value);
                    waypointsFileName = value;
                    OrderWaypoints();
                }
                catch
                {
                    radar.Airports = new Airports();
                }
            }
        }
        private string waypointsFileName = "";
        [DisplayName("Altimeter Stations"), Category("Display Properties")]
        public string[] AltimeterStations 
        {
            get
            {
                return radar.AltimeterStations.ToArray();
            }
            set
            {
                radar.AltimeterStations = value.ToList();
                cbWxUpdateTimer(null);
            }
        }
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
                OrderWaypoints();
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
        [DisplayName("Primary Target Shape"), Description("Shape of primary targets"), Category("Display Properties")]
        public TargetShape TargetShape { get; set; } = TargetShape.Circle;
        [DisplayName("History Target Width"), Description("Width of history targets, in pixels"), Category("Display Properties")]
        public float HistoryWidth { get; set; } = 5;
        [DisplayName("History Target Height"), Description("Height of history targets, in pixels"), Category("Display Properties")]
        public float HistoryHeight { get; set; } = 15;
        [DisplayName("PTL Length"), Description("Length of Predicted Track Lines for owned tracks, in minutes"), Category("Display Properties")]
        public float PTLlength { get; set; } = 1;
        [DisplayName("PTL Length"), Description("Length of Predicted Track Lines for unowned tracks, in minutes"), Category("Display Properties")]
        public float PTLlengthAll { get; set; } = 0;
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
        [DisplayName("Auto Offset Enabled"), Description("Attempt to deconflict overlapping data blocks"), Category("Data block deconflicting")]
        public bool AutoOffset { get; set; } = true;
        [DisplayName("Leader Length"), Description("The number of pixels to offset the data block from the target"), Category("Data block deconflicting")]
        public float LeaderLength { get; set; } = 10;
        [DisplayName("Leader Direction"), Description("The angle to offset the data block from the target"), Category("Data block deconflicting")]
        public LeaderDirection LDRDirection { get; set; } = LeaderDirection.N;
        [Browsable(false)]
        public PointF PreviewLocation
        {
            get
            {
                return PreviewArea.LocationF;
            }
            set
            {
                PreviewArea.LocationF = value;
            }
        }
        [Browsable(false)]
        public PointF StatusLocation
        {
            get
            {
                return StatusArea.LocationF;
            }
            set
            {
                StatusArea.LocationF = value;
            }
        }
        [DisplayName("Wind in Status Area"), Description("Show wind values in Status Area"), Category("Display Properties")]
        public bool WindInStatusArea { get; set; } = false;
        [DisplayName("FPS in Status Area"), Description("Show FPS in Status Area"), Category("Display Properties")]
        public bool FPSInStatusArea { get; set; } = false;
        List<PrimaryReturn> PrimaryReturns = new List<PrimaryReturn>();

        private GameWindow window;
        private bool isScreenSaver = false;

        private TransparentLabel PreviewArea = new TransparentLabel()
        {
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true
        };
        private TransparentLabel StatusArea = new TransparentLabel()
        {
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true
        };
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
        List<RangeBearingLine> rangeBearingLines = new List<RangeBearingLine>();
        Timer wxUpdateTimer;
        int timeshareinterval = 1000;
        Timer dataBlockTimeshareTimer;
        List<WaypointsWaypoint> Waypoints;
        List<Airport> Airports;
        byte timeshare = 0;
        private void Initialize()
        {
            window.Title = "DGScope";
            window.Load += Window_Load;
            window.Closing += Window_Closing;
            window.RenderFrame += Window_RenderFrame;
            window.UpdateFrame += Window_UpdateFrame;
            window.Resize += Window_Resize;
            window.WindowStateChanged += Window_WindowStateChanged;
            window.KeyDown += Window_KeyDown;
            window.KeyUp += Window_KeyUp;
            window.MouseWheel += Window_MouseWheel;
            window.MouseMove += Window_MouseMove;
            window.MouseDown += Window_MouseDown;
            aircraftGCTimer = new Timer(new TimerCallback(cbAircraftGarbageCollectorTimer), null, AircraftGCInterval * 1000, AircraftGCInterval * 1000);
            wxUpdateTimer = new Timer(new TimerCallback(cbWxUpdateTimer), null, 0, 180000);
            dataBlockTimeshareTimer = new Timer(new TimerCallback(cbTimeshareTimer), null, 0, timeshareinterval);
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
            OrderWaypoints();
            radar.Aircraft.CollectionChanged += Aircraft_CollectionChanged;
        }

        private void Aircraft_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (Aircraft item in e.NewItems)
                    {
                        item.HandedOff += Aircraft_HandedOff;
                        item.HandoffInitiated += Aircraft_HandoffInitiated;
                        item.OwnershipChange += Aircraft_OwnershipChange;
                        item.Altitude.SetAltitudeProperties(18000, radar.Altimeter);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (Aircraft item in e.OldItems)
                    {
                        item.HandedOff -= Aircraft_HandedOff;
                        item.OwnershipChange -= Aircraft_OwnershipChange;
                        item.HandoffInitiated -= Aircraft_HandoffInitiated;
                    }
                    break;
            }
        }

        private void Aircraft_HandoffInitiated(object sender, HandoffEventArgs e)
        {
            if (e.PositionTo == ThisPositionIndicator)
            {
                if (e.Aircraft.Owned && e.Aircraft.DataBlock.Flashing)
                    return;
                e.Aircraft.Owned = true;
                e.Aircraft.DataBlock.Flashing = true;
                e.Aircraft.DataBlock2.Flashing = true;
                e.Aircraft.DataBlock3.Flashing = true;
            }
            /*if (e.Aircraft.LastPositionTime > DateTime.Now.AddSeconds(-LostTargetSeconds))
                GenerateDataBlock(e.Aircraft);*/
        }

        private void Aircraft_OwnershipChange(object sender, AircraftEventArgs e)
        {
            /*e.Aircraft.RedrawDataBlock();*/
        }
        private void PositionChange()
        {
            lock (radar.Aircraft)
            {
                radar.Aircraft.ToList().ForEach(x => x.Owned = x.PositionInd == ThisPositionIndicator || x.PendingHandoff == ThisPositionIndicator);
                radar.Aircraft.ToList().ForEach(x => x.DataBlock.Flashing = x.PendingHandoff == ThisPositionIndicator);
                radar.Aircraft.ToList().ForEach(x => x.DataBlock2.Flashing = x.PendingHandoff == ThisPositionIndicator);
                radar.Aircraft.ToList().ForEach(x => x.DataBlock3.Flashing = x.PendingHandoff == ThisPositionIndicator);
            }
        }
        private void Aircraft_HandedOff(object sender, HandoffEventArgs e)
        {
            Console.WriteLine("{0} handed {1} to {2}", e.PositionFrom, e.Aircraft, e.PositionTo);
        }
            
        byte[] settingshash;
        public void Run(bool isScreenSaver)
        {
            this.isScreenSaver = isScreenSaver;
            window.Run();
        }

        private void cbWxUpdateTimer(object state)
        {
            radar.GetWeather(true);
        }

        private void cbTimeshareTimer(object state)
        {
            timeshare++;
            timeshare %= 4;
        }

        private void cbAircraftGarbageCollectorTimer(object state)
        {
            List<Aircraft> delplane;
            lock(radar.Aircraft)
                delplane = radar.Aircraft.Where(x => x.LastMessageTime < DateTime.UtcNow.AddSeconds(-AircraftGCInterval)).ToList();
            foreach (var plane in delplane)
            {
                GL.DeleteTexture(plane.DataBlock.TextureID);
                plane.DataBlock.TextureID = 0;
                //plane.Dispose();
                lock(radar.Aircraft)
                    radar.Aircraft.Remove(plane);
                lock (dataBlocks)
                    dataBlocks.Remove(plane.DataBlock);
                lock (posIndicators)
                    posIndicators.Remove(plane.PositionIndicator);
                Debug.WriteLine("Deleted airplane " + plane.ModeSCode.ToString("X"));
                lock (rangeBearingLines)
                    rangeBearingLines.RemoveAll(line => line.EndPlane == plane || line.StartPlane == plane);
            }
        }

        private void OrderWaypoints()
        {
            try
            {
                Airports = radar.Airports.Airport.ToList();
            }
            catch 
            { 
                radar.Airports.Airport = new Airport[1] { new Airport() };
                Airports = radar.Airports.Airport.ToList();
            }
            try
            {
                Waypoints = radar.Waypoints.Waypoint.ToList().OrderBy(x => x.Location.DistanceTo(ScreenCenterPoint)).ToList();
            }
            catch 
            { 
                radar.Waypoints.Waypoint = new WaypointsWaypoint[1] { new WaypointsWaypoint() };
                Waypoints = radar.Waypoints.Waypoint.ToList();
            }
        }
        private void Window_WindowStateChanged(object sender, EventArgs e)
        {
            //window.CursorVisible = window.WindowState != WindowState.Fullscreen;
        }

        bool _mousesettled = false;
        Point MouseLocation = new Point(0, 0);
        private void Window_MouseMove(object sender, MouseMoveEventArgs e)
        {
            MouseLocation = e.Position;
            if (tempLine != null)
                tempLine.End = LocationFromScreenPoint(e.Position);
            if (!e.Mouse.IsAnyButtonDown)
            {
                double move = Math.Sqrt(Math.Pow(e.XDelta,2) + Math.Pow(e.YDelta,2));

                if (move > 10 && isScreenSaver && _mousesettled)
                {
                    radar.Stop();
                    Environment.Exit(0);
                }
                _mousesettled = true;
               
            }
            else if (e.Mouse.RightButton == ButtonState.Pressed)
            {
                double xMove = e.XDelta * xPixelScale;
                double yMove = e.YDelta * xPixelScale;
                radar.Location = radar.Location.FromPoint(xMove * scale, 270 + ScreenRotation);
                radar.Location = radar.Location.FromPoint(yMove * scale, 0 + ScreenRotation);
                MoveTargets((float)xMove, (float)yMove);
            }

        }
        bool hidewx = false;
        private object ClickedObject(Point ClickedPoint)
        {
            var clickpoint = LocationFromScreenPoint(ClickedPoint);
            object clicked;
            lock (radar.Aircraft)
            {
                clicked = radar.Aircraft.Where(x => x.TargetReturn.BoundsF.Contains(clickpoint) 
                && x.LastPositionTime > DateTime.Now.AddSeconds(-LostTargetSeconds) 
                && x.TargetReturn.Intensity > .001).FirstOrDefault();
                if (clicked == null)
                {
                    clicked = clickpoint;
                }
            }
            return clicked;
        }
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            var clicked = ClickedObject(e.Position);
            if (e.Mouse.LeftButton == ButtonState.Pressed)
            {
                if (tempLine == null) 
                    ProcessCommand(Preview, clicked);
                else if (clicked.GetType() == typeof(Aircraft))
                {
                    tempLine.EndPlane = (Aircraft)clicked;
                    tempLine = null;
                    Preview.Clear();
                }
                else
                {
                    tempLine.EndGeo = ScreenToGeoPoint(e.Position);
                    tempLine = null;
                    Preview.Clear();
                }
            }
            else if (e.Mouse.MiddleButton == ButtonState.Pressed)
            {
                if (clicked.GetType() == typeof(Aircraft))
                {
                    Aircraft plane = (Aircraft)clicked;
                    plane.Marked = plane.Marked ? false : true;
                    GenerateDataBlock(plane);
                }
            }
        }

        private void Click()
        {

        }

        private PointF LocationFromScreenPoint(Point point)
        {
            float x = (point.X * xPixelScale) -1;
            float y = 1 - (point.Y * yPixelScale);
            return new PointF(x, y);
        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var oldscale = scale;
            if (e.Delta > 0 && radar.Range > 5)
                radar.Range -= 5;
            else if (e.Delta < 0)
                radar.Range += 5;
            RescaleTargets((oldscale / scale), (oldar / aspect_ratio));
            
        }

        public enum KeyCode
        {
            
            Triangle = 119,
            Min = 59, 
            InitCntl = 12,
            TermCntl = 13,
            HndOff = 14,
            VP = 15,
            MultiFunc = 16,
            FltData = 18,
            CA = 20,
            SignOn = 21
        }

        public List<KeyCode> Preview = new List<KeyCode>();


        bool waitingfortarget = false;
        RangeBearingLine tempLine;
        private void ProcessCommand(List<KeyCode> KeyList, object clicked = null)
        {
            bool clickedplane = false;
            bool enter = false;
            
            if (clicked != null)
                clickedplane = clicked.GetType() == typeof(Aircraft);
            else
            {
                enter = true;
            }

            if (KeyList.Count < 1 && clicked.GetType() == typeof(Aircraft))
            {
                Aircraft plane = (Aircraft)clicked;
                if (!plane.Owned)
                    plane.FDB = plane.FDB ? false : true;
                else if (plane.PositionInd != ThisPositionIndicator)
                    plane.Owned = false;
                GenerateDataBlock(plane);
            }
            else if (KeyList.Count > 0)
            {
                var commands = KeyList.Count(x => (int)x == (int)Key.Space) + 1;
                var count = 0;
                KeyCode[][] keys = new KeyCode[commands][];
                for (int i = 0; i < commands; i++)
                {
                    List<KeyCode> command = new List<KeyCode>();
                    for (; count < KeyList.Count; count++)
                    {
                        if ((int)KeyList[count] != (int)Key.Space) 
                        {
                            command.Add(KeyList[count]);
                        }
                        else
                        {
                            count++;
                            break;
                        }
                            
                    }
                    keys[i] = command.ToArray();
                }
                string lastline = KeysToString(keys[commands - 1]);
                Aircraft typed;
                lock (radar.Aircraft)
                    typed = radar.Aircraft.Where(x=> x.Callsign != null).ToList()
                        .Find(x => x.Callsign.Trim() == lastline.Trim());
                if (typed != null)
                {
                    if (typed.Squawk != "1200" && typed.Squawk!= null )
                    {
                        clicked = typed;
                        clickedplane = true;
                    }
                }
                switch ((int)keys[0][0])
                {
                    case (int)Key.Keypad1:
                    case (int)Key.Number1:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.NW;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.Keypad2:
                    case (int)Key.Number2:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.N;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.Keypad3:
                    case (int)Key.Number3:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.NE;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.Keypad4:
                    case (int)Key.Number4:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.W;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.Keypad6:
                    case (int)Key.Number6:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.E;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.Keypad7:
                    case (int)Key.Number7:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.SW;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.Keypad8:
                    case (int)Key.Number8:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.S;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.Keypad9:
                    case (int)Key.Number9:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.SE;
                            ((Aircraft)clicked).RedrawDataBlock();
                            Preview.Clear();
                        }
                        break;
                    case (int)KeyCode.InitCntl:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).Owned = true;
                            ((Aircraft)clicked).PositionInd = ThisPositionIndicator;
                            GenerateDataBlock((Aircraft)clicked);
                            Preview.Clear();
                        }
                        break;
                    case (int)KeyCode.TermCntl:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).Owned = false;
                            ((Aircraft)clicked).PositionInd = null;
                            GenerateDataBlock((Aircraft)clicked);
                            Preview.Clear();
                        }
                        break;
                    case (int)KeyCode.SignOn:
                        var newpos = KeysToString(keys[0]);
                        if (newpos == "*")
                            ThisPositionIndicator = "NONE";
                        else
                            ThisPositionIndicator = newpos;
                        lock (radar.Aircraft)
                        {
                            radar.Aircraft.Where(x => x.PositionInd != ThisPositionIndicator &&
                            x.PendingHandoff != ThisPositionIndicator).ToList().ForEach(x => x.Owned = false);
                        }
                        Preview.Clear();
                        break;
                    case (int)Key.KeypadMultiply: // splat commands
                        switch ((int)keys[0][1])
                        {
                            case (int)Key.T:
                                if (clickedplane)
                                {
                                    if (tempLine == null)
                                    {
                                        tempLine = new RangeBearingLine() { StartPlane = (Aircraft)clicked, End = LocationFromScreenPoint(MouseLocation) };
                                        lock(rangeBearingLines)
                                            rangeBearingLines.Add(tempLine);
                                    }
                                    if (keys[0].Length > 2)
                                    {
                                        int rblIndex = 0;
                                        var entered = KeysToString(keys[0]).Substring(1);
                                        if (int.TryParse(entered, out rblIndex))
                                        {
                                            if (rblIndex <= rangeBearingLines.Count)
                                            {
                                                lock (rangeBearingLines)
                                                    rangeBearingLines.RemoveAt(rblIndex - 1);
                                                Preview.Clear();
                                            }
                                        }
                                        else
                                        {
                                            var waypoint = Waypoints.Find(x => x.ID == entered);
                                            if (waypoint != null)
                                            {
                                                tempLine.StartGeo = waypoint.Location;
                                                Preview.Clear();
                                            }

                                        }
                                        if (clickedplane)
                                        {
                                            tempLine.EndPlane = (Aircraft)clicked;
                                            tempLine = null;
                                        }
                                    }
                                    Preview.Clear();
                                }
                                else if (enter)
                                {
                                    if (keys[0].Length == 2)
                                    {
                                        lock (rangeBearingLines)
                                            rangeBearingLines.Clear();
                                        Preview.Clear();
                                    }
                                    if (keys[0].Length > 2)
                                    {
                                        int rblIndex = 0;
                                        var entered = KeysToString(keys[0]).Substring(1);
                                        if (int.TryParse(entered, out rblIndex))
                                        {
                                            if (rblIndex <= rangeBearingLines.Count)
                                            {
                                                lock (rangeBearingLines)
                                                    rangeBearingLines.RemoveAt(rblIndex - 1);
                                                Preview.Clear();
                                            }
                                        }
                                        else
                                        {
                                            var waypoint = Waypoints.Find(x => x.ID == entered);
                                            if (waypoint != null)
                                            {
                                                tempLine = new RangeBearingLine() { StartGeo = waypoint.Location, End = LocationFromScreenPoint(MouseLocation) };
                                                lock (rangeBearingLines)
                                                    rangeBearingLines.Add(tempLine);
                                                Preview.Clear();
                                            }
                                            
                                        }
                                    }
                                }
                                else if (tempLine == null)
                                {
                                    tempLine = new RangeBearingLine() { StartGeo = ScreenToGeoPoint((PointF)clicked) };
                                    lock (rangeBearingLines)
                                        rangeBearingLines.Add(tempLine);
                                    Preview.Clear();
                                }

                                break;
                            case (int)Key.J:
                                if (clickedplane)
                                {
                                    if (keys[0].Length > 2)
                                    {
                                        decimal miles = 0;
                                        var entered = KeysToString(keys[0]).Substring(1);
                                        if (decimal.TryParse(entered, out miles))
                                        {
                                            if (miles > 0 && (double)miles < radar.Range)
                                            {
                                                ((Aircraft)clicked).TPA = new TPARing((Aircraft)clicked, miles, ReturnColor, Font);
                                            }
                                            else
                                            {
                                                ((Aircraft)clicked).TPA = null;
                                            }
                                            Preview.Clear();
                                        }
                                    }
                                    else
                                    {
                                        ((Aircraft)clicked).TPA = null;
                                        Preview.Clear();
                                    }
                                }
                                break;
                            case (int)Key.P:
                                if (clickedplane)
                                {
                                    if (keys[0].Length > 2)
                                    {
                                        decimal miles = 0;
                                        var entered = KeysToString(keys[0]).Substring(1);
                                        if (decimal.TryParse(entered, out miles))
                                        {
                                            if (miles > 0 && (double)miles < radar.Range)
                                            {
                                                ((Aircraft)clicked).TPA = new TPACone((Aircraft)clicked, miles, ReturnColor, Font);
                                            }
                                            else
                                            {
                                                ((Aircraft)clicked).TPA = null;
                                            }
                                            Preview.Clear();
                                        }
                                    }
                                    else
                                    {
                                        ((Aircraft)clicked).TPA = null;
                                        Preview.Clear();
                                    }
                                }
                                break;
                            case (int)Key.KeypadMultiply:
                                if (keys[0].Length > 2)
                                {
                                    switch ((int)keys[0][2])
                                    {
                                        case (int)Key.J:
                                            lock (radar.Aircraft)
                                                radar.Aircraft.Where(x => x.TPA != null).Where(x => x.TPA.Type == TPAType.JRing).ToList().ForEach(x => x.TPA = null);
                                            Preview.Clear();
                                            break;
                                        case (int)Key.P:
                                            lock (radar.Aircraft)
                                                radar.Aircraft.Where(x => x.TPA != null).Where(x => x.TPA.Type == TPAType.PCone).ToList().ForEach(x => x.TPA = null);
                                            Preview.Clear();
                                            break;
                                    }
                                }
                                break;
                        }
                        break;

                    case (int)KeyCode.MultiFunc:
                        //MultiFuntion
                        switch ((int)keys[0][1])
                        {
                            case (int)Key.P:
                                if (!clickedplane)
                                {
                                    PreviewArea.LocationF = (PointF)clicked;
                                    Preview.Clear();
                                }
                                break;
                            case (int)Key.Q:
                                QuickLook = !QuickLook;
                                break;
                            case (int)Key.S: 
                                if (!clickedplane)
                                {
                                    StatusArea.LocationF = (PointF)clicked;
                                    Preview.Clear();
                                }
                                break;
                            case (int)Key.O: //Multifunction O: Auto Offset
                                if (keys[0].Length == 3 && enter)
                                {
                                    if ((int)keys[0][2] == (int)Key.I) //Inhibit
                                        AutoOffset = false;
                                    else if ((int)keys[0][2] == (int)Key.E) //Enable
                                        AutoOffset = true;
                                    else
                                        break;
                                    Preview.Clear();
                                }
                                break;
                        }
                        break;
                }
            }
        }

        private string KeysToString (KeyCode[] keys)
        {
            string output = "";
            foreach (var key in keys)
            {
                switch ((int)key)
                {
                    case (int)Key.A:
                        output += "A";
                        break;
                    case (int)Key.B:
                        output += "B";
                        break;
                    case (int)Key.C:
                        output += "C";
                        break;
                    case (int)Key.D:
                        output += "D";
                        break;
                    case (int)Key.E:
                        output += "E";
                        break;
                    case (int)Key.F:
                        output += "F";
                        break;
                    case (int)Key.G:
                        output += "G";
                        break;
                    case (int)Key.H:
                        output += "H";
                        break;
                    case (int)Key.I:
                        output += "I";
                        break;
                    case (int)Key.J:
                        output += "J";
                        break;
                    case (int)Key.K:
                        output += "K";
                        break;
                    case (int)Key.L:
                        output += "L";
                        break;
                    case (int)Key.M:
                        output += "M";
                        break;
                    case (int)Key.N:
                        output += "N";
                        break;
                    case (int)Key.O:
                        output += "O";
                        break;
                    case (int)Key.P:
                        output += "P";
                        break;
                    case (int)Key.Q:
                        output += "Q";
                        break;
                    case (int)Key.R:
                        output += "R";
                        break;
                    case (int)Key.S:
                        output += "S";
                        break;
                    case (int)Key.T:
                        output += "T";
                        break;
                    case (int)Key.U:
                        output += "U";
                        break;
                    case (int)Key.V:
                        output += "V";
                        break;
                    case (int)Key.W:
                        output += "W";
                        break;
                    case (int)Key.X:
                        output += "X";
                        break;
                    case (int)Key.Y:
                        output += "Y";
                        break;
                    case (int)Key.Z:
                        output += "Z";
                        break;
                    case (int)Key.Keypad0:
                    case (int)Key.Number0:
                        output += "0";
                        break;
                    case (int)Key.Keypad1:
                    case (int)Key.Number1:
                        output += "1";
                        break;
                    case (int)Key.Keypad2:
                    case (int)Key.Number2:
                        output += "2";
                        break;
                    case (int)Key.Keypad3:
                    case (int)Key.Number3:
                        output += "3";
                        break;
                    case (int)Key.Keypad4:
                    case (int)Key.Number4:
                        output += "4";
                        break;
                    case (int)Key.Keypad5:
                    case (int)Key.Number5:
                        output += "5";
                        break;
                    case (int)Key.Keypad6:
                    case (int)Key.Number6:
                        output += "6";
                        break;
                    case (int)Key.Keypad7:
                    case (int)Key.Number7:
                        output += "7";
                        break;
                    case (int)Key.Keypad8:
                    case (int)Key.Number8:
                        output += "8";
                        break;
                    case (int)Key.Keypad9:
                    case (int)Key.Number9:
                        output += "9";
                        break;
                    case (int)Key.Period:
                    case (int)Key.KeypadPeriod:
                        output += ".";
                        break;
                }
            }
                return output;
        }

        private void RenderPreview()
        {
            var oldtext = PreviewArea.Text;
            PreviewArea.Text = GeneratePreviewString(Preview);
            PreviewArea.Redraw = oldtext != PreviewArea.Text;
            PreviewArea.ForeColor = DataBlockColor;
            DrawLabel(PreviewArea);
        }
        private void RenderStatus()
        {
            StatusArea.ForeColor = DataBlockColor;
            StatusArea.Font = Font;
            var oldtext = StatusArea.Text;
            StatusArea.Text = "";
            foreach (var metar in radar.Metars.OrderBy(x=> x.Icao))
            {
                if (metar.IsParsed)
                {
                    try
                    {
                        StatusArea.Text += metar.Icao + " " + Converter.Pressure(metar.Pressure, libmetar.Enums.PressureUnit.inHG).Value.ToString("00.00");
                        if (WindInStatusArea)
                            StatusArea.Text += " " + metar.Wind.Raw;
                    }
                    catch
                    {
                        StatusArea.Text += metar.Icao + " METAR ERR";
                    }
                    StatusArea.Text += "\r\n";
                }
            }
            if (FPSInStatusArea)
                StatusArea.Text += $"FPS: {fps} AC: {radar.Aircraft.Count}";
            if (oldtext != StatusArea.Text)
                StatusArea.Redraw = true;
            DrawLabel(StatusArea);
        }
        private string GeneratePreviewString(List<KeyCode> keys)
        {
            string output = "";
            foreach (var key in keys)
            {
                switch ((int)key)
                {
                    case (int)Key.A:
                        output += "A";
                        break;
                    case (int)Key.B:
                        output += "B";
                        break;
                    case (int)Key.C:
                        output += "C";
                        break;
                    case (int)Key.D:
                        output += "D";
                        break;
                    case (int)Key.E:
                        output += "E";
                        break;
                    case (int)Key.F:
                        output += "F";
                        break;
                    case (int)Key.G:
                        output += "G";
                        break;
                    case (int)Key.H:
                        output += "H";
                        break;
                    case (int)Key.I:
                        output += "I";
                        break;
                    case (int)Key.J:
                        output += "J";
                        break;
                    case (int)Key.K:
                        output += "K";
                        break;
                    case (int)Key.L:
                        output += "L";
                        break;
                    case (int)Key.M:
                        output += "M";
                        break;
                    case (int)Key.N:
                        output += "N";
                        break;
                    case (int)Key.O:
                        output += "O";
                        break;
                    case (int)Key.P:
                        output += "P";
                        break;
                    case (int)Key.Q:
                        output += "Q";
                        break;
                    case (int)Key.R:
                        output += "R";
                        break;
                    case (int)Key.S:
                        output += "S";
                        break;
                    case (int)Key.T:
                        output += "T";
                        break;
                    case (int)Key.U:
                        output += "U";
                        break;
                    case (int)Key.V:
                        output += "V";
                        break;
                    case (int)Key.W:
                        output += "W";
                        break;
                    case (int)Key.X:
                        output += "X";
                        break;
                    case (int)Key.Y:
                        output += "Y";
                        break;
                    case (int)Key.Z:
                        output += "Z";
                        break;
                    case (int)Key.Keypad0:
                    case (int)Key.Number0:
                        output += "0";
                        break;
                    case (int)Key.Keypad1:
                    case (int)Key.Number1:
                        output += "1";
                        break;
                    case (int)Key.Keypad2:
                    case (int)Key.Number2:
                        output += "2";
                        break;
                    case (int)Key.Keypad3:
                    case (int)Key.Number3:
                        output += "3";
                        break;
                    case (int)Key.Keypad4:
                    case (int)Key.Number4:
                        output += "4";
                        break;
                    case (int)Key.Keypad5:
                    case (int)Key.Number5:
                        output += "5";
                        break;
                    case (int)Key.Keypad6:
                    case (int)Key.Number6:
                        output += "6";
                        break;
                    case (int)Key.Keypad7:
                    case (int)Key.Number7:
                        output += "7";
                        break;
                    case (int)Key.Keypad8:
                    case (int)Key.Number8:
                        output += "8";
                        break;
                    case (int)Key.Keypad9:
                    case (int)Key.Number9:
                        output += "9";
                        break;
                    case (int)KeyCode.Triangle:
                        output += "▲";
                        break;
                    case (int)KeyCode.FltData:
                        output += "FD\r\n";
                        break;
                    case (int)KeyCode.HndOff:
                        output += "HO\r\n";
                        break;
                    case (int)KeyCode.InitCntl:
                        output += "IC\r\n";
                        break;
                    case (int)KeyCode.Min:
                        output += "MIN\r\n";
                        break;
                    case (int)KeyCode.MultiFunc:
                        output += "F\r\n";
                        break;
                    case (int)KeyCode.TermCntl:
                        output += "TC\r\n";
                        break;
                    case (int)KeyCode.SignOn:
                        output += "SIGN ON\r\n";
                        break;
                    case (int)KeyCode.VP:
                        output += "VP\r\n";
                        break;
                    case (int)Key.Period:
                    case (int)Key.KeypadPeriod:
                        output += ".";
                        break;
                    case (int)Key.Plus:
                    case (int)Key.KeypadPlus:
                        output += "+";
                        break;
                    case (int)Key.KeypadMultiply:
                        output += "*";
                        break;
                    case (int)Key.Slash:
                    case (int)Key.KeypadDivide:
                        output += "/";
                        break;
                    case (int)Key.Space:
                        output += "\r\n";
                        break;
                    default:
                        break;
                }
                
            }
            return output;
        }
        private bool showAllCallsigns = false;
        private void Window_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            var oldscale = scale;
            if (e.Control)
            {
                switch (e.Key)
                {
                    case Key.C:
                        Preview.Clear();
                        radar.Stop();
                        Environment.Exit(0);
                        break;
                    case Key.S:
                        Preview.Clear();
                        SaveSettings(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DGScope.xml"));
                        break;
                    case Key.P:
                        PropertyForm properties = new PropertyForm(this);
                        properties.Show();
                        break;
                    case Key.F1:
                        double bearing = ScreenCenterPoint.BearingTo(radar.Location) - ScreenRotation;
                        double distance = ScreenCenterPoint.DistanceTo(radar.Location);
                        float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                        float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale));
                        ScreenCenterPoint = _homeLocation;
                        MoveTargets(x, -y);
                        break;
                    case Key.F2:
                        VideoMapSelector selector = new VideoMapSelector(VideoMaps);
                        selector.Show();
                        selector.BringToFront();
                        selector.Focus();
                        break;
                    case Key.T:
                        if (!showAllCallsigns)
                        {
                            lock (dataBlocks)
                            {
                                foreach (var block in dataBlocks.Where(x => !x.ParentAircraft.ShowCallsignWithNoSquawk).ToList())
                                {
                                    block.ParentAircraft.ShowCallsignWithNoSquawk = true;
                                    block.Redraw = true;
                                    //block.ParentAircraft.RedrawTarget(block.ParentAircraft.LocationF);
                                    GenerateDataBlock(block.ParentAircraft);
                                    DrawLabel(block);
                                    Console.WriteLine(block.ParentAircraft);
                                }
                            }
                            showAllCallsigns = true;
                        }
                        else
                        {
                            List<Aircraft> planesWithoutShowAll;
                            lock (radar.Aircraft)
                                planesWithoutShowAll = radar.Aircraft.Where(x => !x.ShowCallsignWithNoSquawk).ToList();
                            foreach (var plane in planesWithoutShowAll)
                            {
                                showAllCallsigns = true;
                            }
                        }
                        break;
                }
            }
            else if (e.Alt)
            {
                switch (e.Key)
                {
                    case Key.Enter:
                    case Key.KeypadEnter:
                        if (isScreenSaver)
                        {
                            radar.Stop();
                            Environment.Exit(0);
                        }
                        else
                            window.WindowState = window.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
                        break;

                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        Preview.Clear();
                        PreviewArea.Redraw = true;
                        if (tempLine != null)
                        {
                            lock (rangeBearingLines)
                                rangeBearingLines.Remove(tempLine);
                            tempLine = null;
                        }
                        break;
                    case Key.Enter:
                    case Key.KeypadEnter:
                        ProcessCommand(Preview);
                        break;
                    case Key.BackSpace:
                        if (Preview.Count > 0)
                            Preview.RemoveAt(Preview.Count - 1);
                        break;
                    default:
                        if ((int)e.Key > 9 && (int)e.Key < 22)
                            Preview.Clear();
                        if (((int)e.Key > 9 && (int)e.Key < 26) || (int)e.Key > 66)
                            Preview.Add((KeyCode)e.Key);
                        break;
                }
            }
            
        }
        private void Window_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.T:
                    if (showAllCallsigns)
                    {
                        lock (dataBlocks)
                        {
                            foreach (var block in dataBlocks.Where(x=>x.ParentAircraft.ShowCallsignWithNoSquawk).ToList())
                            {
                                block.ParentAircraft.ShowCallsignWithNoSquawk = false;
                                block.Redraw = true;
                                block.ParentAircraft.RedrawTarget(block.ParentAircraft.LocationF);
                                GenerateDataBlock(block.ParentAircraft);
                                DrawLabel(block);
                                Console.WriteLine(block.ParentAircraft);
                            }
                        }
                        showAllCallsigns = false;
                    }
                    else
                    {
                        List<Aircraft> planesWithShowAll;
                        lock (radar.Aircraft)
                            planesWithShowAll = radar.Aircraft.Where(x => x.ShowCallsignWithNoSquawk).ToList();
                        foreach (var plane in planesWithShowAll)
                        {
                            showAllCallsigns = false;
                        }
                    }
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
                dataBlocks.ForEach(x => x.ParentAircraft.DataBlock2.Redraw = true);
                dataBlocks.ForEach(x => x.ParentAircraft.DataBlock3.Redraw = true);
            }
            lock (posIndicators)
                posIndicators.ForEach(x => x.Redraw = true);
            oldar = aspect_ratio;
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
        }
        private int fps = 0;
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
            DrawVideoMapLines();
            GenerateTargets();
            DrawTargets();
            DrawRBLs();
            DrawStatic();
            GL.Flush();
            window.SwapBuffers();
            fps = (int)(1f / e.Time);
        }
        private PointF GeoToScreenPoint(GeoPoint geoPoint)
        {
            double bearing = radar.Location.BearingTo(geoPoint) - ScreenRotation;
            double distance = radar.Location.DistanceTo(geoPoint);
            float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
            float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale) * aspect_ratio);
            return new PointF(x, y);
        }

        private GeoPoint ScreenToGeoPoint(PointF Point)
        {
            double r = Math.Sqrt(Math.Pow(Point.X, 2) + Math.Pow(Point.Y / aspect_ratio, 2));
            double angle = Math.Atan((Point.Y / aspect_ratio) / Point.X);
            if (Point.X < 0)
                angle += Math.PI;
            double bearing = 90 - (angle * 180 / Math.PI) + ScreenRotation;
            double distance = r * scale;
            return radar.Location.FromPoint(distance, bearing);
        }
        private GeoPoint ScreenToGeoPoint(Point Point)
        {
            return ScreenToGeoPoint(LocationFromScreenPoint(Point));
        }
        private void DrawRBLs()
        {
            List<RangeBearingLine> lines;
            lock (rangeBearingLines)
                lines = rangeBearingLines.ToList();
            foreach (var line in lines)
            {
                var index = rangeBearingLines.IndexOf(line) + 1;
                if (line.End == null|| (line.End.X == 0 && line.End.Y == 0))
                    return;
                if (line.StartPlane != null)
                {
                    line.Start = line.StartPlane.LocationF;
                    line.StartGeo = line.StartPlane.SweptLocation;
                    line.Line.End1 = line.StartPlane.SweptLocation;
                }
                else if (line.StartGeo != null)
                {
                    line.Start = GeoToScreenPoint((GeoPoint)line.StartGeo);
                    line.Line.End1 = line.StartGeo;
                }
                else
                {
                    return;
                }

                if (line.EndPlane != null)
                {
                    line.End = line.EndPlane.LocationF;
                    line.EndGeo = line.EndPlane.SweptLocation;
                    line.Line.End2 = line.EndPlane.SweptLocation;
                }
                else if (line.EndGeo != null)
                {
                    line.End = GeoToScreenPoint((GeoPoint)line.EndGeo);
                    line.Line.End2 = line.EndGeo;
                }
                
                DrawLine(line.Start, line.End, RBLColor);
                line.Label.ForeColor = RBLColor;
                line.Label.LocationF = line.End;
                int bearing = 0;
                double range = 0;
                if (line.EndGeo != null)
                {
                    bearing = (int)(line.StartGeo.BearingTo(line.EndGeo) - ScreenRotation + 720) % 360;
                    if (bearing == 000)
                        bearing = 360;
                    range = line.StartGeo.DistanceTo(line.EndGeo);
                }
                else
                {
                    GeoPoint tempEndGeo = ScreenToGeoPoint(line.End);
                    bearing = (int)(line.StartGeo.BearingTo(tempEndGeo) - ScreenRotation + 720) % 360;
                    if (bearing == 000)
                        bearing = 360;
                    range = line.StartGeo.DistanceTo(tempEndGeo);
                }
                line.Label.Text = string.Format("{0}/{1}-{2}", bearing.ToString("000"), range.ToString("0.00"), index);
                DrawLabel(line.Label);
            }
        }
        private void DrawStatic()
        {
            RenderPreview();
            RenderStatus();
        }
        private void Window_Load(object sender, EventArgs e)
        {
            if (isScreenSaver)
            {
                window.WindowState = WindowState.Fullscreen;
                window.CursorVisible = false;
            }
            radar.Start();
            oldar = aspect_ratio;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            radar.Stop();
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
            var settingsxml = "";
            try
            {
                settingsxml = XmlSerializer<RadarWindow>.Serialize(this);
                if (settingsxml.Length == 0)
                {
                    System.Windows.Forms.MessageBox.Show("There was a problem serializing the settings"
                        , "Error writing settings file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
                using (StreamWriter file = new StreamWriter(path))
                {
                    file.Write(settingsxml);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Error writing settings file", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
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
        private void DrawCircle (GeoPoint Location, float radius, Color color, bool fill = false)
        {
            var LocationF = GeoToScreenPoint(Location);
            var x = LocationF.X;
            var y = LocationF.Y;
            var r = radius / scale;
            DrawCircle(x, y, r, aspect_ratio, 100, color, fill);
        }
        private void DrawTPA(Aircraft plane)
        {
            if (plane.TPA == null)
            {
                return;
            }
            else if (plane.TPA.Type == TPAType.JRing)
            {
                DrawCircle(plane.SweptLocation, (float)plane.TPA.Miles, plane.TPA.Color, false);
            }
            else if (plane.TPA.Type == TPAType.PCone)
            {

            }
            return;
        }
        private void DrawVideoMapLines()
        {
            List<Line> lines = new List<Line>();
            foreach (var map in VideoMaps)
            {
                if (map.Visible)
                    lines.AddRange(map.Lines);
            }
            DrawLines(lines, VideoMapLineColor);
        }

        private void DrawLines (List<Line> lines, Color color)
        {
            foreach (Line line in lines)
            {
                DrawLine(line, color);
            }
        }

        private void DrawLine(PointF Point1, PointF Point2, Color color)
        {
            DrawLine(Point1.X, Point1.Y, Point2.X, Point2.Y, color);
        }
        private void DrawLine(Line line, Color color)
        {
            double scale = radar.Range / Math.Sqrt(2);
            double yscale = (double)window.Width / (double)window.Height;
            double bearing1 = radar.Location.BearingTo(line.End1) - ScreenRotation;
            double distance1 = radar.Location.DistanceTo(line.End1);
            float x1 = (float)(Math.Sin(bearing1 * (Math.PI / 180)) * (distance1 / scale));
            float y1 = (float)(Math.Cos(bearing1 * (Math.PI / 180)) * (distance1 / scale) * yscale);

            double bearing2 = radar.Location.BearingTo(line.End2) - ScreenRotation;
            double distance2 = radar.Location.DistanceTo(line.End2);
            float x2 = (float)(Math.Sin(bearing2 * (Math.PI / 180)) * (distance2 / scale));
            float y2 = (float)(Math.Cos(bearing2 * (Math.PI / 180)) * (distance2 / scale) * yscale);

            DrawLine(x1, y1, x2, y2, color);
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
            x1 = RoundUpToNearest(x1, xPixelScale);
            x2 = RoundUpToNearest(x2, xPixelScale);
            y1 = RoundUpToNearest(y1, yPixelScale);
            y2 = RoundUpToNearest(y2, yPixelScale);
            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(width);
            GL.Color4(color);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);
            GL.End();
        }
        private List<TransparentLabel> dataBlocks = new List<TransparentLabel>();
        private List<TransparentLabel> posIndicators = new List<TransparentLabel>();
        private void GenerateDataBlock(Aircraft aircraft)
        {
            var oldcolor = aircraft.DataBlock.ForeColor;
            if (aircraft.Emergency)
            {
                aircraft.DataBlock.ForeColor = DataBlockEmergencyColor;
                aircraft.DataBlock2.ForeColor = DataBlockEmergencyColor;
                aircraft.DataBlock3.ForeColor = DataBlockEmergencyColor;
            }
            else if (aircraft.Marked)
            {
                aircraft.DataBlock.ForeColor = SelectedColor;
                aircraft.DataBlock2.ForeColor = SelectedColor;
                aircraft.DataBlock3.ForeColor = SelectedColor;
            }
            else if (aircraft.Owned)
            {
                aircraft.DataBlock.ForeColor = OwnedColor;
                aircraft.DataBlock2.ForeColor = OwnedColor;
            }
            else if (aircraft.FDB)
            {
                aircraft.DataBlock.ForeColor = DataBlockColor;
                aircraft.DataBlock2.ForeColor = DataBlockColor;
            }
            else
            {
                aircraft.DataBlock.ForeColor = LDBColor;
                aircraft.DataBlock2.ForeColor = LDBColor;
            }
            aircraft.PositionIndicator.ForeColor = aircraft.DataBlock.ForeColor;
            aircraft.DataBlock3.ForeColor = aircraft.DataBlock.ForeColor;
            if (oldcolor != aircraft.DataBlock.ForeColor)
                aircraft.DataBlock.Redraw = true;
            aircraft.RedrawDataBlock();
            Bitmap text_bmp = aircraft.DataBlock.TextBitmap();
            var realWidth = text_bmp.Width * xPixelScale;
            var realHeight = text_bmp.Height * yPixelScale;
            aircraft.DataBlock.SizeF = new SizeF(realWidth, realHeight);
            aircraft.DataBlock.ParentAircraft = aircraft;
            aircraft.DataBlock2.ParentAircraft = aircraft;
            aircraft.DataBlock3.ParentAircraft = aircraft;
            aircraft.DataBlock.LocationF = OffsetDatablockLocation(aircraft);
            aircraft.DataBlock2.LocationF = aircraft.DataBlock.LocationF;
            aircraft.DataBlock3.LocationF = aircraft.DataBlock.LocationF;
            if (!dataBlocks.Contains(aircraft.DataBlock))
            {
                lock (dataBlocks)
                    dataBlocks.Add(aircraft.DataBlock);
            }

        }
        private void GenerateTarget(Aircraft aircraft)
        {
            
            double bearing = radar.Location.BearingTo(aircraft.Location) - ScreenRotation;
            double distance = radar.Location.DistanceTo(aircraft.Location);
            float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
            float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale) * aspect_ratio);
            var location = new PointF(x, y);
            if (aircraft.LastPositionTime > DateTime.UtcNow.AddSeconds(-LostTargetSeconds))
            {
                if (aircraft.LastHistoryDrawn < DateTime.UtcNow.AddSeconds(-HistoryInterval))
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
                        aircraft.TargetReturn.Angle = (Math.Atan((location.X - aircraft.TargetReturn.LocationF.X) / (location.Y - aircraft.TargetReturn.LocationF.Y)) * (180 / Math.PI));
                    }
                    PrimaryReturn newreturn = new PrimaryReturn();
                    aircraft.TargetReturn = newreturn;
                    newreturn.ParentAircraft = aircraft;
                    newreturn.FadeTime = FadeTime;
                    newreturn.NewLocation = location;
                    newreturn.Intensity = 1;
                    newreturn.ForeColor = ReturnColor;
                    newreturn.ShapeHeight = TargetHeight;
                    newreturn.ShapeWidth = TargetWidth;
                    lock (PrimaryReturns)
                        PrimaryReturns.Add(newreturn);
                    aircraft.LastHistoryDrawn = DateTime.UtcNow;
                }
                aircraft.RedrawTarget(location);
                aircraft.PTL.End1 = aircraft.Location;
                double ptldistance = aircraft.Owned ? (aircraft.GroundSpeed / 60) * PTLlength : (aircraft.GroundSpeed / 60) * PTLlengthAll;
                aircraft.PTL.End2 = aircraft.Location.FromPoint(ptldistance, aircraft.Track);

                if (aircraft.TrueAltitude <= radar.MaxAltitude && aircraft.TrueAltitude >= MinAltitude)
                    GenerateDataBlock(aircraft);
                else if (!aircraft.Owned && !aircraft.FDB)
                    lock (dataBlocks)
                        dataBlocks.Remove(aircraft.DataBlock);


                Bitmap text_bmp = aircraft.PositionIndicator.TextBitmap();
                var realWidth = text_bmp.Width * xPixelScale;
                var realHeight = text_bmp.Height * yPixelScale;
                aircraft.PositionIndicator.SizeF = new SizeF(realWidth, realHeight);
                aircraft.PositionIndicator.CenterOnPoint(location);
                if (aircraft.PositionInd != null)
                    aircraft.PositionIndicator.Text = aircraft.PositionInd.Last().ToString();
                if (aircraft.Marked)
                    aircraft.PositionIndicator.ForeColor = SelectedColor;
                else if (aircraft.Owned)
                    aircraft.PositionIndicator.ForeColor = OwnedColor;
                else
                    aircraft.PositionIndicator.ForeColor = DataBlockColor;

                if (!posIndicators.Contains(aircraft.PositionIndicator))
                {
                    lock (posIndicators)
                        posIndicators.Add(aircraft.PositionIndicator);
                }
                aircraft.Drawn = true;

                

            }
            else
            {
                lock (dataBlocks)
                    dataBlocks.Remove(aircraft.DataBlock);
                lock (posIndicators)
                    posIndicators.Remove(aircraft.PositionIndicator);
            }
        }
        private void GenerateTargets()
        {
            List<Aircraft> targets = radar.Scan();
            foreach (Aircraft aircraft in targets)
            {
                if (QuickLook)
                    aircraft.QuickLook = true;
                GenerateTarget(aircraft);
            }
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                if (target.Intensity < .001)
                {
                    lock(PrimaryReturns)
                        PrimaryReturns.Remove(target);
                    if (target.ParentAircraft.TargetReturn == target)
                    {
                        lock(dataBlocks)
                            dataBlocks.Remove(target.ParentAircraft.DataBlock);
                        lock (posIndicators)
                            posIndicators.Remove(target.ParentAircraft.PositionIndicator);
                    }
                }
            }            
        }
        private bool inRange (Aircraft plane)
        {
            return plane.Location.DistanceTo(radar.Location) <= radar.Range;
        }

        private PointF OffsetDatablockLocation(Aircraft thisAircraft, LeaderDirection direction)
        {
            PointF blockLocation = new PointF();
            float xoffset = 0;
            float yoffset = 0;
            switch (direction)
            {
                case LeaderDirection.N:
                    yoffset += LeaderLength * yPixelScale;
                    break;
                case LeaderDirection.S:
                    yoffset -= LeaderLength * yPixelScale;
                    break;
                case LeaderDirection.E:
                    xoffset += LeaderLength * xPixelScale;
                    break;
                case LeaderDirection.W:
                    xoffset -= LeaderLength * xPixelScale;
                    break;
                case LeaderDirection.NE:
                    yoffset += LeaderLength * yPixelScale * (float)Math.Sqrt(2) / 2;
                    xoffset += LeaderLength * xPixelScale * (float)Math.Sqrt(2) / 2;
                    break;
                case LeaderDirection.SE:
                    xoffset += LeaderLength * xPixelScale * (float)Math.Sqrt(2) / 2;
                    yoffset -= LeaderLength * yPixelScale * (float)Math.Sqrt(2) / 2;
                    break;
                case LeaderDirection.NW:
                    xoffset -= LeaderLength * xPixelScale * (float)Math.Sqrt(2) / 2;
                    yoffset += LeaderLength * yPixelScale * (float)Math.Sqrt(2) / 2;
                    break;
                case LeaderDirection.SW:
                    xoffset -= LeaderLength * xPixelScale * (float)Math.Sqrt(2) / 2;
                    yoffset -= LeaderLength * yPixelScale * (float)Math.Sqrt(2) / 2;
                    break;
            }
            blockLocation.Y = thisAircraft.LocationF.Y + yoffset;
            blockLocation.X = thisAircraft.LocationF.X + xoffset;
            switch (direction)
            {
                case LeaderDirection.NW:
                case LeaderDirection.W:
                case LeaderDirection.SW:
                    if (thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock2.SizeF.Width)
                        blockLocation.X -= thisAircraft.DataBlock.SizeF.Width;
                    else
                        blockLocation.X -= thisAircraft.DataBlock2.SizeF.Width;
                    break;
            }
            /*switch (direction)
            {
                case LeaderDirection.S:
                    blockLocation.Y -= thisAircraft.DataBlock.SizeF.Height;
                    break;
                case LeaderDirection.SW:
                case LeaderDirection.SE:
                case LeaderDirection.E:
                case LeaderDirection.W:
                case LeaderDirection.NE:
                case LeaderDirection.NW:
                    blockLocation.Y -= thisAircraft.DataBlock.SizeF.Height * 0.75f;
                    break;
            }*/
            blockLocation.Y -= thisAircraft.DataBlock.SizeF.Height * 0.75f;
            PointF leaderStart = new PointF(thisAircraft.LocationF.X, thisAircraft.LocationF.Y);
            
            switch (direction)
            {
                case LeaderDirection.NE:
                    leaderStart.Y = thisAircraft.TargetReturn.BoundsF.Bottom;
                    leaderStart.X = thisAircraft.TargetReturn.BoundsF.Right;
                    break;
                case LeaderDirection.N:
                    leaderStart.Y = thisAircraft.TargetReturn.BoundsF.Bottom;
                    break;
                case LeaderDirection.NW:
                    leaderStart.Y = thisAircraft.TargetReturn.BoundsF.Bottom;
                    leaderStart.X = thisAircraft.TargetReturn.BoundsF.Left;
                    break;
                case LeaderDirection.SE:
                    leaderStart.Y = thisAircraft.TargetReturn.BoundsF.Top;
                    leaderStart.X = thisAircraft.TargetReturn.BoundsF.Right;
                    break;
                case LeaderDirection.S:
                    leaderStart.Y = thisAircraft.TargetReturn.BoundsF.Top;
                    break;
                case LeaderDirection.SW:
                    leaderStart.Y = thisAircraft.TargetReturn.BoundsF.Top;
                    leaderStart.X = thisAircraft.TargetReturn.BoundsF.Left;
                    break;
                case LeaderDirection.E:
                    leaderStart.X = thisAircraft.TargetReturn.BoundsF.Right;
                    break;
                case LeaderDirection.W:
                    leaderStart.X = thisAircraft.TargetReturn.BoundsF.Left;
                    break;
                default:
                    Console.Write("wat");
                    break;

            }
            if (blockLocation.X < thisAircraft.LocationF.X)
            {
                if (thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock2.SizeF.Width)
                    thisAircraft.ConnectingLine.End = new PointF(blockLocation.X + thisAircraft.DataBlock.SizeF.Width,
                        blockLocation.Y + (thisAircraft.DataBlock.SizeF.Height * 0.75f));
                else
                    thisAircraft.ConnectingLine.End = new PointF(blockLocation.X + thisAircraft.DataBlock2.SizeF.Width,
                        blockLocation.Y + (thisAircraft.DataBlock.SizeF.Height * 0.75f));
            }
            else
            {
                thisAircraft.ConnectingLine.End = new PointF(blockLocation.X, blockLocation.Y + (thisAircraft.DataBlock.SizeF.Height * 0.75f));
            }
            thisAircraft.ConnectingLine.Start = leaderStart;
            if ((direction != thisAircraft.LDRDirection && thisAircraft.LDRDirection != null) ||
                (direction != LDRDirection && thisAircraft.LDRDirection == null))
                thisAircraft.RedrawDataBlock(false, direction);

            return blockLocation;
        }
        private PointF OffsetDatablockLocation(Aircraft thisAircraft)
        {
            LeaderDirection newDirection = LDRDirection;
            LeaderDirection oldDirection = LDRDirection;
            if (thisAircraft.LDRDirection != null)
            {
                oldDirection = (LeaderDirection)thisAircraft.LDRDirection;
                newDirection = (LeaderDirection)thisAircraft.LDRDirection;
            }

            if (newDirection != oldDirection)
            {
                thisAircraft.RedrawDataBlock(false);
            }

            PointF blockLocation = OffsetDatablockLocation(thisAircraft, newDirection);
            
            
            if (AutoOffset && inRange(thisAircraft) && thisAircraft.FDB)
            {
                
                RectangleF bounds = new RectangleF(blockLocation, thisAircraft.DataBlock.SizeF);
                int minconflicts = int.MaxValue;
                LeaderDirection bestDirection = newDirection;
                for (int i = 0; i < 8; i++)
                {
                    int conflictcount = 0;
                    List<TransparentLabel> otherDataBlocks = new List<TransparentLabel>();
                    lock (dataBlocks)
                        otherDataBlocks.AddRange(dataBlocks);
                    if (thisAircraft.LDRDirection != null)
                        newDirection = (LeaderDirection)(((int)thisAircraft.LDRDirection + (i * 45)) % 360);
                    else
                        newDirection = (LeaderDirection)(((int)LDRDirection + (i * 45)) % 360);
                    blockLocation = OffsetDatablockLocation(thisAircraft, newDirection);
                    
                    bounds.Location = blockLocation;

                    foreach (var otherDataBlock in otherDataBlocks)
                    {
                        var otherPlane = otherDataBlock.ParentAircraft;
                        if (thisAircraft.ModeSCode != otherPlane.ModeSCode)
                        {
                            RectangleF otherBounds = new RectangleF(otherPlane.DataBlock.LocationF, otherPlane.DataBlock.SizeF);
                            
                            if (bounds.IntersectsWith(otherBounds))
                            {
                                conflictcount+=2;
                            }
                            if (bounds.IntersectsWith(otherPlane.TargetReturn.BoundsF))
                            {
                                conflictcount++;
                            }
                            if (thisAircraft.ConnectingLine.IntersectsWith(otherPlane.ConnectingLine) ||
                                thisAircraft.ConnectingLine.IntersectsWith(otherPlane.TargetReturn.BoundsF) ||
                                thisAircraft.ConnectingLine.IntersectsWith(otherBounds))
                            {
                                conflictcount+=2;
                            }
                        }
                    }
                    if (conflictcount < minconflicts)
                    {
                        minconflicts = conflictcount;
                        bestDirection = newDirection;
                    }
                    if (conflictcount == 0)
                    {
                        break;
                    }
                    else
                    {
                        
                    }
                }
                if (minconflicts > 0)
                {
                    newDirection = bestDirection;
                }
                blockLocation = OffsetDatablockLocation(thisAircraft, newDirection);

            }
            
            
            return blockLocation;
        }

        Timer aircraftGCTimer;

        private void RescaleTargets(float scalechange, float ar_change)
        {
            
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                PointF newLocation = new PointF(target.LocationF.X * scalechange, (target.LocationF.Y * scalechange) / ar_change);
                target.LocationF = newLocation;
            }
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    PointF newLocation = new PointF(plane.LocationF.X * scalechange, (plane.LocationF.Y * scalechange) / ar_change);
                    plane.LocationF = newLocation;
                    plane.DataBlock.LocationF = OffsetDatablockLocation(plane);
                    plane.DataBlock2.LocationF = plane.DataBlock.LocationF;
                    plane.DataBlock3.LocationF = plane.DataBlock.LocationF;
                    plane.PositionIndicator.CenterOnPoint(newLocation);
                    //plane.RedrawDataBlock();
                }
            }
        }

        private void MoveTargets(float xChange, float yChange)
        {
            yChange = yChange * aspect_ratio;
            foreach (PrimaryReturn target in PrimaryReturns.ToList())
            {
                target.LocationF = new PointF(target.LocationF.X + xChange, target.LocationF.Y - yChange);
            }
            foreach (TransparentLabel block in dataBlocks.ToList())
            {
                block.LocationF = new PointF(block.LocationF.X + xChange, block.LocationF.Y - yChange);
                block.ParentAircraft.ConnectingLine.Start = new PointF(block.ParentAircraft.ConnectingLine.Start.X + xChange, block.ParentAircraft.ConnectingLine.Start.Y - yChange);
                block.ParentAircraft.ConnectingLine.End = new PointF(block.ParentAircraft.ConnectingLine.End.X + xChange, block.ParentAircraft.ConnectingLine.End.Y - yChange);
                block.NewLocation = block.LocationF;
                block.ParentAircraft.DataBlock2.LocationF = block.LocationF;
                block.ParentAircraft.DataBlock3.LocationF = block.LocationF;
            }
            lock (posIndicators)
                posIndicators.ForEach(x => x.LocationF = new PointF(x.LocationF.X + xChange, x.LocationF.Y - yChange));
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    plane.LocationF = new PointF(plane.LocationF.X + xChange, plane.LocationF.Y - yChange);
                }
            }
        }

        private void DrawTarget(PrimaryReturn target)
        {
            if (target != target.ParentAircraft.TargetReturn) // history
            {
                if ((target.ParentAircraft.TrueAltitude > MaxAltitude || target.ParentAircraft.TrueAltitude < MinAltitude) && !target.ParentAircraft.FDB)
                    return;
            }
            switch (TargetShape)
            {
                case TargetShape.Rectangle:
                    float targetHeight = target.ShapeHeight * xPixelScale;// (window.ClientRectangle.Height/2);
                    float targetWidth = target.ShapeWidth * yPixelScale;// (window.ClientRectangle.Width/2);
                    float atan = (float)Math.Atan(targetHeight / targetWidth);
                    float targetHypotenuse = (float)(Math.Sqrt((targetHeight * targetHeight) + (targetWidth * targetWidth)) / 2);
                    float x1 = (float)(Math.Sin(atan) * targetHypotenuse);
                    float y1 = (float)(Math.Cos(atan) * targetHypotenuse);
                    float circleradius = 4f * xPixelScale;

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
                    GL.Translate(-target.LocationF.X, -target.LocationF.Y, 0.0f);


                    GL.PopMatrix();
                    break;
                case TargetShape.Circle:
                    target.SizeF = new SizeF(target.ShapeWidth * 2 * xPixelScale, target.ShapeWidth * 2 * yPixelScale);
                    DrawCircle(target.LocationF.X, target.LocationF.Y, target.ShapeWidth * xPixelScale, aspect_ratio, 30, target.ForeColor, true);
                    break;
            }
            
            
            
        }

        private void DrawTargets()
        {
            lock (radar.Aircraft)
            {
                radar.Aircraft.Where(x => x.PositionInd == ThisPositionIndicator).ToList().ForEach(x => x.Owned = true);
                radar.Aircraft.Where(x => x.TPA != null).ToList().ForEach(x => DrawTPA(x));
                foreach (var handoffPlane in radar.Aircraft.Where(x => x.PendingHandoff == ThisPositionIndicator))
                {
                    if (handoffPlane.Owned && handoffPlane.DataBlock.Flashing)
                        continue;
                    handoffPlane.Owned = true;
                    handoffPlane.DataBlock.Flashing = true;
                    handoffPlane.DataBlock2.Flashing = true;
                    if (handoffPlane.LastPositionTime > DateTime.Now.AddSeconds(-LostTargetSeconds))
                        GenerateDataBlock(handoffPlane);
                }
                foreach (var handedoffPlane in radar.Aircraft.Where(x => x.PositionInd == x.PendingHandoff))
                {
                    if (handedoffPlane.PendingHandoff != null)
                        handedoffPlane.PendingHandoff = null;
                    if (handedoffPlane.DataBlock.Flashing)
                    {
                        handedoffPlane.DataBlock.Flashing = false;
                        handedoffPlane.DataBlock2.Flashing = false;
                        handedoffPlane.DataBlock3.Flashing = false;
                        if (handedoffPlane.LastPositionTime > DateTime.Now.AddSeconds(-LostTargetSeconds))
                            GenerateDataBlock(handedoffPlane);
                    }
                }
                foreach (var flashingPlane in radar.Aircraft.Where(x => x.DataBlock.Flashing))
                {
                    if (flashingPlane.PendingHandoff != ThisPositionIndicator)
                    {
                        flashingPlane.DataBlock.Flashing = false;
                        flashingPlane.DataBlock2.Flashing = false;
                        flashingPlane.DataBlock3.Flashing = false;
                        if (flashingPlane.LastPositionTime > DateTime.Now.AddSeconds(-LostTargetSeconds))
                            GenerateDataBlock(flashingPlane);
                    }
                }
            }
            foreach (var target in PrimaryReturns.OrderBy(x => x.Intensity).ToList())
            {
                DrawTarget(target);
            }
            foreach (var block in dataBlocks.ToList().OrderBy(x=>x.ParentAircraft.ModeSCode))
            {
                if (!HideDataTags)
                {
                    if (PTLlength > 0 && block.ParentAircraft.Owned)
                    {
                        DrawLine(block.ParentAircraft.PTL, Color.White);
                    }
                    else if (PTLlengthAll > 0 && !block.ParentAircraft.Owned)
                    {
                        DrawLine(block.ParentAircraft.PTL, Color.White);
                    }
                    if (timeshare % 2 == 0)
                        DrawLabel(block);
                    else if (timeshare % 4 == 1)
                        DrawLabel(block.ParentAircraft.DataBlock2);
                    else
                        DrawLabel(block.ParentAircraft.DataBlock3);
                }
            }
            posIndicators.ForEach(x => DrawLabel(x));
            
        }

        private void DrawLabel(TransparentLabel Label)
        {
            /*if (!radar.Aircraft.Contains(Label.ParentAircraft))
                return;*/
            GL.Enable(EnableCap.Texture2D);
            if (Label.TextureID == 0)
                Label.TextureID = GL.GenTexture();
            var text_texture = Label.TextureID;
            if (Label.Redraw)
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
                if (Label.ParentAircraft != null)
                {
                    if (Label == Label.ParentAircraft.DataBlock)
                    {
                        Label.LocationF = OffsetDatablockLocation(Label.ParentAircraft);
                        Label.ParentAircraft.DataBlock2.LocationF = Label.LocationF;
                        Label.ParentAircraft.DataBlock3.LocationF = Label.LocationF;
                    }
                    else if (Label == Label.ParentAircraft.DataBlock2)
                    {
                        Label.ParentAircraft.DataBlock.LocationF = OffsetDatablockLocation(Label.ParentAircraft);
                        Label.LocationF = Label.ParentAircraft.DataBlock.LocationF;
                        Label.ParentAircraft.DataBlock3.LocationF = Label.LocationF;
                    }
                    else if (Label == Label.ParentAircraft.DataBlock3)
                    {
                        Label.ParentAircraft.DataBlock.LocationF = OffsetDatablockLocation(Label.ParentAircraft);
                        Label.LocationF = Label.ParentAircraft.DataBlock.LocationF;
                        Label.ParentAircraft.DataBlock2.LocationF = Label.LocationF;
                    }
                }
                    

                //text_bmp.Save($"{text_texture}.bmp");
            }
            
            
            GL.BindTexture(TextureTarget.Texture2D, text_texture);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(Label.DrawColor);
            
            var Location = Label.LocationF;
            var x = RoundUpToNearest(Location.X, xPixelScale);
            var y = RoundUpToNearest(Location.Y, yPixelScale);
            var width = Label.SizeF.Width;
            var height = Label.SizeF.Height;
            GL.TexCoord2(0,0);
            GL.Vertex2(x, height + y);
            GL.TexCoord2(1, 0); 
            GL.Vertex2(width + x, height + y);
            GL.TexCoord2(1, 1); 
            GL.Vertex2(width + x, y);
            GL.TexCoord2(0, 1); 
            GL.Vertex2(x, y);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);

            
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Label.ForeColor);
            if (Label.ParentAircraft != null)
            {
                if (Label == Label.ParentAircraft.DataBlock || Label == Label.ParentAircraft.DataBlock2 || Label == Label.ParentAircraft.DataBlock3)
                {
                    ConnectingLineF line = new ConnectingLineF();
                    line = Label.ParentAircraft.ConnectingLine;
                    GL.Vertex2(line.Start.X, line.Start.Y);
                    GL.Vertex2(line.End.X, line.End.Y);
                }
            }
            GL.End();


        }

        private void DrawAllScreenObjectBounds()
        {
            List<IScreenObject> screenObjects = new List<IScreenObject>();
            lock (PrimaryReturns)
                screenObjects.AddRange(PrimaryReturns);
            
            foreach (var item in screenObjects)
            {
                var bounds = new RectangleF(item.LocationF, item.SizeF);
                DrawRectangle(bounds, Color.Gray);
            }
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

        
        


        public static float RoundUpToNearest(float passednumber, float roundto)
        {
            // 105.5 up to nearest 1 = 106
            // 105.5 up to nearest 10 = 110
            // 105.5 up to nearest 7 = 112
            // 105.5 up to nearest 100 = 200
            // 105.5 up to nearest 0.2 = 105.6
            // 105.5 up to nearest 0.3 = 105.6

            //if no rounto then just pass original number back
            //return passednumber;
            if (roundto == 0)
            {
                return passednumber;
            }
            else
            {
                return (float)Math.Ceiling(passednumber / roundto) * roundto;
            }
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
