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

        [Browsable(false)]
        public PointF PreviewLocation { get; set; } = new PointF();

        [DisplayName("Fade Time"), Description("The number of seconds the target is faded out over.  A higher number is a slower fade."), Category("Display Properties")]
        public double FadeTime { get; set; } = 30;
        //public int FadeReps { get; set; } = 6;
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
        List<PrimaryReturn> PrimaryReturns = new List<PrimaryReturn>();

        private GameWindow window;
        private bool isScreenSaver = false;

        private TransparentLabel PreviewArea = new TransparentLabel()
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
        //Thread deconflictThread;
        Timer deconflictTimer;
        List<RangeBearingLine> rangeBearingLines = new List<RangeBearingLine>();
        
        private void Initialize()
        {
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
            //deconflictTimer = new Timer(new TimerCallback(Deconflict), null, 100, 100);
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
            PreviewArea.Font = Font;

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
                Debug.WriteLine("Deleted airplane " + plane.ModeSCode.ToString("X"));
            }
        }
        private void Window_WindowStateChanged(object sender, EventArgs e)
        {
            //window.CursorVisible = window.WindowState != WindowState.Fullscreen;
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
                && x.LastPositionTime > DateTime.Now.AddSeconds(-LostTargetSeconds)).FirstOrDefault();
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
                 ProcessCommand(Preview, clicked);
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
            CA = 20
            
        }

        public List<KeyCode> Preview = new List<KeyCode>();


        bool waitingfortarget = false;
        private void ProcessCommand(List<KeyCode> KeyList, object clicked = null)
        {
            bool clickedplane = false;
            bool enter = false;
            if (clicked != null)
                clickedplane = clicked.GetType() == typeof(Aircraft);
            else
                enter = true;
            
            
            if (KeyList.Count < 1 && clicked.GetType() == typeof(Aircraft))
            {
                Aircraft plane = (Aircraft)clicked;
                if (!plane.Owned)
                    plane.FDB = plane.FDB ? false : true;
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
                            GenerateDataBlock((Aircraft)clicked);
                            Preview.Clear();
                        }
                        break;
                    case (int)KeyCode.TermCntl:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).Owned = false;
                            GenerateDataBlock((Aircraft)clicked);
                            Preview.Clear();
                        }
                        break;
                    case (int)Key.KeypadMultiply:
                        switch ((int)keys[0][1])
                        {
                            case (int)Key.T:
                                if (enter)
                                {
                                    if (keys[0].Length == 2)
                                    {
                                        rangeBearingLines.Clear();
                                        Preview.Clear();
                                    }
                                    else if (keys[0].Length > 2)
                                    {

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

                        }
                        break;
                }
            }
        }

        private void RenderPreview()
        {
            var oldtext = PreviewArea.Text;
            PreviewArea.Text = GeneratePreviewString(Preview);
            PreviewArea.Redraw = oldtext != PreviewArea.Text;
            PreviewArea.ForeColor = DataBlockColor;
            DrawLabel(PreviewArea);

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
                    case (int)KeyCode.VP:
                        output += "VP\r\n";
                        break;
                    case (int)Key.Period:
                    case (int)Key.KeypadPeriod:
                        output += ".\r\n";
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
                        Environment.Exit(0);
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
                    case Key.F11:
                        if (isScreenSaver)
                            Environment.Exit(0);
                        else
                            window.WindowState = window.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
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
            else
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        Preview.Clear();
                        PreviewArea.Redraw = true;
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
            DrawVideoMapLines();
            GenerateTargets();
            DrawTargets();
            DrawStatic();
            GL.Flush();
            window.SwapBuffers();
            window.Title = $"(Vsync: {window.VSync}) FPS: {1f / e.Time:0} Aircraft: {radar.Aircraft.Count}";
        }
        private void DrawStatic()
        {
            RenderPreview();
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
        private void GenerateDataBlock(Aircraft aircraft)
        {
            var oldcolor = aircraft.DataBlock.ForeColor;
            if (aircraft.Emergency)
            {
                aircraft.DataBlock.ForeColor = DataBlockEmergencyColor;
            }
            else if (aircraft.Marked)
            {
                aircraft.DataBlock.ForeColor = SelectedColor;
            }
            else if (aircraft.Owned)
            {
                aircraft.DataBlock.ForeColor = OwnedColor;
            }
            else if (aircraft.FDB)
            {
                aircraft.DataBlock.ForeColor = DataBlockColor;
            }
            else
            {
                aircraft.DataBlock.ForeColor = LDBColor;
            }
            if (oldcolor != aircraft.DataBlock.ForeColor)
                aircraft.DataBlock.Redraw = true;
            aircraft.RedrawDataBlock();
            Bitmap text_bmp = aircraft.DataBlock.TextBitmap();
            var realWidth = text_bmp.Width * xPixelScale;
            var realHeight = text_bmp.Height * yPixelScale;
            aircraft.DataBlock.SizeF = new SizeF(realWidth, realHeight);
            aircraft.DataBlock.ParentAircraft = aircraft;
            aircraft.DataBlock.LocationF = OffsetDatablockLocation(aircraft);
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
                aircraft.RedrawTarget(location);
                aircraft.PTL.End1 = aircraft.Location;
                double ptldistance = aircraft.Owned ? (aircraft.GroundSpeed / 60) * PTLlength : (aircraft.GroundSpeed / 60) * PTLlengthAll;
                aircraft.PTL.End2 = aircraft.Location.FromPoint(ptldistance, aircraft.Track);

                newreturn.NewLocation = location;
                newreturn.Intensity = 1;
                newreturn.ForeColor = ReturnColor;
                newreturn.ShapeHeight = TargetHeight;
                newreturn.ShapeWidth = TargetWidth;
                if (aircraft.Altitude <= radar.MaxAltitude && aircraft.Altitude >= MinAltitude)
                    GenerateDataBlock(aircraft);
                else if (!aircraft.Owned && !aircraft.FDB)
                    lock (dataBlocks)
                        dataBlocks.Remove(aircraft.DataBlock);
                aircraft.Drawn = true;

                lock (PrimaryReturns)
                    PrimaryReturns.Add(newreturn);
                
            }
            else
            {
                lock (dataBlocks)
                    dataBlocks.Remove(aircraft.DataBlock);
            }
        }
        private void GenerateTargets()
        {
            List<Aircraft> targets = radar.Scan();
            foreach (Aircraft aircraft in targets)
            {
                if (QuickLook)
                    aircraft.FDB = true;
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
                        dataBlocks.Remove(target.ParentAircraft.DataBlock);
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
            float xoffset = (float)(Math.Cos((Math.PI / 180) * (double)direction) * LeaderLength * xPixelScale);
            float yoffset = (float)(Math.Sin((Math.PI / 180) * (double)direction) * LeaderLength * yPixelScale);
            blockLocation.Y = thisAircraft.LocationF.Y + yoffset;
            blockLocation.X = thisAircraft.LocationF.X + xoffset;
            switch (direction)
            {
                case LeaderDirection.NW:
                case LeaderDirection.W:
                case LeaderDirection.SW:
                    blockLocation.X -= thisAircraft.DataBlock.SizeF.Width;
                    break;
            }
            switch (direction)
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
            }
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
                thisAircraft.ConnectingLine.End = new PointF(blockLocation.X + thisAircraft.DataBlock.SizeF.Width,
                        blockLocation.Y + (thisAircraft.DataBlock.SizeF.Height * 0.75f));
            }
            else
            {
                thisAircraft.ConnectingLine.End = new PointF(blockLocation.X, blockLocation.Y + (thisAircraft.DataBlock.SizeF.Height * 0.75f));
            }
            thisAircraft.ConnectingLine.Start = leaderStart;

            return blockLocation;
        }
        private PointF OffsetDatablockLocation(Aircraft thisAircraft)
        {
            LeaderDirection newDirection = LDRDirection;
            if (thisAircraft.LDRDirection != null)
                newDirection = (LeaderDirection)thisAircraft.LDRDirection;
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
            foreach (TransparentLabel block in dataBlocks.ToList())
            {
                /*PointF newLocation = new PointF(block.LocationF.X * scalechange, (block.LocationF.Y * scalechange) / ar_change);
                block.LocationF = newLocation;
                block.NewLocation = newLocation;
                block.ParentAircraft.ConnectingLine.Start = new PointF(block.ParentAircraft.ConnectingLine.Start.X * scalechange, (block.ParentAircraft.ConnectingLine.Start.Y * scalechange) / ar_change);
                //block.ParentAircraft.ConnectingLine.End = block.LocationF;*/
            }
            lock (radar.Aircraft)
            {
                foreach (Aircraft plane in radar.Aircraft)
                {
                    PointF newLocation = new PointF(plane.LocationF.X * scalechange, (plane.LocationF.Y * scalechange) / ar_change);
                    //plane.ConnectingLine.End = newLocation;
                    plane.LocationF = newLocation;
                    plane.DataBlock.LocationF = OffsetDatablockLocation(plane);
                    //plane.DataBlock.Redraw = true;
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
                
            }
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
                    if (PTLlength > 0 && block.ParentAircraft.Owned)
                    {
                        DrawLine(block.ParentAircraft.PTL, Color.White);
                    }
                    else if (PTLlengthAll > 0 && !block.ParentAircraft.Owned)
                    {
                        DrawLine(block.ParentAircraft.PTL, Color.White);
                    }
                    DrawLabel(block);
                }
            }
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
                    Label.LocationF = OffsetDatablockLocation(Label.ParentAircraft);
                //text_bmp.Save($"{text_texture}.bmp");
            }
            
            
            GL.BindTexture(TextureTarget.Texture2D, text_texture);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(Label.ForeColor);
            
            //Deconflict(Label);
            var Location = Label.LocationF;
            var x = RoundUpToNearest(Location.X, xPixelScale);
            var y = RoundUpToNearest(Location.Y, yPixelScale);
            var width = RoundUpToNearest(Label.SizeF.Width, xPixelScale);
            var height = RoundUpToNearest(Label.SizeF.Height, yPixelScale);
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
                ConnectingLineF line = new ConnectingLineF();
                line = Label.ParentAircraft.ConnectingLine;
                //line.End = ConnectingLinePoint(Label.ParentAircraft.TargetReturn.BoundsF, Label.BoundsF);
                //line.Start = ConnectingLinePoint(Label.BoundsF, Label.ParentAircraft.TargetReturn.BoundsF);
                GL.Vertex2(line.Start.X, line.Start.Y);
                GL.Vertex2(line.End.X, line.End.Y);
                
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
            bool xmid = true;
            bool ymid = false;
            if (degrees != 90 || degrees != 270)
            {
                if (Math.Abs(Math.Tan(angle - (Math.PI / 2.0f)) * radius ) > (Label.Width / 4))
                    xmid = false;
                if (Math.Abs(Math.Tan(angle) * (radius * aspect_ratio)) < (Label.Height / 4))
                    ymid = true;
            }

            if (xmid)
            {
                EndPoint.X = StartPoint.X + ((float)Math.Cos(angle) * radius) - (Label.Width / 2);//X bound to mid
            }
            else if (degrees > 270 || degrees < 90)
            {
                EndPoint.X = StartPoint.X + (float)Math.Cos(angle) * radius; //X bound to left
            }
            else 
            {
                EndPoint.X = StartPoint.X + ((float)Math.Cos(angle) * radius) - Label.Width; //X bound to right
            }
            
            if (ymid)
            {
                EndPoint.Y = StartPoint.Y + ((float)Math.Sin(angle) * (radius * aspect_ratio)) - (Label.Height / 2); //Y bound to mid
            }
            else if (degrees >=0 && degrees <= 180)
            {
                EndPoint.Y = StartPoint.Y + (float)Math.Sin(angle) * (radius * aspect_ratio); //Y bound to bottom
            }
            else 
            {
                EndPoint.Y = StartPoint.Y + ((float)Math.Sin(angle) * (radius * aspect_ratio)) - Label.Height; //Y bound to top
            }
            return EndPoint;
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
