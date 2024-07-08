﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Input;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Drawing.Design;
using DGScope.Receivers;
using System.Threading;
using System.Windows.Forms.Design;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DGScope.STARS;
using Vector4 = OpenTK.Vector4;
using System.Xml;
using System.Diagnostics;

namespace DGScope
{
    public class RadarWindow
    {
        public static LeaderDirection ParseLDR(string direction)
        {
            if (direction == null)
                return LeaderDirection.N;
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
        private static TimeSync timesync = new TimeSync();
        [XmlIgnore]
        [DisplayName("Background Color"), Description("Radar Background Color"), Category("Colors")]
        public Color BackColor { get; set; } = Color.Black;
        [XmlIgnore]
        [DisplayName("Range Ring Color"), Category("Colors")]
        public Color RangeRingColor { get; set; } = Color.FromArgb(140, 140, 140);
        [XmlIgnore]
        [DisplayName("Video Map Category A Color"), Category("Colors")]
        public Color VideoMapLineColor { get; set; } = Color.FromArgb(140, 140, 140);
        [XmlIgnore]
        [DisplayName("Video Map Category B Color"), Category("Colors")]
        public Color VideoMapBLineColor { get; set; } = Color.FromArgb(140, 140, 140);
        [XmlIgnore]
        [DisplayName("Primary Target Color"), Description("Primary Radar Target color"), Category("Colors")]
        public Color ReturnColor { get; set; } = Color.FromArgb(30, 120, 255);
        [DisplayName("Beacon Target Color"), Description("Primary Radar Target color"), Category("Colors")]
        public Color BeaconTargetColor { get; set; } = Color.FromArgb(0, 255, 0);
        [XmlIgnore]
        [DisplayName("FDB Color"), Description("Color of aircraft full data blocks"), Category("Colors")]
        public Color DataBlockColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Pointout Color"), Description("Color of aircraft data blocks in pointout status"), Category("Colors")]
        public Color PointoutColor { get; set; } = Color.Yellow;
        [XmlIgnore]
        [DisplayName("Owned FDB Color"), Description("Color of aircraft owned full data blocks"), Category("Colors")]
        public Color OwnedColor { get; set; } = Color.White;
        [XmlIgnore]
        [DisplayName("LDB Color"), Description("Color of aircraft limited data blocks"), Category("Colors")]
        public Color LDBColor { get; set; } = Color.Lime;
        [XmlIgnore]
        [DisplayName("Selected Data Block Color"), Description("Color of aircraft limited data blocks"), Category("Colors")]
        public Color SelectedColor { get; set; } = Color.FromArgb(0, 255, 255);
        [XmlIgnore]
        [DisplayName("Data Block Emergency Color"), Description("Color of emergency aircraft data blocks"), Category("Colors")]
        public Color DataBlockEmergencyColor { get; set; } = Color.Red;
        [XmlIgnore]
        [DisplayName("RBL Color"), Description("Color of range bearing lines"), Category("Colors")]
        public Color RBLColor { get; set; } = Color.White;
        [XmlIgnore]
        [DisplayName("TPA Color"), Description("Color of Terminal Proximity Alert Cones/Rings"), Category("Colors")]
        public Color TPAColor { get; set; } = Color.FromArgb(90, 180, 255);
        [XmlIgnore]
        [DisplayName("ATPA Caution Color"), Description("Color of Automated Terminal Proximity Caution"), Category("Colors")]
        public Color ATPACautionColor { get; set; } = Color.FromArgb(255, 255, 0);
        [XmlIgnore]
        [DisplayName("ATPA Alert Color"), Description("Color of Automated Terminal Proximity Alert"), Category("Colors")]
        public Color ATPAAlertColor { get; set; } = Color.FromArgb(255, 55, 0);

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
        [XmlElement("VideoMapLineColorB")]
        [Browsable(false)]
        public int VideoMapBLineColorAsArgb
        {
            get { return VideoMapBLineColor.ToArgb(); }
            set { VideoMapBLineColor = Color.FromArgb(value); }
        }
        [XmlElement("ReturnColor")]
        [Browsable(false)]
        public int ReturnColorAsArgb
        {
            get { return ReturnColor.ToArgb(); }
            set { ReturnColor = Color.FromArgb(value); }
        }
        [XmlElement("BeaconColor")]
        [Browsable(false)]
        public int BeaconColorAsArgb
        {
            get { return BeaconTargetColor.ToArgb(); }
            set { BeaconTargetColor = Color.FromArgb(value); }
        }
        [XmlElement("DataBlockColor")]
        [Browsable(false)]
        public int DataBlockColorAsArgb
        {
            get { return DataBlockColor.ToArgb(); }
            set { DataBlockColor = Color.FromArgb(value); }
        }
        [XmlElement("PointoutColor")]
        [Browsable(false)]
        public int PointoutColorAsArgb
        {
            get { return PointoutColor.ToArgb(); }
            set { PointoutColor = Color.FromArgb(value); }
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
        [XmlElement("RBLColor")]
        [Browsable(false)]
        public int RBLColorAsArgb
        {
            get { return RBLColor.ToArgb(); }
            set { RBLColor = Color.FromArgb(value); }
        }
        [XmlElement("TPAColor")]
        [Browsable(false)]
        public int TPAColorAsArgb
        {
            get { return TPAColor.ToArgb(); }
            set { TPAColor = Color.FromArgb(value); }
        }

        [XmlElement("HistoryColors")]
        [Browsable(false)]
        public int[] HistoryColorsAsArgb
        {
            get
            {
                var array = new int[HistoryColors.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = HistoryColors[i].ToArgb();
                }
                return array;
            }
            set
            {
                if (value == null)
                    return;
                HistoryColors = new Color[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    HistoryColors[i] = Color.FromArgb(value[i]);
                }
            }
        }

        [XmlIgnore]
        [DisplayName("History Colors"), Description("Color of aircraft history"), Category("Colors")]
        public Color[] HistoryColors
        {
            get; set;
        } = new Color[] { Color.FromArgb(30, 80, 200), Color.FromArgb(70, 70, 170), Color.FromArgb(50, 50, 130), Color.FromArgb(40, 40, 110), Color.FromArgb(30, 30, 90) };


        [DisplayName("Fade Time"), Description("The number of seconds the target is faded out over.  A higher number is a slower fade."), Category("Display Properties")]
        public double FadeTime { get; set; } = 30;
        [DisplayName("Lost Target Seconds"), Description("The number of seconds before a target's data block is removed from the scope."), Category("Display Properties")]
        public int LostTargetSeconds { get; set; } = 30;
        [DisplayName("Aircraft Database Cleanup Interval"), Description("The number of seconds between removing aircraft from memory."), Category("Display Properties")]
        public int AircraftGCInterval { get; set; } = 60;

        [DisplayName("Screen Rotation"), Description("The number of degrees to rotate the image"), Category("Display Properties")]
        public double ScreenRotation { get; set; } = 0;
        [DisplayName("Show Range Rings"), Category("Display Properties")]
        public bool ShowRangeRings { get; set; } = true;

        [DisplayName("TPA Size"), Description("Show TPA Mileage Values"), Category("Display Properties")]
        public bool TPASize
        {
            get
            {
                return tpasize;
            }
            set
            {
                lock (Aircraft)
                {
                    Aircraft.ToList().ForEach(x =>
                    {
                        if (x.TPA != null)
                            x.TPA.ShowSize = value;
                    });
                }
                tpasize = value;
            }
        }
        [DisplayName("Quick Look Position List"), Description("Show FDB on these positions"), Category("Display Properties")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, " +
        "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
        typeof(UITypeEditor))]
        public List<string> QuickLookList { get; set; } = new List<string>();
        [DisplayName("Datablock Clock"), Description("Clock phase settings for data blocks"), Category("Data Blocks")]
        public ClockPhase ClockPhase { get; set; } = new ClockPhase();
        //public double TimeshareInterval
        //{
        //    get
        //    {
        //        if (dataBlockTimeshareTimer == null)
        //            return 1.5;
        //        return timeshareinterval / 1000d;
        //    }
        //    set
        //    {
        //        if (value != timeshareinterval && dataBlockTimeshareTimer != null)
        //        {
        //            dataBlockTimeshareTimer.Change(0, (int)(value * 1000));
        //        }
        //        timeshareinterval = (int)(value * 1000d);
        //    }
        //}

        [DisplayName("History Fade"), Description("Whether or not the history returns fade out"), Category("Display Properties")]
        public bool HistoryFade { get; set; } = false;
        [DisplayName("Primary Fade"), Description("Whether or not the primary returns fade out"), Category("Display Properties")]
        public bool PrimaryFade { get; set; } = false;
        [DisplayName("History Direction Angle"), Description("Determines direction of drawing history returns.  If true, they are drawn with respect to the aircraft's track.  " +
            "If false, they retain their direction with respect to the receiving radar site."), Category("Display Properties")]
        public bool HistoryDirectionAngle { get; set; } = false;
        [DisplayName("Invert Mouse"), Description("Invert the direction the mouse will move adjustments in the DCB"), Category("Display Properties")]
        public bool InvertMouse { get; set; } = false;
        [DisplayName("Invert Keypad"), Description("Invert the direction the keypad will move leader lines"), Category("Display Properties")]
        public bool InvertKeyboard { get; set; } = false;
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
        [XmlIgnore]
        public VideoMapList VideoMaps { get; set; } = new VideoMapList();


        private string videoMapFilename = null;
        [Browsable(false)]
        public string VideoMapFilename
        {
            get
            {
                return videoMapFilename;
            }
            set
            {
                    videoMapFilename = value;
                if (string.IsNullOrEmpty(value))
                {
                    LoadVideoMapFile();
                }
            }
        }
        [XmlIgnore]
        public ATPA ATPA = new ATPA();
        [DisplayName("Separation Table"), Category("ATPA")]
        public SeparationTable ATPASeparationTable
        {
            get => ATPA.RequiredSeparation;
            set => ATPA.RequiredSeparation = value;
        }
        [DisplayName("Volumes"), Category("ATPA")]
        public List<ATPAVolume> ATPAVolumes
        {
            get => ATPA.Volumes;
            set => ATPA.Volumes = value;
        }
        [DisplayName("Active"), Description("ATPA active system-wide"), Category("ATPA")]
        public bool ATPAActive
        {
            get => ATPA.Active;
            set => ATPA.Active = value;
        }
        [DisplayName("Excluded ACIDs"), Description("Excluded ACIDs"), Category("ATPA")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, " +
        "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
        typeof(UITypeEditor))]
        public List<string> ATPAExcludedACIDs
        {
            get => ATPA.ExcludedACIDs;
            set => ATPA.ExcludedACIDs = value.ToList();
        }
        [DisplayName("Excluded SSR"), Description("Excluded Secondary Surveillance Radar Beacon Codes"), Category("ATPA")]
        public List<BeaconCodeRange> ATPAExcludedSSR
        {
            get => ATPA.ExcludedSSRCodes;
            set => ATPA.ExcludedSSRCodes = value;
        }
        [DisplayName("Display ATPA Monitor Cones"), Category("ATPA")]
        public bool DrawATPAMonitorCones { get; set; } = false;

        [Editor(typeof(ATPAVolumeSelectorEditor), typeof(UITypeEditor))]
        [DisplayName("Active Volumes"), Category("ATPA")]
        [XmlIgnore]
        public List<ATPAVolume> ActiveVolumes { get; set; } = new List<ATPAVolume>();
        [Editor(typeof(ATPATwoPointFiveSelectorEditor), typeof(UITypeEditor))]
        [DisplayName("Active 2.5 nm Approach Volumes"), Category("ATPA")]
        [XmlIgnore]
        public List<ATPAVolume> ActiveATPATwoPointFive { get; set; } = new List<ATPAVolume>();
        float scale => (float)(CurrentPrefSet.Range); // Math.Sqrt(2));
        float pixelScale;
        //float xPixelScale;// => pixelScale; //2f / window.ClientSize.Width;
        //float yPixelScale;// => pixelScale; // 2f / window.ClientSize.Height;
        float aspect_ratio;// => 1.0f;//(float)window.ClientSize.Width / (float)window.ClientSize.Height;
        float oldar;
        [DisplayName("Selected Beacon Codes"), Category("Display Properties")]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, " +
        "System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
        typeof(UITypeEditor))]
        public List<string> SelectedBeaconCodes { get; set; } = new List<string>();
        [DisplayName("Selected Beacon Code Character"), Category("Display Properties")]
        public char SelectedBeaconCodeChar { get; set; } = '□';

        private string cps = "NONE";
        [XmlIgnore]
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
                    Airports = XmlSerializer<Airports>.DeserializeFromFile(value).Airport.ToList();
                    airportsFileName = value;
                    OrderWaypoints();
                }
                catch
                {
                    Airports = new Airports().Airport.ToList();
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
                    Waypoints = XmlSerializer<Waypoints>.DeserializeFromFile(value).Waypoint.ToList();
                    waypointsFileName = value;
                    OrderWaypoints();
                }
                catch
                {
                    Waypoints = new Waypoints().Waypoint.ToList();
                }
            }
        }
        private string waypointsFileName = "";
        [DisplayName("Altimeter Stations"), Category("Display Properties")]
        public string[] AltimeterStations
        {
            get
            {
                return wx.AltimeterStations.ToArray();
            }
            set
            {
                wx.AltimeterStations = value.ToList();
                cbWxUpdateTimer(null);
            }
        }
        private Radar radar;
        public static ObservableCollection<Aircraft> Aircraft = new ObservableCollection<Aircraft>();
        [Editor(typeof(ReceiverCollectionEditor), typeof(UITypeEditor))]
        [DisplayName("Receivers"), Description("The collection of data receivers for the Radar scope")]
        public ListOfIReceiver Receivers { get; set; } = new ListOfIReceiver();
        private GeoPoint _homeLocation = new GeoPoint(0, 0);
        [DisplayName("Facility Center")]
        public GeoPoint HomeLocation
        {
            get => _homeLocation;
            set
            {
                _homeLocation = value;
                OrderWaypoints();
            }
        }
        [DisplayName("Unassociated Max Altitude"), Category("Altitude Filters"), Description("The maximum altitude of unassociated targets.")]
        public int MaxAltitude { get; set; }

        [DisplayName("Unassociated Minimum Altitude"), Category("Altitude Filters"), Description("The minimum altitude of unassociated targets.")]
        public int MinAltitude { get; set; }
        [DisplayName("Associated Minimum Altitude"), Category("Altitude Filters"), Description("The minimum altitude of associated targets.")]
        public int MinAltitudeAssociated { get; set; } = -9900;
        [DisplayName("Associated Maximum Altitude"), Category("Altitude Filters"), Description("The maximum altitude of associated targets.")]
        public int MaxAltitudeAssociated { get; set; } = 99900;
        [DisplayName("Radar Sites"), Category("Radar Properties")]
        public List<Radar> RadarSites { get; set; } = new List<Radar>();
        [DisplayName("Active Radar Site"), Category("Radar Properties")]
        public int ActiveRadarSite
        {
            get => RadarSites.IndexOf(radar);
            set
            {
                if (value >= RadarSites.Count)
                {
                    radar = RadarSites.Last();
                }
                else if (value < 0)
                {
                    radar = Radar.FUSED;
                }
                else
                {
                    radar = RadarSites[value];
                }
            }
        }


        [DisplayName("Vertical Sync"), Description("Limit FPS to refresh rate of monitor"), Category("Display Properties")]
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
        [DisplayName("Target Extent Symbols"), Description("Parameters for display of targets"), Category("Display Properties")]
        public TargetExtentSymbols TargetExtentSymbols { get; set; } = new TargetExtentSymbols()
        {
            SearchTargets = new TargetExtentSymbols.SearchTargetParams()
            {
                RangeExtent = 5,
                AzimuthExtents = new TargetExtentSymbols.SearchTargetParams.AzimuthExtentValues()
                {
                    Ten = 28,
                    Twenty = 19,
                    Thirty = 16,
                    Forty = 12,
                    Fifty = 11,
                    Sixty = 9
                },
                AzimuthExtentMinimum = 75
            },
            FusedTracks = new TargetExtentSymbols.FusedTrackTargetSymbolParams()
            {
                MinimumPixelDimension = 12,
                NormalSymbolDistanceDimension = 22,
                SymbolOpacity = 100
            },
            BeaconTargets = new TargetExtentSymbols.BeaconTargetParams()
            {
                RangeExtent = 1,
                AzimuthExtentFactor = 20,
                RangeOffset = 3
            },
            PositionSymbolOffset = 8,
            FMATargetSymbols = new TargetExtentSymbols.FMATargetSymbolParams()
            {
                Radius = 3
            }
        };
        [DisplayName("TPA P-Cone Width"), Description("Width of the end of the TPA P-Cone, in pixels"), Category("Display Properties")]
        public float TPAConeWidth { get; set; } = 10;

        [DisplayName("Nexrad Weather Radar")]
        public NexradDisplay Nexrad { get; set; } = new NexradDisplay();
        [Browsable(false)]
        public List<NexradDisplay> Nexrads { get; set; }
        [DisplayName("Data Block Font")]
        [XmlIgnore]
        public Font Font { get; set; } = new Font("Consolas", 10);
        [XmlElement("FontName")]
        [Browsable(false)]
        public string FontName { get { return Font.FontFamily.Name; } set { Font = new Font(value, Font.Size, Font.Unit); } }
        [XmlElement("FontSize")]
        [Browsable(false)]
        public int FontSize { get { return (int)Font.Size; } set { Font = new Font(Font.FontFamily, value, Font.Unit); } }
        [XmlElement("FontSizeUnit")]
        [Browsable(false)]
        public GraphicsUnit FontSizeUnit { get { return Font.Unit; } set { Font = new Font(Font.FontFamily, Font.Size, value); } }
        [DisplayName("DCB Font")]
        [XmlIgnore]
        public Font DCBFont { get => dcb.Font; set => dcb.Font = value; }
        [XmlElement("DBCFontName")]
        [Browsable(false)]
        public string DCBFontName { get { return DCBFont.FontFamily.Name; } set { DCBFont = new Font(value, DCBFont.Size, DCBFont.Unit); } }
        [XmlElement("DBCFontSize")]
        [Browsable(false)]
        public int DCBFontSize { get { return (int)DCBFont.Size; } set { DCBFont = new Font(DCBFont.FontFamily, value, DCBFont.Unit); } }
        [XmlElement("DCBFontSizeUnit")]
        [Browsable(false)]
        public GraphicsUnit DCBFontSizeUnit { get { return DCBFont.Unit; } set { DCBFont = new Font(DCBFont.FontFamily, DCBFont.Size, value); } }
        [DisplayName("Auto Offset Enabled"), Description("Attempt to deconflict overlapping data blocks"), Category("Data Blocks")]
        public bool AutoOffset { get; set; } = false;
        [DisplayName("Server Address"), Category("NTP")]
        public string NTPServerAddress
        {
            get => timesync.Server;
            set
            {
                timesync.Server = value;
            }
        }
        [DisplayName("Time Sync Inverval"), Category("NTP")]
        [XmlIgnore]
        public TimeSpan NTPInterval
        {
            get => timesync.TimeSyncInterval;
            set
            {
                timesync.TimeSyncInterval = value;
            }
        }
        [Browsable(false)]
        [XmlElement("NTPInverval")]
        public int NTPIntervalMs
        {
            get => (int)NTPInterval.TotalMilliseconds;
            set
            {
                NTPInterval = TimeSpan.FromMilliseconds(value);
            }
        }
        [Browsable(false)]
        public PointF PreviewLocation
        { get; set; }
        [Browsable(false)]
        public PointF StatusLocation
        { get; set; }
        [DisplayName("Wind in Status Area"), Description("Show wind values in Status Area"), Category("Display Properties")]
        public bool WindInStatusArea { get; set; } = false;
        [DisplayName("FPS in Status Area"), Description("Show FPS in Status Area"), Category("Display Properties")]
        public bool FPSInStatusArea { get; set; } = false;
        [DisplayName("Use ADS-B Callsigns Unassociated"), Description("Use the ADS-B Callsign for unassociated tracks"), Category("Display Properties")]
        public bool UseADSBCallsigns { get; set; } = false;
        [DisplayName("Use ADS-B Callsigns Unassociated 1200"), Description("Use the ADS-B Callsign for unassociated tracks squawking 1200"), Category("Display Properties")]
        public bool UseADSBCallsigns1200 { get; set; } = false;
        [DisplayName("Use ADS-B Callsigns Associated"), Description("Use the ADS-B Callsign for associated tracks"), Category("Display Properties")]
        public bool UseADSBCallsignsAssociated { get; set; } = false;
        [DisplayName("QuickLook"), Description("QuickLook all tracks, including unassociated"), Category("Display Properties")]
        public bool QuickLook { get; set; } = false;
        [DisplayName("Pref Set"), Category("Display Properties")]
        public PrefSet CurrentPrefSet
        {
            get => prefSet;
            set
            {
                prefSet = value;
                VideoMaps.ForEach(x => x.Visible = value.DisplayedMaps.Contains(x.Number));
            }
        }
        char?[] atises = new char?[10];
        string?[] gentexts = new string?[10];
        private PrefSet prefSet = new PrefSet();
        private GameWindow window;
        private bool isScreenSaver = false;
        private bool tpasize = true;
        private GeoPoint ScreenCenterPoint => CurrentPrefSet.ScopeCentered ? HomeLocation : CurrentPrefSet.ScreenCenterPoint;
        private GeoPoint RangeRingCenter => CurrentPrefSet.RangeRingsCentered ? HomeLocation : CurrentPrefSet.RangeRingLocation;
        private static bool timeManual = false;
        private static DateTime manualTime;
        private static Stopwatch manualTimer = new Stopwatch();
        [XmlIgnore]
        public static DateTime CurrentTime
        {
            get
            {
                if (timeManual)
                {
                    return manualTime + manualTimer.Elapsed;
                }
                return timesync.CurrentTime();
            }
            set
            {
                manualTime = value;
                manualTimer.Restart();
                timeManual = true;
            }
        }
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
        List<MinSep> minSeps = new List<MinSep>();
        Timer wxUpdateTimer;
        int timeshareinterval = 1500;
        Timer dataBlockTimeshareTimer;
        List<WaypointsWaypoint> Waypoints = new Waypoints().Waypoint.ToList();
        List<Airport> Airports = new Airports().Airport.ToList();
        WeatherService wx = new WeatherService();

        public static Color AdjustedColor(Color color, int brightness)
        {
            double brightnesslevel = brightness / 100d;
            var a = (int)(color.A * 1);
            if (brightnesslevel == 0)
            {
                a = 0;
            }
            var r = (int)(color.R * brightnesslevel);
            var g = (int)(color.G * brightnesslevel);
            var b = (int)(color.B * brightnesslevel);
            return Color.FromArgb(a, r, g, b);
        }
        public void LoadVideoMapFile()
        {
            try
            {
                VideoMaps = VideoMapList.DeserializeFromJsonFile(videoMapFilename);
                VideoMaps.ForEach(x => x.Visible = CurrentPrefSet.DisplayedMaps.Contains(x.Number));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
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
            window.KeyPress += Window_KeyPress;
            window.KeyUp += Window_KeyUp;
            window.MouseWheel += Window_MouseWheel;
            window.MouseMove += Window_MouseMove;
            window.MouseDown += Window_MouseDown;
            window.MouseUp += Window_MouseUp;
            if (RadarSites.Count > 0)
                radar = RadarSites[0];
            else
                radar = new Radar();
            aircraftGCTimer = new Timer(new TimerCallback(cbAircraftGarbageCollectorTimer), null, AircraftGCInterval * 1000, AircraftGCInterval * 1000);
            wxUpdateTimer = new Timer(new TimerCallback(cbWxUpdateTimer), null, 0, 180000);
            GL.ClearColor(AdjustedColor(BackColor, CurrentPrefSet.Brightness.Background));
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
            Aircraft.CollectionChanged += Aircraft_CollectionChanged;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentPrefSet.DCBVisible)
                dcb.ActiveMenu.MouseUp();
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
                        if (item.Altitude == null)
                            item.Altitude = new Altitude();
                        item.Altitude.SetAltitudeProperties(18000, wx.Altimeter);
                        item.SetSelectedSquawkList(SelectedBeaconCodes, SelectedBeaconCodeChar);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (Aircraft item in e.OldItems)
                    {
                        item.HandedOff -= Aircraft_HandedOff;
                        item.OwnershipChange -= Aircraft_OwnershipChange;
                        item.HandoffInitiated -= Aircraft_HandoffInitiated;

                        DeletePlane(item, false);
                    }
                    break;
            }
        }

        private void DeletePlane(Aircraft plane, bool leaveHistory = true)
        {
            lock (plane)
            {
                lock (dataBlocks)
                {
                    dataBlocks.Remove(plane.DataBlock);
                    dataBlocks.Remove(plane.DataBlock2);
                    dataBlocks.Remove(plane.DataBlock3);
                }
                lock (posIndicators)
                {
                    posIndicators.Remove(plane.PositionIndicator);
                }
                lock (rangeBearingLines)
                    rangeBearingLines.RemoveAll(line => line.EndPlane == plane || line.StartPlane == plane);
                lock (minSeps)
                    minSeps.RemoveAll(minsep => minsep.Plane1 == plane || minsep.Plane2 == plane);
                plane.Deleted = true;
                lock (deletedPlanes)
                    deletedPlanes.Add(plane);
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
            /*if (e.Aircraft.LastPositionTime > CurrentTime.AddSeconds(-LostTargetSeconds))
                GenerateDataBlock(e.Aircraft);*/
        }

        private void Aircraft_OwnershipChange(object sender, AircraftEventArgs e)
        {
            /*e.Aircraft.RedrawDataBlock();*/
        }
        private void PositionChange()
        {
            lock (Aircraft)
            {
                var aclist = Aircraft.ToList();
                aclist.ForEach(x => x.Owned = x.PositionInd == ThisPositionIndicator || x.PendingHandoff == ThisPositionIndicator);
                aclist.ForEach(x => x.DataBlock.Flashing = x.PendingHandoff == ThisPositionIndicator);
                aclist.ForEach(x => x.DataBlock2.Flashing = x.PendingHandoff == ThisPositionIndicator);
                aclist.ForEach(x => x.DataBlock3.Flashing = x.PendingHandoff == ThisPositionIndicator);
                aclist.ForEach(x => x.FDB = false);
            }
            QuickLookList.Remove(ThisPositionIndicator);
            QuickLookList.Remove(ThisPositionIndicator + "+");
        }
        private void Aircraft_HandedOff(object sender, HandoffEventArgs e)
        {
            /*e.Aircraft.RedrawDataBlock(false);*/
        }

        byte[] settingshash;
        string settingsPath;
        public void Run(bool isScreenSaver, string settingsPath)
        {
            this.settingsPath = settingsPath;
            this.isScreenSaver = isScreenSaver;
            window.Run();
        }

        private void cbWxUpdateTimer(object state)
        {
            Task.Run(() => wx.GetWeather(true));
        }



        private void cbAircraftGarbageCollectorTimer(object state)
        {
            List<Aircraft> delplane;
            lock (Aircraft)
                delplane = Aircraft.Where(x => x.LastMessageTime < CurrentTime.AddSeconds(-AircraftGCInterval)).ToList();
            foreach (var plane in delplane)
            {
                lock (Aircraft)
                    Aircraft.Remove(plane);
                DeletePlane(plane, false);
            }
        }
        private List<Aircraft> deletedPlanes = new List<Aircraft>();
        private void DeleteTextures()
        {
            lock (deletedPlanes)
            {
                deletedPlanes.ToList().ForEach(plane =>
                {
                    if (plane.DataBlock.TextureID != 0)
                    {
                        GL.DeleteTexture(plane.DataBlock.TextureID);
                        GL.DeleteTexture(plane.DataBlock2.TextureID);
                        GL.DeleteTexture(plane.DataBlock3.TextureID);
                        GL.DeleteTexture(plane.PositionIndicator.TextureID);
                        plane.DataBlock.TextureID = 0;
                        plane.DataBlock2.TextureID = 0;
                        plane.DataBlock3.TextureID = 0;
                        plane.PositionIndicator.TextureID = 0;
                        plane.DataBlock.Dispose();
                        plane.DataBlock2.Dispose();
                        plane.DataBlock3.Dispose();
                        plane.PositionIndicator.Dispose();
                    }
                    lock (plane.History)
                    {
                        for (int i = 0; i < plane.History.Length; i++)
                        {
                            if (plane.History[i] != null)
                            {
                                plane.History[i].Dispose();
                            }
                        }
                    }
                    plane.TargetReturn.Dispose();
                    deletedPlanes.Remove(plane);
                });
            }
        }
        public void StartReceivers()
        {
            foreach (Receiver receiver in Receivers)
            {
                receiver.SetAircraftList(Aircraft);
                receiver.SetWeatherRadarDisplay(Nexrad);
                if (receiver.Enabled)
                    try
                    {
                        receiver.Start();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("An error occured starting receiver {0}.\r\n{1}",
                            receiver.Name, ex.Message), "Error starting receiver", System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Warning);
                    }
            }
        }

        public void StopReceivers()
        {
            foreach (Receiver receiver in Receivers)
                receiver.Stop();
        }
        private void OrderWaypoints()
        {
            Waypoints = Waypoints.ToList().OrderBy(x => x.Location.DistanceTo(HomeLocation)).ToList();
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
            if (CurrentPrefSet.DCBVisible)
                dcb.ActiveMenu.MouseMove(e.Position);
            if (tempLine != null)
                tempLine.End = LocationFromScreenPoint(e.Position);
            if (!e.Mouse.IsAnyButtonDown)
            {

                double move = Math.Sqrt(Math.Pow(e.XDelta, 2) + Math.Pow(e.YDelta, 2));
                if (move > 10 && isScreenSaver && _mousesettled)
                {
                    StopReceivers();
                    Environment.Exit(0);
                }
                _mousesettled = true;

            }
            else if (e.Mouse.RightButton == ButtonState.Pressed)
            {
                /*if (centeredmouse)
                    return;
                float xMove = e.XDelta * pixelScale;
                float yMove = e.YDelta * pixelScale;
                var center = new Vector4((float)ScreenCenterPoint.Longitude, (float)ScreenCenterPoint.Latitude, 0.0f, 1.0f);
                Vector4 move = new Vector4(-xMove, yMove, 0.0f, 1.0f);
                move *= rotscale;
                var trans = Matrix4.CreateTranslation(move.X, move.Y, move.Z);
                center *= trans;
                CurrentPrefSet.ScopeCentered = false;
                CurrentPrefSet.ScreenCenterPoint = new GeoPoint(center.Y, center.X);
                */
            }
        }
        bool hidewx = false;
        private object ClickedObject(Point ClickedPoint)
        {
            PointF clickpoint;
            if (ClickedPoint == null)
                clickpoint = LocationFromScreenPoint(new Point((int)mouseprev.X, (int)mouseprev.Y));
            else
                clickpoint = LocationFromScreenPoint(ClickedPoint);
            object clicked;
            if (CurrentPrefSet.DCBVisible && dcb.ActiveMenu.DrawnBounds.Contains(ClickedPoint))
            {
                return ClickedPoint;
            }
            lock (Aircraft)
            {
                clicked = Aircraft.Where(x => x.PositionIndicator.BoundsF.Contains(clickpoint)
                && x.LastPositionTime > CurrentTime.AddSeconds(-LostTargetSeconds)
                && x.TargetReturn.Intensity > .001).FirstOrDefault();
                if (clicked == null)
                {
                    clicked = clickpoint;
                }
            }
            return clicked;
        }
        Aircraft debugPlane;
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            object clicked;
            bool enterclick = false;
            Point mousepos;
            if (e != null)
            {
                mousepos = e.Position;
            }
            else
            {
                mousepos = new Point((int)mouseprev.X, (int)mouseprev.Y);
                enterclick = true;
            }
            clicked = ClickedObject(mousepos);
            if (CurrentPrefSet.DCBVisible)
                dcb.ActiveMenu.MouseDown();
            if (enterclick || e.Mouse.LeftButton == ButtonState.Pressed)
            {
                if ((Keyboard.GetState().IsKeyDown(Key.ControlLeft) || Keyboard.GetState().IsKeyDown(Key.ControlRight)) &&
                    (Keyboard.GetState().IsKeyDown(Key.ShiftLeft) || Keyboard.GetState().IsKeyDown(Key.ShiftRight)))
                {
                    System.Windows.Forms.Clipboard.SetText(ScreenToGeoPoint(e.Position).ToString());
                }
                else if (activeDcbButton != null && activeDcbButton.GetType() != typeof(DCBSubmenuButton))
                {
                    if (activeDcbButton == dcbPlaceRRButton)
                    {
                        CurrentPrefSet.RangeRingLocation = ScreenToGeoPoint(e.Position);
                        CurrentPrefSet.RangeRingsCentered = false;
                    }
                    else if (activeDcbButton.GetType() == typeof(DCBAdjustmentButton))
                    {
                        var loc = new Point(window.Location.X + activeDcbButton.DrawnBounds.X + activeDcbButton.Width / 2, window.Location.Y + activeDcbButton.DrawnBounds.Y + activeDcbButton.Height / 2);
                        Mouse.SetPosition(loc.X, loc.Y);
                        window.CursorVisible = true;
                    }
                    ReleaseDCBButton();
                }
                else if (tempLine == null)
                {
                    ProcessCommand(Preview, clicked);
                }
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
                    //GenerateDataBlock(plane);
                }
            }
            else if (e.Mouse.RightButton == ButtonState.Pressed)
            {
                if (clicked.GetType() == typeof(Aircraft))
                {
                    debugPlane = (Aircraft)clicked;
                }
            }
        }

        private PointF LocationFromScreenPoint(Point point)
        {
            var vec = new Vector4(point.X, point.Y, 0, 1);
            vec *= pixeltransform;
            return new PointF(vec.X, vec.Y);
            float x = (2 * (point.X / (float)window.ClientSize.Width) - 1);
            float y = 1 - 2 * (point.Y / (float)window.ClientSize.Height);
            if (window.ClientSize.Width > window.ClientSize.Height)
            {
                x *= aspect_ratio;
            }
            else
            {
                y /= aspect_ratio;
            }
            return new PointF(x, y);
        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var button = activeDcbButton as DCBAdjustmentButton;
            if (button != null)
            {
                button.MouseWheel(e.Delta);
            }
            /*
            if (e.Delta > 0 && CurrentPrefSet.Range > 6)
                CurrentPrefSet.Range -= 1;
            else if (e.Delta < 0)
                CurrentPrefSet.Range += 1;
            */
        }

        public enum KeyCode
        {
            Min = 59,
            InitCntl = 12,
            TermCntl = 13,
            HndOff = 14,
            VP = 15,
            MultiFunc = 16,
            FltData = 18,
            CA = 20,
            SignOn = 21,
            RngRing = 201,
            WX = 202,
            RecenterEverything = 500
        }

        public List<object> Preview = new List<object>();


        bool waitingfortarget = false;
        RangeBearingLine tempLine;
        MinSep tempMinSep;
        private void ProcessCommand(List<object> KeyList, object clicked = null)
        {
            bool clickedplane = false;
            bool enter = false;
            if (KeyList.Count == 0 && clicked != null) // no keys, implied command
            {
                ProcessImpliedCommand(clicked);
                return;
            }
            if (clicked != null)
                clickedplane = clicked.GetType() == typeof(Aircraft);
            else
            {
                enter = true;
            }

            if (enter && CurrentPrefSet.DCBVisible)
            {
                dcb.ActiveMenu.MouseDown();
                dcb.ActiveMenu.MouseUp();
            }
            if (KeyList.Count < 1 && clicked != null && clicked.GetType() == typeof(Aircraft))
            {
                Aircraft plane = (Aircraft)clicked;
                if (plane.ForceQuickLook)
                    plane.ForceQuickLook = false;
                else if (!plane.Owned)
                    plane.FDB = plane.FDB ? false : true;
                else if (plane.PositionInd != ThisPositionIndicator)
                {
                    plane.Owned = false;
                }
                //GenerateDataBlock(plane);
            }
            else if (KeyList.Count > 0)
            {
                var commands = KeyList.Count(x =>
                {
                    var type = x.GetType();
                    if (type == typeof(char))
                        if ((char)x == ' ')
                            return true;
                    return false;
                }) + 1;
                var count = 0;
                object[][] keys = new object[commands][];
                for (int i = 0; i < commands; i++)
                {
                    List<object> command = new List<object>();
                    for (; count < KeyList.Count; count++)
                    {
                        if ((KeyList[count].GetType() != typeof(char) || (char)KeyList[count] != ' '))
                        //if ((int)KeyList[count] != (int)Key.Space) 
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
                lock (Aircraft)
                    typed = Aircraft.Where(x => x.FlightPlanCallsign != null).ToList()
                        .Find(x => x.FlightPlanCallsign.Trim() == lastline.Trim());
                if (typed == null)
                {
                    lock (Aircraft)
                        typed = Aircraft.Where(x => x.Squawk != null).ToList()
                            .Find(x => x.Squawk.Trim() == lastline.Trim());
                }
                if (!string.IsNullOrEmpty(lastline.Trim()) && !clickedplane && typed != null)
                {
                    if (typed.Squawk != "1200" && typed.Squawk != null)
                    {
                        clicked = typed;
                        clickedplane = true;
                    }
                }
                if (keys[0].Length < 1)
                    return;
                switch (keys[0][0])
                {
                    case '1' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            if (!InvertKeyboard)
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.NW;
                            else
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.SW;
                            lock(clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '2' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            if (!InvertKeyboard)
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.N;
                            else
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.S;
                            lock (clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '3' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            if (!InvertKeyboard)
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.NE;
                            else
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.SE;
                            lock (clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '4' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.W;
                            lock (clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '5' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = null;
                            lock (clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '6' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            ((Aircraft)clicked).LDRDirection = LeaderDirection.E;
                            ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '7' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            if (!InvertKeyboard)
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.SW;
                            else
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.NW;
                            lock (clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '8' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            if (!InvertKeyboard)
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.S;
                            else
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.N;
                            lock (clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case '9' when keys[0].Length == 1:
                        if (clickedplane)
                        {
                            if (!InvertKeyboard)
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.SE;
                            else
                                ((Aircraft)clicked).LDRDirection = LeaderDirection.NE;
                            lock (clicked) ((Aircraft)clicked).RedrawDataBlock(radar);
                            Preview.Clear();
                        }
                        break;
                    case Key.F3:
                        /*if (clickedplane)
                        {
                            ((Aircraft)clicked).Owned = true;
                            ((Aircraft)clicked).PositionInd = ThisPositionIndicator;
                            Preview.Clear();
                        }
                        */
                        break;
                    case Key.F4:
                        if (clickedplane)
                        {
                            var plane = (Aircraft)clicked;
                            if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                            {
                                DisplayPreviewMessage("ILL TRK");
                            }
                            else
                            {
                                Preview.Clear();
                                plane.DeleteFP();
                            }
                        }
                        else
                        {
                            DisplayPreviewMessage("NO FLIGHT");
                        }
                        break;
                    case Key.F12:
                        var newpos = KeysToString(keys[0]);
                        if (newpos == "*")
                            ThisPositionIndicator = "NONE";
                        else
                            ThisPositionIndicator = newpos;
                        lock (Aircraft)
                        {
                            Aircraft.Where(x => x.PositionInd != ThisPositionIndicator &&
                            x.PendingHandoff != ThisPositionIndicator).ToList().ForEach(x => x.Owned = false);
                        }
                        Preview.Clear();
                        break;
                    case '*': // splat commands
                        if (keys[0].Length >= 2)
                        {
                            switch (keys[0][1])
                            {
                                case 'B':
                                    if (keys[0].Length == 3)
                                        if (enter)
                                        {
                                            if ((char)keys[0][2] == 'E')
                                                DrawATPAMonitorCones = true;
                                            else if ((char)keys[0][2] == 'I')
                                                DrawATPAMonitorCones = false;
                                            Preview.Clear();
                                        }
                                    break;
                                case 'D':
                                    if ((keys[0].Length == 3 || keys[0].Length == 4) && keys[0][2].GetType() == typeof(char) && (char)keys[0][2] == '+')
                                    {
                                        if (enter)
                                        {
                                            if (keys[0].Length == 3)
                                            {
                                                TPASize = !TPASize;
                                                Preview.Clear();
                                            }
                                            else if (keys[0][3].GetType() != typeof(char))
                                            {
                                                DisplayPreviewMessage("FORMAT");
                                            }
                                            else if ((char)keys[0][3] == 'E')
                                            {
                                                TPASize = true;
                                                Preview.Clear();
                                            }
                                            else if ((char)keys[0][3] == 'I')
                                            {
                                                TPASize = false;
                                                Preview.Clear();
                                            }
                                            else
                                            {
                                                DisplayPreviewMessage("FORMAT");
                                            }
                                        }
                                        else if (clickedplane)
                                        {
                                            var plane = clicked as Aircraft;
                                            if (plane.TPA == null)
                                            {
                                                DisplayPreviewMessage("ILL FNCT");
                                            }
                                            else if (keys[0].Length == 3)
                                            {
                                                plane.TPA.ShowSize = !plane.TPA.ShowSize;
                                                Preview.Clear();
                                            }
                                            else if (keys[0][3].GetType() != typeof(char))
                                            {
                                                DisplayPreviewMessage("FORMAT");
                                            }
                                            else if ((char)keys[0][3] == 'E')
                                            {
                                                plane.TPA.ShowSize = true;
                                                Preview.Clear();
                                            }
                                            else if ((char)keys[0][3] == 'I')
                                            {
                                                plane.TPA.ShowSize = false;
                                                Preview.Clear();
                                            }
                                            else
                                            {
                                                DisplayPreviewMessage("FORMAT");
                                            }
                                        }
                                        else
                                        {
                                            DisplayPreviewMessage("NO TRK");
                                        }
                                    }
                                    break;
                                case 'T':
                                    if (clickedplane)
                                    {
                                        if (tempLine == null)
                                        {
                                            tempLine = new RangeBearingLine() { StartPlane = (Aircraft)clicked, End = LocationFromScreenPoint(MouseLocation) };
                                            lock (rangeBearingLines)
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
                                            var entered = KeysToString(keys[0]).Substring(2);
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
                                case 'J':
                                    if (clickedplane)
                                    {
                                        if (keys[0].Length >= 3 && keys[0].Length <= 5)
                                        {
                                            decimal miles = 0;
                                            var entered = KeysToString(keys[0]).Substring(2);
                                            if (decimal.TryParse(entered, out miles))
                                            {
                                                if (miles > 0 && (double)miles <= 30)
                                                {
                                                    ((Aircraft)clicked).TPA = new TPARing((Aircraft)clicked, miles, TPAColor, Font, TPASize);
                                                }
                                                else
                                                {
                                                    DisplayPreviewMessage("FORMAT");
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
                                case 'P':
                                    if (clickedplane)
                                    {
                                        if (keys[0].Length >= 3 && keys[0].Length <= 5)
                                        {
                                            decimal miles = 0;
                                            var entered = KeysToString(keys[0]).Substring(2);
                                            if (decimal.TryParse(entered, out miles))
                                            {
                                                if (miles > 0 && (double)miles <= 30)
                                                {
                                                    ((Aircraft)clicked).TPA = new TPACone((Aircraft)clicked, miles, TPAColor, Font, TPASize);
                                                }
                                                else
                                                {
                                                    DisplayPreviewMessage("FORMAT");
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
                                    else
                                    {
                                        DisplayPreviewMessage("NO TRK");
                                    }
                                    break;
                                case '*':
                                    if (keys[0].Length > 2)
                                    {
                                        switch (keys[0][2])
                                        {
                                            case 'J':
                                                lock (Aircraft)
                                                    Aircraft.Where(x => x.TPA != null).Where(x => x.TPA.Type == TPAType.JRing).ToList().ForEach(x => x.TPA = null);
                                                Preview.Clear();
                                                break;
                                            case 'P':
                                                lock (Aircraft)
                                                    Aircraft.Where(x => x.TPA != null).Where(x => x.TPA.Type == TPAType.PCone).ToList().ForEach(x => x.TPA = null);
                                                Preview.Clear();
                                                break;
                                            default:
                                                if (keys[0].Length == 4)
                                                {
                                                    var pos = KeysToString(keys[0]).Substring(2);
                                                    if (clickedplane && pos == ThisPositionIndicator)
                                                    {
                                                        var plane = clicked as Aircraft;
                                                        plane.ForceQuickLook = true;
                                                        //GenerateDataBlock(plane);
                                                        Preview.Clear();
                                                    }
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case '.':
                        if (keys[0].Length == 1 && clickedplane)
                        {
                            var plane = clicked as Aircraft;
                            if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                            {
                                DisplayPreviewMessage("ILL TRK");
                            }
                            else
                            {
                                plane.Scratchpad = "";
                                Preview.Clear();
                                plane.SendUpdate();
                            }
                        }
                        break;
                    case '+':
                        if (keys[0].Length == 1 && clickedplane)
                        {
                            var plane = clicked as Aircraft;
                            if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                            {
                                DisplayPreviewMessage("ILL TRK");
                            }
                            else
                            {
                                plane.Scratchpad2 = "";
                                Preview.Clear();
                                plane.SendUpdate();
                            }
                        }
                        break;
                    case Key.F7:
                        //MultiFuntion
                        if (keys[0].Count() < 2)
                            break;
                        switch (keys[0][1])
                        {
                            case '2': //Multifunction 2
                                if (keys[0].Length >= 6 && KeysToString(keys[0], 2).Substring(0, 4) == "ATPA") //ATPA Commands
                                {
                                    if (keys[0].Length == 7) // Enable system-wide
                                    {
                                        if ((char)keys[0][6] == 'E') // Enable
                                        {
                                            if (ATPA.Active)
                                            {
                                                DisplayPreviewMessage("NO CHANGE");
                                            }
                                            else if (ATPA.Volumes.Count == 0)
                                            {
                                                DisplayPreviewMessage("ILL FNCT");
                                            }
                                            else
                                            {
                                                ATPA.Active = true;
                                                Preview.Clear();
                                            }
                                        }
                                        else if ((char)keys[0][6] == 'I') //Inhibit
                                        {
                                            if (!ATPA.Active)
                                            {
                                                DisplayPreviewMessage("NO CHANGE");
                                            }
                                            else if (ATPA.Volumes.Count == 0)
                                            {
                                                DisplayPreviewMessage("ILL FNCT");
                                            }
                                            else
                                            {
                                                ATPA.Active = false;
                                                Preview.Clear();
                                            }
                                        }
                                        else
                                        {
                                            DisplayPreviewMessage("FORMAT");
                                        }
                                    }
                                    if (keys[0].Length >= 8 && keys[0].Length <= 12)
                                    {
                                        if (ATPA.Active)
                                        {
                                            var volnamefull = KeysToString(keys[0], 6);
                                            var volname = volnamefull.Substring(0, volnamefull.Length - 1);
                                            var volumes = ATPA.Volumes.Where(x => x.VolumeId == volname);
                                            if (volumes.Count() == 1)
                                            {
                                                var volume = volumes.First();
                                                if (volnamefull.Last() == 'E')
                                                {
                                                    if (volume.Active)
                                                    {
                                                        DisplayPreviewMessage("NO CHANGE");
                                                    }
                                                    else
                                                    {
                                                        volume.Active = true;
                                                        Preview.Clear();
                                                    }
                                                }
                                                else if (volnamefull.Last() == 'I')
                                                {
                                                    if (!volume.Active)
                                                    {
                                                        DisplayPreviewMessage("NO CHANGE");
                                                    }
                                                    else
                                                    {
                                                        volume.Active = false;
                                                        Preview.Clear();
                                                    }
                                                }
                                                else
                                                {
                                                    DisplayPreviewMessage("FORMAT");
                                                }
                                            }
                                            else
                                            {
                                                DisplayPreviewMessage("ILL VOL");
                                            }
                                        }
                                        else
                                        {
                                            DisplayPreviewMessage("ILL FNCT");
                                        }
                                    }
                                }
                                else if (keys[0].Length >= 4 && KeysToString(keys[0], 1).Substring(0, 3) == "2.5")
                                {
                                    if (keys[0].Length >= 6 && keys[0].Length <= 10)
                                    {
                                        if (ATPA.Active)
                                        {
                                            var volnamefull = KeysToString(keys[0], 4);
                                            var volname = volnamefull.Substring(0, volnamefull.Length - 1);
                                            var volumes = ATPA.Volumes.Where(x => x.VolumeId == volname && x.Active);
                                            if (volumes.Count() == 1)
                                            {
                                                var volume = volumes.First();
                                                if (volume.TwoPointFiveEnabled)
                                                {
                                                    if (volnamefull.Last() == 'E')
                                                    {
                                                        if (volume.TwoPointFiveActive)
                                                        {
                                                            DisplayPreviewMessage("NO CHANGE");
                                                        }
                                                        else
                                                        {
                                                            volume.TwoPointFiveActive = true;
                                                            Preview.Clear();
                                                        }
                                                    }
                                                    else if (volnamefull.Last() == 'I')
                                                    {
                                                        if (!volume.TwoPointFiveActive)
                                                        {
                                                            DisplayPreviewMessage("NO CHANGE");
                                                        }
                                                        else
                                                        {
                                                            volume.TwoPointFiveActive = false;
                                                            Preview.Clear();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        DisplayPreviewMessage("FORMAT");
                                                    }
                                                }
                                                else
                                                {
                                                    DisplayPreviewMessage("ILL FNCT");
                                                }
                                            }
                                            else
                                            {
                                                DisplayPreviewMessage("ILL VOL");
                                            }
                                        }
                                        else
                                        {
                                            DisplayPreviewMessage("ILL FNCT");
                                        }
                                    }
                                    else
                                    {
                                        DisplayPreviewMessage("FORMAT");
                                    }
                                }
                                else
                                {
                                    DisplayPreviewMessage("FORMAT");
                                }
                                break;
                            case 'B': //Mutlifunction B: Beacons
                                if (keys[0].Length >= 4 && keys[0].Length <= 6 && enter)
                                {
                                    var squawk = KeysToString(keys[0], 2);
                                    if (SelectedBeaconCodes.Contains(squawk))
                                        SelectedBeaconCodes.RemoveAll(x => x == squawk);
                                    else
                                        SelectedBeaconCodes.Add(squawk);
                                    Preview.Clear();
                                }
                                else if (keys[0].Length == 3 && (int)keys[0][2] == (int)Key.KeypadMultiply)
                                {
                                    SelectedBeaconCodes.Clear();
                                    Preview.Clear();
                                }
                                break;
                            case 'D': //Multifunction D
                                if (keys[0].Length == 3 && (char)keys[0][2] == '*' && !enter)
                                {
                                    GeoPoint clickedlocation;
                                    if (clicked.GetType() == typeof(PointF))
                                    {
                                        clickedlocation = ScreenToGeoPoint((PointF)clicked);
                                        DisplayPreviewMessage(clickedlocation.ToDmsString(), 30);
                                    }
                                }
                                Preview.Clear();
                                break;
                            case 'F': //Multifunction F: Filters
                                bool success = false;
                                if (keys[0].Length == 8)
                                {
                                    var alts = KeysToString(keys[0], 2);
                                    if (int.TryParse(alts.Substring(0, 3), out int min))
                                    {
                                        if (int.TryParse(alts.Substring(3), out int max))
                                        {
                                            if (min == 0)
                                            {
                                                MinAltitude = -9990;
                                            }
                                            else
                                            {
                                                MinAltitude = min * 100;
                                            }
                                            MaxAltitude = max * 100;
                                            success = true;
                                            Preview.Clear();
                                        }
                                        else
                                        {
                                            DisplayPreviewMessage("FORMAT");
                                        }
                                    }
                                    else
                                    {
                                        DisplayPreviewMessage("FORMAT");
                                    }
                                }
                                if (keys.Length == 2 && keys[1].Length == 6)
                                {
                                    var alts = KeysToString(keys[1]);
                                    if (int.TryParse(alts.Substring(0, 3), out int min))
                                    {
                                        if (int.TryParse(alts.Substring(3), out int max))
                                        {
                                            if (min == 0)
                                            {
                                                MinAltitudeAssociated = -9990;
                                            }
                                            else
                                            {
                                                MinAltitudeAssociated = min * 100;
                                            }
                                            MaxAltitudeAssociated = max * 100;
                                            Preview.Clear();
                                        }
                                        else
                                        {
                                            DisplayPreviewMessage("FORMAT");
                                        }
                                    }
                                    else
                                    {
                                        DisplayPreviewMessage("FORMAT");
                                    }
                                }
                                else if (keys.Length != 1)
                                {
                                    DisplayPreviewMessage("FORMAT");
                                }
                                if (!success)
                                {
                                    DisplayPreviewMessage("FORMAT");
                                }
                                break;
                            case 'L': //Leader Lines
                                if (keys[0].Length > 2)
                                {
                                    int dirpos = 2;
                                    string pos = null;
                                    if (keys[0].Length == 5)
                                    {
                                        dirpos += 2;
                                        pos = KeysToString(keys[0], 2).Substring(0, 2);
                                    }
                                    if (!int.TryParse((keys[0][dirpos]).ToString(), out int dir))
                                    {
                                        Preview.Clear();
                                        DisplayPreviewMessage("FORMAT");
                                        break;
                                    }
                                    LeaderDirection direction;
                                    switch (dir)
                                    {
                                        case 7 when InvertKeyboard:
                                            direction = LeaderDirection.NW;
                                            break;
                                        case 7 when !InvertKeyboard:
                                            direction = LeaderDirection.SW;
                                            break;
                                        case 8 when InvertKeyboard:
                                            direction = LeaderDirection.N;
                                            break;
                                        case 8 when !InvertKeyboard:
                                            direction = LeaderDirection.S;
                                            break;
                                        case 9 when InvertKeyboard:
                                            direction = LeaderDirection.NE;
                                            break;
                                        case 9 when !InvertKeyboard:
                                            direction = LeaderDirection.SE;
                                            break;
                                        case 4:
                                            direction = LeaderDirection.W;
                                            break;
                                        case 6:
                                            direction = LeaderDirection.E;
                                            break;
                                        case 1 when !InvertKeyboard:
                                            direction = LeaderDirection.NW;
                                            break;
                                        case 1 when InvertKeyboard:
                                            direction = LeaderDirection.SW;
                                            break;
                                        case 2 when !InvertKeyboard:
                                            direction = LeaderDirection.N;
                                            break;
                                        case 2 when InvertKeyboard:
                                            direction = LeaderDirection.S;
                                            break;
                                        case 3 when !InvertKeyboard:
                                            direction = LeaderDirection.NE;
                                            break;
                                        case 3 when InvertKeyboard:
                                            direction = LeaderDirection.SE;
                                            break;
                                        default:
                                            direction = LeaderDirection.Invalid;
                                            break;
                                    }
                                    if (keys[0].Length == 3)
                                    {
                                        if (direction != LeaderDirection.Invalid)
                                        {
                                            CurrentPrefSet.OwnedDataBlockPosition = direction;
                                        }
                                    }
                                    else if (keys[0].Length == 4 && (char)keys[0][3] == '*')
                                    {
                                        if (direction != LeaderDirection.Invalid)
                                        {
                                            CurrentPrefSet.UnownedDataBlockPosition = direction;
                                        }
                                    }
                                    else if (keys[0].Length == 4 && (char)keys[0][3] == 'U')
                                    {
                                        if (direction != LeaderDirection.Invalid)
                                        {
                                            CurrentPrefSet.UnassociatedDataBlockPosition = direction;
                                        }
                                    }
                                    else if (pos != null)
                                    {
                                        lock (CurrentPrefSet.OtherOwnersLeaderDirections)
                                        {
                                            if (CurrentPrefSet.OtherOwnersLeaderDirections.ContainsKey(pos))
                                            {
                                                if (dir == 5)
                                                {
                                                    CurrentPrefSet.OtherOwnersLeaderDirections.Remove(pos);
                                                }
                                                else if (direction != LeaderDirection.Invalid)
                                                {
                                                    CurrentPrefSet.OtherOwnersLeaderDirections[pos] = direction;
                                                }
                                            }
                                            else if (direction != LeaderDirection.Invalid)
                                            {
                                                CurrentPrefSet.OtherOwnersLeaderDirections.Add(pos, direction);
                                            }
                                        }
                                    }
                                }
                                Preview.Clear();
                                break;
                            case 'P':
                                if (!clickedplane)
                                {
                                    PreviewLocation = (PointF)clicked;
                                    Preview.Clear();
                                }
                                break;
                            case 'Q':
                                if ((keys[0].Length >= 4 || keys[0].Length <= 6) && enter)
                                {
                                    var qlstring = KeysToString(keys[0]).Substring(1);
                                    bool qlplus = false;
                                    if (!string.IsNullOrEmpty(qlstring))
                                        qlplus = qlstring.Last() == '+';
                                    string qlpos = qlstring;

                                    if (string.IsNullOrEmpty(qlpos))
                                    {
                                        DisplayPreviewMessage("ILL POS", 10);
                                    }
                                    else if (qlplus)
                                    {
                                        qlpos = qlstring.Substring(0, qlstring.Length - 1);
                                        if (QuickLookList.Contains(qlpos))
                                            QuickLookList.Remove(qlpos);
                                        if (QuickLookList.Contains(qlpos + "+"))
                                            QuickLookList.Remove(qlpos + "+");
                                        else
                                            QuickLookList.Add(qlpos + "+");
                                    }
                                    else
                                    {
                                        if (QuickLookList.Contains(qlpos))
                                            QuickLookList.Remove(qlpos);
                                        else if (QuickLookList.Contains(qlpos + "+"))
                                            QuickLookList.Remove(qlpos + "+");
                                        else
                                            QuickLookList.Add(qlpos);
                                    }
                                    Preview.Clear();
                                }
                                break;
                            case 'S':
                                if (!clickedplane && keys[0].Length == 2)
                                {
                                    StatusLocation = (PointF)clicked;
                                    Preview.Clear();
                                }
                                else if (!clickedplane && keys[0].Length >= 3 && keys[0][2].GetType() == typeof(char))
                                {
                                    char textchar = (char)keys[0][2];
                                    if (char.IsLetter(textchar))
                                    {
                                        atises[0] = textchar;
                                        if (keys[0].Length > 3)
                                        {
                                            string text = "";
                                            for (int i = 3; i < keys[0].Length; i++)
                                            {
                                                if (keys[0][i].GetType() == typeof(char))
                                                {
                                                    text += (char)keys[0][i];
                                                }
                                            }
                                            if (keys.Length > 1)
                                            {
                                                for (int i = 1; i < keys.Length; i++)
                                                {
                                                    text += " ";
                                                    for (int j = 0; j < keys[i].Length; j++)
                                                    {
                                                        if (keys[i][j].GetType() == typeof(char))
                                                        {
                                                            text += (char)keys[i][j];
                                                        }
                                                    }
                                                }
                                            }
                                            gentexts[0] = text;
                                        }
                                        Preview.Clear();
                                    }
                                }
                                break;
                            case 'O': //Multifunction O: Auto Offset
                                if (keys[0].Length == 3 && enter)
                                {
                                    if ((char)keys[0][2] == 'I') //Inhibit
                                        AutoOffset = false;
                                    else if ((char)keys[0][2] == 'E') //Enable
                                        AutoOffset = true;
                                    else
                                        break;
                                    Preview.Clear();
                                }
                                break;
                            case 'R':
                                if (clickedplane)
                                {
                                    var plane = clicked as Aircraft;
                                    plane.ShowPTL = !plane.ShowPTL;
                                    Preview.Clear();
                                }
                                break;
                            case 'Y':
                                if (clickedplane && keys.Length == 1)
                                {
                                    var plane = clicked as Aircraft;
                                    if (keys[0].Length == 2)
                                    {
                                        if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                                        {
                                            DisplayPreviewMessage("ILL TRK");
                                        }
                                        else
                                        {
                                            plane.Scratchpad = "";
                                            Preview.Clear();
                                            plane.SendUpdate();
                                        }
                                    }
                                    else if (keys[0].Length == 3 && (char)keys[0][2] == '+')
                                    {
                                        if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                                        {
                                            DisplayPreviewMessage("ILL TRK");
                                        }
                                        else
                                        {
                                            plane.Scratchpad2 = "";
                                            Preview.Clear();
                                            plane.SendUpdate();
                                        }
                                    }
                                    else if (keys[0].Length >= 3 && keys[0].Length <= 6 && (char)keys[0][2] != '+')
                                    {
                                        if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                                        {
                                            DisplayPreviewMessage("ILL TRK");
                                        }
                                        else
                                        {
                                            plane.Scratchpad = KeysToString(keys[0], 2);
                                            Preview.Clear();
                                            plane.SendUpdate();
                                        }
                                    }
                                    else if (keys[0].Length >= 4 && keys[0].Length <= 7 && (char)keys[0][2] == '+')
                                    {
                                        if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                                        {
                                            DisplayPreviewMessage("ILL TRK");
                                        }
                                        else
                                        {
                                            plane.Scratchpad2 = KeysToString(keys[0], 3);
                                            Preview.Clear();
                                            plane.SendUpdate();
                                        }
                                    }
                                    else
                                    {
                                        DisplayPreviewMessage("FORMAT");
                                    }
                                }
                                else if (!clickedplane && keys.Length == 2)
                                {
                                    var planestring = KeysToString(keys[0], 2);
                                    var planes = Aircraft.Where(x =>
                                    {
                                        if (x.FlightPlanCallsign != null && x.FlightPlanCallsign.Trim() == planestring)
                                        {
                                            return true;
                                        }
                                        if (x.AssignedSquawk != null && x.AssignedSquawk.Trim() == planestring)
                                        {
                                            return true;
                                        }
                                        return false;
                                    });
                                    if (planes.Count() != 1)
                                    {
                                        DisplayPreviewMessage("NO FLIGHT");
                                    }
                                    else
                                    {
                                        var plane = planes.First();
                                        if ((char)keys[1][0] == '+' && keys[1].Length >= 2 && keys[1].Length <= 5)
                                        {
                                            plane.Scratchpad2 = KeysToString(keys[1], 1);
                                            Preview.Clear();
                                            plane.SendUpdate();
                                        }
                                        else if ((char)keys[1][0] != '+' && keys[1].Length >= 1 && keys[1].Length <= 4)
                                        {
                                            plane.Scratchpad = KeysToString(keys[1]);
                                            Preview.Clear();
                                            plane.SendUpdate();
                                        }
                                        else
                                        {
                                            DisplayPreviewMessage("FORMAT");
                                        }
                                    }
                                }
                                else
                                {
                                    DisplayPreviewMessage("FORMAT");
                                }
                                break;
                        }
                        break;
                    case KeyCode.RngRing:
                        //Range Rings
                        if (keys[0].Length == 1)
                        {
                        }
                        else if (enter)
                        {
                            if (double.TryParse(KeysToString(Preview.ToArray()), out double interval))
                            {
                                CurrentPrefSet.RangeRingSpacing = (int)interval;
                            }
                        }
                        Preview.Clear();
                        break;
                    case KeyCode.WX:
                        if (keys[0].Length == 2 && keys[0][1].GetType() == typeof(char) && enter)
                        {
                            string wxlevelstring = ((char)keys[0][1]).ToString();
                            if (int.TryParse(wxlevelstring, out int level))
                            {
                                if (level > 0 && level < 7)
                                {
                                    level--;
                                    Nexrad.LevelsEnabled[level] = !Nexrad.LevelsEnabled[level];
                                    Nexrad.RecomputeVertices();
                                }
                            }
                            Preview.Clear();
                        }
                        break;
                    case KeyCode.RecenterEverything:
                        if (keys.Length == 2)
                        {
                            var airportcode = KeysToString(keys[1]);
                            var airports = Airports.Where(x => x.ID == airportcode);
                            if (airports.Count() == 1)
                            {
                                var airport = airports.First();
                                GeoPoint loc = new GeoPoint(airport.Location.Latitude, airport.Location.Longitude);
                                CurrentPrefSet.ScopeCentered = true;
                                HomeLocation = loc;
                                CurrentPrefSet.RangeRingLocation = loc;
                                ScreenRotation = (double)airport.MagVar;
                                Preview.Clear();
                            }
                            else
                            {
                                DisplayPreviewMessage("NO AIRPORT");
                            }
                        }
                        break;
                    case Key.End:
                        //Min Sep
                        tempLine = null;
                        if (clickedplane)
                        {
                            var plane = clicked as Aircraft;
                            if (tempMinSep == null)
                            {
                                tempMinSep = new MinSep(plane, null);
                            }
                            else
                            {
                                var minsep = new MinSep(tempMinSep.Plane1, plane);
                                tempMinSep = null;
                                lock (minSeps)
                                    minSeps.Add(minsep);
                                Preview.Clear();
                            }
                        }
                        else if (enter)
                        {
                            lock (minSeps)
                                minSeps.Clear();
                            Preview.Clear();
                            tempMinSep = null;
                        }
                        break;
                    default:
                        if (tempLine != null && enter)
                        {
                            if (clickedplane)
                            {
                                tempLine.EndPlane = clicked as Aircraft;
                                tempLine = null;
                                Preview.Clear();
                            }
                            else
                            {
                                var entered = KeysToString(keys[0]);
                                var waypoint = Waypoints.Find(x => x.ID == entered);
                                if (waypoint != null)
                                {
                                    tempLine.EndGeo = waypoint.Location;
                                    tempLine = null;
                                    Preview.Clear();
                                }
                            }
                        }
                        if (keys[0].Length == 3 && clickedplane)
                        {
                            var plane = clicked as Aircraft;
                            if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                            {
                                DisplayPreviewMessage("ILL TRK");
                            }
                            else
                            {
                                plane.Scratchpad = KeysToString(keys[0]);
                                plane.SendUpdate();
                                Preview.Clear();
                            }
                        }
                        else if (keys[0].Length == 4 && clickedplane && KeysToString(keys[0]).Last() == '+')
                        {
                            var plane = clicked as Aircraft;
                            if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                            {
                                DisplayPreviewMessage("ILL TRK");
                            }
                            else
                            {
                                plane.Scratchpad2 = KeysToString(keys[0]).Substring(0, 3);
                                plane.SendUpdate();
                                Preview.Clear();
                            }
                        }
                        else if (keys[0].Length == 4 && clickedplane)
                        {
                            var plane = clicked as Aircraft;
                            if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                            {
                                DisplayPreviewMessage("ILL TRK");
                            }
                            else
                            {
                                plane.Type = KeysToString(keys[0]);
                                plane.SendUpdate();
                                Preview.Clear();
                            }
                        }
                        else if (keys[0].Length == 2 && clickedplane)
                        {
                            var plane = clicked as Aircraft;
                            if (!string.IsNullOrEmpty(plane.PendingHandoff) || plane.PositionInd != ThisPositionIndicator)
                            {
                                DisplayPreviewMessage("ILL TRK");
                            }
                            else
                            {
                                plane.PendingHandoff = KeysToString(keys[0]);
                                plane.SendUpdate();
                                Preview.Clear();
                            }
                        }
                        break;
                }
            }
        }

        private void ProcessImpliedCommand(object clicked = null)
        {
            /*
                Aircraft plane = (Aircraft)clicked;
                if (plane.Pointout)
                    plane.Pointout = false;
                else if (!plane.Owned)
                    plane.FDB = plane.FDB ? false : true;
                else if (plane.PositionInd != ThisPositionIndicator)
                {
                    plane.Owned = false;
                }
                GenerateDataBlock(plane);
             */
            bool clickedplane = false;
            bool enter = false;
            if (clicked != null)
                clickedplane = clicked.GetType() == typeof(Aircraft);
            else
                enter = true;
            if (clickedplane)
            {
                var plane = clicked as Aircraft;
                // Accept Handoff, Recall handoff
                if (plane.PendingHandoff == ThisPositionIndicator)
                {
                    plane.PositionInd = ThisPositionIndicator;
                    plane.PendingHandoff = null;
                    plane.SendUpdate();
                }
                else if (plane.PositionInd == ThisPositionIndicator && !string.IsNullOrEmpty(plane.PendingHandoff))
                {
                    plane.PendingHandoff = null;
                    plane.SendUpdate();
                }
                // Accept pointout, Recall pointout, Clear pointout color, Clear/reject / cancel pointout indication
                else if (plane.Pointout)
                {
                    plane.Pointout = false;
                }
                else if (plane.ForceQuickLook)
                {
                    plane.ForceQuickLook = false;
                }
                // acknowledge CA / MSAW / SPC / FMA track
                // stop blinking Cancelled flight plan indicator
                // Change ABC to RBC for track in mismatch
                // Display suspended track's flight plan in Preview area
                // Inhibit Duplicate beacon code indicator
                // Remove ADS-B duplicate target address indicator
                // Remove blinking Resume flight progress indicator
                // Remove blinking indicators and/or inhibit blinking Full data block
                // Inhibit blinking data block at former local owner's TCW/TDW
                // Remove ACID/Target ID mismatch indicator
                // Remove ADS-B data loss indicator
                // Return data block to unowned color
                else if (plane.Owned && plane.PositionInd != ThisPositionIndicator)
                {
                    plane.Owned = false;
                }
                // Take control of interfacility track
                // Remove frozen flight from display
                // Acknowledge Time Based Flow Management (TBFM) runway mismatch
                // Beacon readout - owned and associated track
                else if (plane.Owned && !string.IsNullOrEmpty(plane.FlightPlanCallsign))
                {
                    DisplayPreviewMessage(string.Format("{0} {1} {2}", plane.FlightPlanCallsign, plane.Squawk, plane.AssignedSquawk));
                }
                // Toggle quick look for a single track
                else if (!plane.Owned && !string.IsNullOrEmpty(plane.FlightPlanCallsign))
                {
                    plane.FDB = !plane.FDB;
                }
                // Create FP and associate to LDB with blinking ACID or frozen SPC
                // Inibit No flight plan alert for unassociated track
                // Beacon Readout - unassociated track
                else if (string.IsNullOrEmpty(plane.FlightPlanCallsign))
                {
                    plane.FDB = !plane.FDB;
                }
                //GenerateDataBlock(plane);
            }
        }
        private string KeysToString(object[] keys, int start = 0)
        {
            string output = "";
            for (int i = start; i < keys.Length; i++)
            {
                var key = keys[i];
                var type = key.GetType();
                if (type == typeof(KeyCode) || type == typeof(Key))
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
                        case (int)Key.Plus:
                        case (int)Key.KeypadPlus:
                            output += "+";
                            break;
                    }
                }
                else if (key.GetType() == typeof(char))
                {
                    output += key;
                }
            }
            return output;
        }

        private void RenderPreview()
        {
            var oldtext = PreviewArea.Text;
            if (previewmessage != null && previewmessageexpiry <= CurrentTime)
                previewmessage = null;
            else if (previewmessage != null && Preview.Count == 0)
                PreviewArea.Text = previewmessage;
            else
                PreviewArea.Text = GeneratePreviewString(Preview);
            PreviewArea.ForceRedraw();
            PreviewArea.ForeColor = AdjustedColor(DataBlockColor, CurrentPrefSet.Brightness.FullDataBlocks);
            PreviewArea.LocationF = new PointF(PreviewLocation.X, PreviewLocation.Y - PreviewArea.SizeF.Height);
            DrawLabel(PreviewArea);
        }
        string previewmessage = null;
        DateTime previewmessageexpiry;

        private void DisplayPreviewMessage(string message, double seconds = 5)
        {
            Preview.Clear();
            previewmessage = message;
            previewmessageexpiry = CurrentTime.AddSeconds(seconds);
        }
        private void RenderStatus()
        {
            StatusArea.ForeColor = AdjustedColor(DataBlockColor, CurrentPrefSet.Brightness.Lists);
            StatusArea.Font = Font;
            var oldtext = StatusArea.Text;
            var timesyncind = timesync.Synchronized ? " " : "*";
            StatusArea.Text = CurrentTime.ToString("HHmm/ss") + timesyncind + wx.Altimeter.Value.ToString("00.00") + "\r\n";
            for (int i = 0; i < 10; i++)
            {
                if (atises[i] != null)
                {
                    StatusArea.Text += atises[i] + " ";
                    if (gentexts[i] != null)
                        StatusArea.Text += gentexts[i];
                    StatusArea.Text += "\r\n";
                }
            }
            if (SelectedBeaconCodes.Count > 0)
            {

                foreach (var squawk in SelectedBeaconCodes)
                {
                    StatusArea.Text += squawk + " ";
                }
                StatusArea.Text += "\r\n";
            }
            StatusArea.Text += (int)CurrentPrefSet.Range + "NM" + " PTL: " + CurrentPrefSet.PTLLength.ToString("0.0") + "\r\n";
            StatusArea.Text += ToFilterAltitudeString(MinAltitude) + " " + ToFilterAltitudeString(MaxAltitude) + " U "
                + ToFilterAltitudeString(MinAltitudeAssociated) + " " + ToFilterAltitudeString(MaxAltitudeAssociated) + " A\r\n";
            if (ATPA.Active)
            {
                StatusArea.Text += "INTRAIL ON: ";
                ATPA.Volumes.ForEach(x =>
                {
                    if (x.Active)
                    {
                        StatusArea.Text += x.VolumeId + " ";
                    }
                });
                StatusArea.Text += "\r\n";
                var tpfv = ATPA.Volumes.Where(v => v.TwoPointFiveEnabled && v.TwoPointFiveActive && v.Active);
                if (tpfv.Count() > 0)
                {
                    StatusArea.Text += "INTRAIL 2.5 ON: ";
                    tpfv.ToList().ForEach(v => StatusArea.Text += v.VolumeId + " ");
                    StatusArea.Text += "\r\n";
                }
            }
            int metarnum = 0;
            bool crlast = false;
            foreach (var metar in wx.Metars.OrderBy(x => x.ICAO))
            {
                metarnum++;
                if (metar.IsValid)
                {
                    try
                    {
                        string station = metar.ICAO;
                        if (station.Length == 4 && station[0] == 'K') //not really correct, but whatever
                            station = station.Substring(1);
                        if (metar.Pressure != null)
                        {
                            StatusArea.Text += station;
                            StatusArea.Text += " ";
                            StatusArea.Text += metar.Pressure.GetConvertedValue(csharp_metar_decoder.entity.Value.Unit.MercuryInch).ToString("00.00");

                        }
                        else
                        {
                            StatusArea.Text += station + " 00.00";
                        }
                        //if (WindInStatusArea)
                        //    StatusArea.Text += " " + metar.Wind.Raw;
                    }
                    catch
                    {
                        StatusArea.Text += metar.ICAO + " METAR ERR";
                    }
                    if (WindInStatusArea || metarnum % 3 == 0)
                    {
                        StatusArea.Text += "\r\n";
                        crlast = true;
                    }
                    else
                    {
                        StatusArea.Text += " ";
                        crlast = false;
                    }
                }
            }
            if (!crlast)
            {
                StatusArea.Text += "\r\n";
            }
            if (FPSInStatusArea)
            {
                StatusArea.Text += $"FPS: {fps} AC: {Aircraft.Count}";
                StatusArea.Text += "\r\n";
            }
            if (QuickLookList.Count > 0)
            {
                StatusArea.Text += "QL: ";
                foreach (string quicklook in QuickLookList)
                {
                    StatusArea.Text += $"{quicklook} ";
                }
                StatusArea.Text += "\r\n";
            }
            StatusArea.LocationF = new PointF(StatusLocation.X, StatusLocation.Y - StatusArea.SizeF.Height);
            DrawLabel(StatusArea);
        }

        private string ToFilterAltitudeString(int altitude)
        {
            int hundreds = Math.Abs(altitude / 100);
            string altString = hundreds.ToString("000");
            if (altitude < 0)
                altString = "N99";
            return altString;
        }
        private string GeneratePreviewString(List<object> keys)
        {
            string output = "";
            foreach (var key in keys)
            {
                var type = key.GetType();
                if (type == typeof(KeyCode) || type == typeof(Key))
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
                        case (int)KeyCode.RngRing:
                            output += "RR";
                            break;
                        case (int)KeyCode.WX:
                            output += "WX";
                            break;
                        case (int)KeyCode.RecenterEverything:
                            output += "RECENTER";
                            break;
                        default:
                            break;
                    }
                }
                else if (type == typeof(char))
                {
                    switch ((char)key)
                    {
                        case ' ':
                            output += "\r\n";
                            break;
                        case '`':
                            output += "▲";
                            break;
                        default:
                            output += key;
                            break;
                    }
                }
            }
            output += " ";
            return output;
        }
        private void Window_KeyPress(object sender, KeyPressEventArgs e)
        {
            char key = char.ToUpper(e.KeyChar);
            Preview.Add(key);
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
                        StopReceivers();
                        Environment.Exit(0);
                        break;
                    case Key.S:
                        if (e.Shift)
                        {
                            using (System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog())
                            {
                                dialog.RestoreDirectory = true;
                                dialog.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                                dialog.FilterIndex = 1;
                                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    settingsPath = dialog.FileName;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        Preview.Clear();
                        SaveSettings(settingsPath);
                        break;
                    case Key.P:
                        PropertyForm properties = new PropertyForm(this);
                        properties.Show();
                        break;
                    case Key.Q:
                        QuickLook = !QuickLook;
                        break;
                    case Key.F1:
                        if (!e.Shift)
                        {
                            if (CurrentPrefSet.ScopeCentered)
                            {
                                CurrentPrefSet.ScopeCentered = false;
                            }
                            else
                            {
                                CurrentPrefSet.ScopeCentered = true;
                            }
                        }
                        else
                        {
                            Preview.Add(KeyCode.RecenterEverything);
                            Preview.Add(' ');
                        }
                        break;
                    case Key.F2:
                        VideoMapSelector selector = new VideoMapSelector(VideoMaps);
                        selector.Show();
                        selector.BringToFront();
                        selector.Focus();
                        break;
                    case Key.F8:
                        CurrentPrefSet.DCBVisible = !CurrentPrefSet.DCBVisible;
                        break;
                    case Key.F9:
                        Preview.Clear();
                        Preview.Add(KeyCode.RngRing);
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
                            StopReceivers();
                            Environment.Exit(0);
                        }
                        else
                            window.WindowState = window.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
                        break;
                    case Key.F4:
                        Preview.Clear();
                        StopReceivers();
                        SaveSettings(settingsPath);
                        Environment.Exit(0);
                        break;

                }
            }
            else
            {
                switch (e.Key)
                {
                    case Key.F13:
                    case Key.F14:
                    case Key.F15:
                    case Key.F16:
                    case Key.F17:
                        Preview.Clear();
                        Preview.Add(KeyCode.WX);
                        break;
                    case Key.F18:
                    case Key.F19:
                    case Key.F20:
                    case Key.F21:
                    case Key.F22:
                    case Key.F23:
                    case Key.F24:
                    case Key.LShift:
                    case Key.RShift:
                        break;
                    case Key.Escape:
                        Preview.Clear();
                        previewmessage = null;
                        ReleaseDCBButton();
                        centeredmouse = true;
                        window.CursorVisible = true;
                        if (tempLine != null)
                        {
                            lock (rangeBearingLines)
                                rangeBearingLines.Remove(tempLine);
                            tempLine = null;
                        }
                        if (tempMinSep != null)
                            tempMinSep = null;
                        break;
                    case Key.Enter:
                    case Key.KeypadEnter:
                    case Key.PageDown:
                        ProcessCommand(Preview);
                        break;
                    case Key.BackSpace:
                        if (Preview.Count > 0)
                            Preview.RemoveAt(Preview.Count - 1);
                        break;
                    case Key.F1:
                        if (!showAllCallsigns)
                        {
                            showAllCallsigns = true;
                        }
                        break;
                    default:
                        if (((int)e.Key > 9 && (int)e.Key < 22) || e.Key == Key.End)
                            Preview.Clear();
                        bool isText = (e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.Number0 && e.Key <= Key.Number9) || (e.Key >= Key.Keypad0 && e.Key <= Key.Keypad9) || e.Key == Key.Period || e.Key == Key.KeypadPeriod
            || e.Key == Key.Slash || e.Key == Key.Quote || e.Key == Key.Plus || e.Key == Key.BracketLeft || e.Key == Key.BracketRight || e.Key == Key.Minus || e.Key == Key.KeypadMultiply || e.Key == Key.KeypadPlus || e.Key == Key.Space || e.Key == Key.Grave;
                        if (!isText)
                            Preview.Add(e.Key);
                        break;

                }
            }

        }
        private void Window_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    if (showAllCallsigns)
                    {
                        showAllCallsigns = false;
                    }
                    break;
            }
        }

        private void Window_Resize(object sender, EventArgs e)
        {

            var oldscale = scale;
            GL.Viewport(0, 0, window.Width, window.Height);
        }

        private void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
        }

        private int fps = 0;
        private DCB dcb = new DCB();
        private DCBMenu dcbMainMenu = new DCBMenu();
        private DCBButton activeDcbButton;
        private DCBAdjustmentButton dcbRangeButton = new DCBAdjustmentButton() { Height = 80, Width = 80 };
        private DCBAdjustmentButton dcbPlaceCntrButton = new DCBAdjustmentButton() { Height = 40, Width = 80, Text = "PLACE\r\nCNTR" };
        private DCBToggleButton dcbOffCntrButton = new DCBToggleButton() { Height = 40, Width = 80, Text = "OFF\r\nCNTR" };
        private DCBAdjustmentButton dcbRRButton = new DCBAdjustmentButton() { Height = 80, Width = 80 };
        private DCBActionButton dcbPlaceRRButton = new DCBActionButton() { Height = 40, Width = 80, Text = "PLACE\r\nRR" };
        private DCBButton dcbRRCntrButton = new DCBButton() { Height = 40, Width = 80, Text = "RR\r\nCNTR" };
        private DCBSubmenuButton dcbMapsButton = new DCBSubmenuButton() { Height = 80, Width = 80, Text = "MAPS" };
        private DCBMenu dcbMapsMenu = new DCBMenu();
        private DCBButton dcbMapsSubmenuDoneButton = new DCBButton() { Height = 40, Width = 80, Text = "DONE" };
        private DCBButton dcbClearAllMapsButton = new DCBButton() { Height = 40, Width = 80, Text = "CLR ALL" };
        private DCBToggleButton[] dcbMapButton = new DCBToggleButton[32];
        private DCBToggleButton[] dcbWxButton = new DCBToggleButton[6];
        private DCBSubmenuButton dcbBriteButton = new DCBSubmenuButton() { Height = 80, Width = 80, Text = "BRITE" };
        private DCBAdjustmentButton dcbLdrDirButton = new DCBAdjustmentButton() { Height = 40, Width = 80 };
        private DCBAdjustmentButton dcbLdrLenButton = new DCBAdjustmentButton() { Height = 40, Width = 80 };
        private DCBSubmenuButton dcbCharSizeButton = new DCBSubmenuButton() { Height = 80, Width = 80, Text = "CHAR\r\nSIZE", Disabled = true };
        private DCBButton dcbModeButton = new DCBButton() { Height = 80, Width = 80, Text = "MODE\r\nFSL", Disabled = true };
        private DCBSubmenuButton dcbSiteButton = new DCBSubmenuButton() { Height = 80, Width = 80 };


        private DCBButton dcbShiftButton = new DCBButton() { Height = 80, Width = 80, Text = "SHIFT" };
        private DCBButton dcbShiftButton2 = new DCBButton() { Height = 80, Width = 80, Text = "SHIFT" };
        
        private DCBMenu dcbShiftMenu = new DCBMenu();
        private DCBButton dcbVolumeButton = new DCBButton() { Height = 80, Width = 80, Text = "VOL\r\nN/A", Disabled = true };
        private DCBAdjustmentButton dcbHistoryNumButton = new DCBAdjustmentButton() { Height = 40, Width = 80 };
        private DCBAdjustmentButton dcbHistoryRateButton = new DCBAdjustmentButton() { Height = 40, Width = 80 };
        private DCBButton dcbCursorHomeButton = new DCBButton() { Height = 80, Width = 80, Text = "CURSOR\r\nHOME", Disabled = true };
        private DCBAdjustmentButton dcbCursorSpeedButton = new DCBAdjustmentButton() { Height = 80, Width = 80, Text = "CSR SPD\r\nN/A", Disabled = true };
        private DCBButton dcbMapUncorButton = new DCBButton() { Height = 80, Width = 80, Text = "MAP\r\nUNCOR", Disabled = true };
        private DCBButton dcbUncorButton = new DCBButton() { Height = 80, Width = 80, Text = "UNCOR", Disabled = true };
        private DCBButton dcbBeaconModeButton = new DCBButton() { Height = 80, Width = 80, Text = "BEACON\r\nMODE-2", Disabled = true };
        private DCBButton dcbRtqcButton = new DCBButton() { Height = 80, Width = 80, Text = "RTQC", Disabled = true };
        private DCBButton dcbMcpButton = new DCBButton() { Height = 80, Width = 80, Text = "MCP", Disabled = true };
        private DCBButton dcbDcbTopButton = new DCBButton() { Height = 40, Width = 80, Text = "DCB\r\nTOP" };
        private DCBButton dcbDcbLeftButton = new DCBButton() { Height = 40, Width = 80, Text = "DCB\r\nLEFT" };
        private DCBButton dcbDcbRightButton = new DCBButton() { Height = 40, Width = 80, Text = "DCB\r\nRIGHT" };
        private DCBButton dcbDcbBottomButton = new DCBButton() { Height = 40, Width = 80, Text = "DCB\r\nBOTTOM" };
        private DCBAdjustmentButton dcbPtlLengthButton = new DCBAdjustmentButton() { Height = 80, Width = 80 };
        private DCBToggleButton dcbPtlOwnButton = new DCBToggleButton { Height = 40, Width = 80, Text = "PTL OWN" };
        private DCBToggleButton dcbPtlAllButton = new DCBToggleButton { Height = 40, Width = 80, Text = "PTL ALL" };

        private DCBMenu briteMenu = new DCBMenu();
        private DCBAdjustmentButton briteDCBbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteBKCbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteMPAbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteMPBbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteFDBbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteLSTbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton britePOSbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteLDBbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteOTHbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteTLSbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteRRbutton  = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteCMPbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteBCNbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton britePRIbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteHSTbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteWXbutton  = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBAdjustmentButton briteWXCbutton = new DCBAdjustmentButton() { Width = 80, Height = 40 };
        private DCBButton briteDoneButton = new DCBButton { Width = 80, Height = 80, Text = "DONE" };

        private DCBMenu siteMenu = new DCBMenu();

        public TCP TCP { get; set; } = new TCP();

        private void SetupDCB()
        {
            dcbMainMenu.AddButton(dcbRangeButton);
            dcbRangeButton.Click += DcbButtonClick;
            dcbMainMenu.AddButton(dcbPlaceCntrButton);
            dcbPlaceCntrButton.Click += DcbButtonClick;
            dcbMainMenu.AddButton(dcbOffCntrButton);
            dcbOffCntrButton.Click += DcbButtonClick;
            dcbMainMenu.AddButton(dcbRRButton);
            dcbRRButton.Click += DcbButtonClick;
            dcbMainMenu.AddButton(dcbPlaceRRButton);
            dcbPlaceRRButton.Click += DcbScopeActionButtonClick;
            dcbMainMenu.AddButton(dcbRRCntrButton);
            dcbRRCntrButton.Click += DcbButtonClick;
            
            dcbMainMenu.AddButton(dcbMapsButton);
            dcbMapsButton.Submenu = dcbMapsMenu;
            dcbMapsButton.Click += DcbSubmenuButtonClick;
            for (int i = 0; i < 6; i++)
            {
                dcbMapButton[i] = new DCBToggleButton { Height = 40, Width = 80, Text = "MAP\r\n" + (i + 1) };
                dcbMainMenu.AddButton(dcbMapButton[i]);
                dcbMapButton[i].Click += DcbMapButtonClick;
            }
            dcbMapsMenu.AddButton(dcbMapsSubmenuDoneButton);
            dcbMapsMenu.AddButton(dcbClearAllMapsButton);
            dcbMapsSubmenuDoneButton.Click += DcbButtonClick;
            dcbClearAllMapsButton.Click += DcbClearAllMapsButton_Click;
            for (int i = 6; i < dcbMapButton.Length; i++)
            {
                dcbMapButton[i] = new DCBToggleButton { Height = 40, Width = 80, Text = "MAP\r\n" + (i + 1) };
                dcbMapsMenu.AddButton(dcbMapButton[i]);
                dcbMapButton[i].Click += DcbMapButtonClick;
            }
            for (int i = 0; i < dcbWxButton.Length; i++)
            {
                dcbWxButton[i] = new DCBToggleButton { Height = 80, Width = 40, RotateIfVertical = true, Text = "WX" + (i + 1) };
                dcbMainMenu.AddButton(dcbWxButton[i]);
                dcbWxButton[i].Click += DcbWxButtonClick;
            }
            dcbMainMenu.AddButton(dcbBriteButton);
            dcbBriteButton.Click += DcbSubmenuButtonClick;
            dcbBriteButton.Submenu = briteMenu;
            dcbMainMenu.AddButton(dcbLdrDirButton);
            dcbLdrDirButton.Click += DcbButtonClick;
            dcbLdrDirButton.Up += DcbLdrDirButton_Up;
            dcbLdrDirButton.Down += DcbLdrDirButton_Down;
            dcbMainMenu.AddButton(dcbLdrLenButton);
            dcbLdrLenButton.Click += DcbButtonClick;
            dcbMainMenu.AddButton(dcbCharSizeButton);
            dcbMainMenu.AddButton(dcbModeButton);
            dcbMainMenu.AddButton(dcbSiteButton);
            dcbSiteButton.Submenu = siteMenu;
            dcbSiteButton.Click += DcbSubmenuButtonClick;
            dcbMainMenu.AddButton(dcbShiftButton);
            dcbShiftButton.Click += DcbButtonClick;

            //Auxiliary DCB Menu
            dcbShiftMenu.AddButton(dcbVolumeButton);
            dcbShiftMenu.AddButton(dcbHistoryNumButton);
            dcbShiftMenu.AddButton(dcbHistoryRateButton);
            dcbShiftMenu.AddButton(dcbCursorHomeButton);
            dcbShiftMenu.AddButton(dcbCursorSpeedButton);
            dcbShiftMenu.AddButton(dcbMapUncorButton);
            dcbShiftMenu.AddButton(dcbUncorButton);
            dcbShiftMenu.AddButton(dcbBeaconModeButton);
            dcbShiftMenu.AddButton(dcbRtqcButton);
            dcbShiftMenu.AddButton(dcbMcpButton);
            dcbShiftMenu.AddButton(dcbDcbTopButton);
            dcbShiftMenu.AddButton(dcbDcbLeftButton);
            dcbShiftMenu.AddButton(dcbDcbRightButton);
            dcbShiftMenu.AddButton(dcbDcbBottomButton);
            dcbShiftMenu.AddButton(dcbPtlLengthButton);
            dcbShiftMenu.AddButton(dcbPtlOwnButton);
            dcbShiftMenu.AddButton(dcbPtlAllButton);
            dcbShiftMenu.AddButton(dcbShiftButton2);
            dcbHistoryNumButton.Click += DcbButtonClick;
            dcbHistoryRateButton.Click += DcbButtonClick;
            dcbDcbTopButton.Click += DcbButtonClick;
            dcbDcbLeftButton.Click += DcbButtonClick;
            dcbDcbRightButton.Click += DcbButtonClick;
            dcbDcbBottomButton.Click += DcbButtonClick;
            dcbPtlOwnButton.Click += DcbButtonClick;
            dcbPtlAllButton.Click += DcbButtonClick;
            dcbPtlLengthButton.Click += DcbButtonClick;
            dcbShiftButton2.Click += DcbButtonClick;

            briteMenu.AddButton(briteDCBbutton);
            briteMenu.AddButton(briteBKCbutton);
            briteMenu.AddButton(briteMPAbutton);
            briteMenu.AddButton(briteMPBbutton);
            briteMenu.AddButton(briteFDBbutton);
            briteMenu.AddButton(briteLSTbutton);
            briteMenu.AddButton(britePOSbutton);
            briteMenu.AddButton(briteLDBbutton);
            briteMenu.AddButton(briteOTHbutton);
            briteMenu.AddButton(briteTLSbutton);
            briteMenu.AddButton(briteRRbutton);
            briteMenu.AddButton(briteCMPbutton);
            briteMenu.AddButton(briteBCNbutton);
            briteMenu.AddButton(britePRIbutton);
            briteMenu.AddButton(briteHSTbutton);
            briteMenu.AddButton(briteWXbutton);
            briteMenu.AddButton(briteWXCbutton);
            briteMenu.AddButton(briteDoneButton);
            briteDCBbutton.Click += DcbButtonClick;
            briteBKCbutton.Click += DcbButtonClick; 
            briteMPAbutton.Click += DcbButtonClick; 
            briteMPBbutton.Click += DcbButtonClick; 
            briteFDBbutton.Click += DcbButtonClick; 
            briteLSTbutton.Click += DcbButtonClick; 
            britePOSbutton.Click += DcbButtonClick; 
            briteLDBbutton.Click += DcbButtonClick; 
            briteOTHbutton.Click += DcbButtonClick; 
            briteTLSbutton.Click += DcbButtonClick; 
            briteRRbutton. Click += DcbButtonClick; 
            briteCMPbutton.Click += DcbButtonClick; 
            briteBCNbutton.Click += DcbButtonClick; 
            britePRIbutton.Click += DcbButtonClick; 
            briteHSTbutton.Click += DcbButtonClick; 
            briteWXbutton. Click += DcbButtonClick;
            briteWXCbutton.Click += DcbButtonClick; 
            briteDoneButton.Click += DcbButtonClick;


            dcb.ActiveMenu = dcbMainMenu;
            
        }

        private void DcbLdrDirButton_Down(object sender, EventArgs e)
        {
            switch (CurrentPrefSet.OwnedDataBlockPosition)
            {
                case LeaderDirection.NW:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.W;
                    break;
                case LeaderDirection.N:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.NW;
                    break;
                case LeaderDirection.NE:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.N;
                    break;
                case LeaderDirection.E:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.NE;
                    break;
                case LeaderDirection.SE:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.E;
                    break;
                case LeaderDirection.S:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.SE;
                    break;
                case LeaderDirection.SW:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.S;
                    break;
                case LeaderDirection.W:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.SW;
                    break;
            }
        }

        private void DcbLdrDirButton_Up(object sender, EventArgs e)
        {
            switch (CurrentPrefSet.OwnedDataBlockPosition)
            {
                case LeaderDirection.NW:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.N;
                    break;
                case LeaderDirection.N:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.NE;
                    break;
                case LeaderDirection.NE:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.E;
                    break;
                case LeaderDirection.E:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.SE;
                    break;
                case LeaderDirection.SE:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.S;
                    break;
                case LeaderDirection.S:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.SW;
                    break;
                case LeaderDirection.SW:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.W;
                    break;
                case LeaderDirection.W:
                    CurrentPrefSet.OwnedDataBlockPosition = LeaderDirection.NW;
                    break;
            }
        }

        private void DcbSubmenuButtonClick(object sender, EventArgs e)
        {
            var button = sender as DCBSubmenuButton;
            button.SetScreenLocation(window.Location);
            activeDcbButton = button;


            if (sender == dcbSiteButton)
            {
                activeDcbButton = dcbSiteButton;
                siteMenu.ClearButtons();
                int i = 1;
                RadarSites.ForEach(x =>
                {
                    if (x.RadarType != RadarType.SLANT_RANGE)
                        return;
                    DCBButton siteButton = new DCBButton()
                    {
                        Height = 80,
                        Width = 80,
                        Text = x.Char + "\r\n" + x.Name,
                        Value = x,
                        Active = radar == x
                    };
                    siteMenu.AddButton(siteButton);
                    siteButton.Click += SiteButton_Click;
                });
                var multiButton = new DCBButton()
                {
                    Height = 80,
                    Width = 80,
                    Text = "MULTI",
                    Enabled = false,
                };
                siteMenu.AddButton(multiButton);
                multiButton.Click += SiteButton_Click;
                var fusedButton = new DCBButton()
                {
                    Height = 80,
                    Width = 80,
                    Text = "FUSED",
                    Value = Radar.FUSED,
                    Active = radar == Radar.FUSED
                };
                siteMenu.AddButton(fusedButton);
                fusedButton.Click += SiteButton_Click;
            }
        }

        private void DcbButtonClick(object sender, EventArgs e)
        {
            if (sender == dcbShiftButton)
            {
                dcb.ActiveMenu = dcbShiftMenu;
            }
            else if (sender == dcbShiftButton2)
            {
                dcb.ActiveMenu = dcbMainMenu;
            }
            else if (sender == dcbDcbTopButton)
            {
                CurrentPrefSet.DCBLocation = DCBLocation.Top;
            }
            else if (sender == dcbDcbLeftButton)
            {
                CurrentPrefSet.DCBLocation = DCBLocation.Left;
            }
            else if (sender == dcbDcbBottomButton)
            {
                CurrentPrefSet.DCBLocation = DCBLocation.Bottom;
            }
            else if (sender == dcbDcbRightButton)
            {
                CurrentPrefSet.DCBLocation = DCBLocation.Right;
            }
            else if (sender == dcbRRCntrButton)
            {
                CurrentPrefSet.RangeRingsCentered = !CurrentPrefSet.RangeRingsCentered;
            }
            else if (sender == dcbOffCntrButton)
            {
                if (CurrentPrefSet.ScopeCentered)
                {
                    CurrentPrefSet.ScopeCentered = false;
                }
                else
                {
                    CurrentPrefSet.ScopeCentered = true;
                }
            }
            else if (sender == dcbPtlOwnButton)
            {
                CurrentPrefSet.PTLOwn = !CurrentPrefSet.PTLOwn;
                if (CurrentPrefSet.PTLOwn)
                    CurrentPrefSet.PTLAll = false;
            }
            else if (sender == dcbPtlAllButton)
            {
                CurrentPrefSet.PTLAll = !CurrentPrefSet.PTLAll; 
                if (CurrentPrefSet.PTLAll)
                    CurrentPrefSet.PTLOwn = false;
            }
            else if (sender.GetType() == typeof(DCBAdjustmentButton))
            {
                window.CursorVisible = false;
                CenterMouse();
                activeDcbButton = sender as DCBAdjustmentButton;
            }
            else if (sender == dcbMapsSubmenuDoneButton)
            {
                dcbMapsButton.OnClick(e);
            }
            else if (sender == briteDoneButton)
            {
                dcbBriteButton.OnClick(e);
            }
        }

        private void SiteButton_Click(object sender, EventArgs e)
        {
            dcbSiteButton.Active = false;
            var button = sender as DCBButton;
            //button.ParentMenu.Enabled = true;
            if (button == null) { return; }
            var site = button.Value as Radar;
            if (site == null) { return; }
            radar = site;
            ReleaseDCBButton();
        }

        private void DcbClearAllMapsButton_Click(object sender, EventArgs e)
        {
            VideoMaps.ForEach(x => x.Visible = false);
        }

        private void DcbMapsSubmenuDoneButton_Click(object sender, EventArgs e)
        {
        }

        private void DcbScopeActionButtonClick(object sender, EventArgs e)
        {
            activeDcbButton = sender as DCBButton;
            if (activeDcbButton.GetType() == typeof(DCBActionButton))
                activeDcbButton.Active = true;
            
        }

        private void DcbMapButtonClick(object sender, EventArgs e)
        {
            for (int i = 0; i < dcbMapButton.Length; i++)
            {
                if (VideoMaps.Count <= i)
                    break;
                if (sender == dcbMapButton[i])
                {
                    var map = VideoMaps.Where(x => x.Number == TCP.DCBMapList[i]).FirstOrDefault();
                    if (map == null)
                        return;
                    map.Visible = !map.Visible;
                }
            }
        }

        private void DcbWxButtonClick(object sender, EventArgs e)
        {
            for (int i = 0; i < dcbWxButton.Length; i++)
            {
                if (sender == dcbWxButton[i])
                {
                    Nexrad.LevelsEnabled[i] = !Nexrad.LevelsEnabled[i];
                }
            }
            Nexrad.RecomputeVertices();
        }
        private void ReleaseDCBButton()
        {
            //if (activeDcbButton.GetType() != typeof(DCBAdjustmentButton))
            //    activeDcbButton.Active = false;
            if (activeDcbButton == null)
                return;
            if (activeDcbButton.GetType() == typeof(DCBActionButton))
                ((DCBActionButton)activeDcbButton).ActionDone();
            activeDcbButton.Active = false;
            if (activeDcbButton.ParentMenu != null)
            {
                activeDcbButton.ParentMenu.Enabled = true;
                activeDcbButton = activeDcbButton.ParentMenu.SubmenuButton;
            }
            else
            {
                activeDcbButton = null;
            }
            window.CursorVisible = true;
            System.Windows.Forms.Cursor.Clip = new Rectangle();
        }
        private void UpdateDCB()
        {
            dcb.Location = CurrentPrefSet.DCBLocation;
            dcb.Visible = CurrentPrefSet.DCBVisible;
            dcbRangeButton.Text = "RANGE\r\n" + CurrentPrefSet.Range;
            dcbOffCntrButton.Active = ScreenCenterPoint != HomeLocation;
            dcbRRButton.Text = "RR\r\n" + (int)CurrentPrefSet.RangeRingSpacing;
            dcbRRCntrButton.Active = RangeRingCenter == HomeLocation;
            for (int i = 0; i < dcbMapButton.Length; i++)
            {
                if (VideoMaps.Count <= i)
                    break;
                var map = VideoMaps.Where(x => x.Number == TCP.DCBMapList[i]).FirstOrDefault();
                if (map == null)
                    continue;
                dcbMapButton[i].Text = map.Number + "\r\n" + map.Mnemonic;
                dcbMapButton[i].Active = map.Visible;
            }
            for (int i = 0; i < dcbWxButton.Length; i++)
            {
                if (Nexrad.LevelsEnabled == null || Nexrad.LevelsEnabled.Length <= i)
                    break;
                dcbWxButton[i].Active = Nexrad.LevelsEnabled[i];
                if (Nexrad.LevelsAvailable == null || Nexrad.LevelsAvailable.Length <= i)
                    break;
                dcbWxButton[i].Text = Nexrad.LevelsAvailable[i] ? "WX" + (i + 1) + "\r\nAVL" : "WX" + (i + 1) + "\r\n ";
                dcbWxButton[i].BackColorActive = Nexrad.LevelsAvailable[i] ? Color.SlateBlue : Color.Green;
                dcbWxButton[i].BackColorInactive = Nexrad.LevelsAvailable[i] ? Color.DarkSlateBlue : Color.FromArgb(0, 80, 0);
            }
            dcbLdrDirButton.Text = "LDR DIR\r\n" + CurrentPrefSet.OwnedDataBlockPosition;
            dcbLdrLenButton.Text = "LDR LEN\r\n" + CurrentPrefSet.LeaderLength;
            dcbSiteButton.Text = "SITE\r\n" + radar.Name;
            dcbDcbTopButton.Active = dcb.Location == DCBLocation.Top;
            dcbDcbBottomButton.Active = dcb.Location == DCBLocation.Bottom;
            dcbDcbLeftButton.Active = dcb.Location == DCBLocation.Left;
            dcbDcbRightButton.Active = dcb.Location == DCBLocation.Right;
            dcbHistoryNumButton.Text = "HISTORY\r\n" + CurrentPrefSet.HistoryNum;
            dcbHistoryRateButton.Text = "H_RATE\r\n" + CurrentPrefSet.HistoryRate;
            dcbPtlLengthButton.Text = "PTL\r\nLNTH\r\n" + CurrentPrefSet.PTLLength;
            dcbPtlOwnButton.Active = CurrentPrefSet.PTLOwn;
            dcbPtlAllButton.Active = CurrentPrefSet.PTLAll;
            briteDCBbutton.Text = "DCB " + CurrentPrefSet.Brightness.DCB;
            briteBKCbutton.Text = "BKC " + CurrentPrefSet.Brightness.Background;
            briteMPAbutton.Text = "MPA " + CurrentPrefSet.Brightness.MapA;
            briteMPBbutton.Text = "MPB " + CurrentPrefSet.Brightness.MapB;
            briteFDBbutton.Text = "FDB " + CurrentPrefSet.Brightness.FullDataBlocks;
            briteLSTbutton.Text = "LST " + CurrentPrefSet.Brightness.Lists;
            britePOSbutton.Text = "POS " + CurrentPrefSet.Brightness.PositionSymbols;
            briteLDBbutton.Text = "LDB " + CurrentPrefSet.Brightness.LimitedDataBlocks;
            briteOTHbutton.Text = "OTH " + CurrentPrefSet.Brightness.OtherFDBs;
            briteTLSbutton.Text = "TLS " + CurrentPrefSet.Brightness.Tools;
            briteRRbutton .Text = "RR "  + CurrentPrefSet.Brightness.RangeRings;
            briteCMPbutton.Text = "CMP " + CurrentPrefSet.Brightness.Compass;
            briteBCNbutton.Text = "BCN " + CurrentPrefSet.Brightness.BeaconTargets;
            britePRIbutton.Text = "PRI " + CurrentPrefSet.Brightness.PrimaryTargets;
            briteHSTbutton.Text = "HST " + CurrentPrefSet.Brightness.History;
            briteWXbutton .Text = "WX "  + CurrentPrefSet.Brightness.Weather;
            briteWXCbutton.Text = "WXC " + CurrentPrefSet.Brightness.WeatherContrast;
        } 

        private Matrix4 geoToScreen;
        private Matrix4 rotscale;
        private Matrix4 arscale;
        private Matrix4 pixeltransform;
        private bool centeredlast;

        private float dataBlockOffset, dataBlockDiagonalOffset, dataBlockOffsetScale;
        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            var state = Mouse.GetState();
            var mousecurrent = new Vector4(state.X, state.Y, 0, 1);
            if (!centeredlast)
            {
                mousemove = mousecurrent - mouseprev;
                if (InvertMouse)
                {
                    mousemove *= -1;
                }
            }
            centeredlast = centeredmouse;
            centeredmouse = false;
            ProcessMouse();

            mouseprev = mousecurrent;
            DeleteTextures();
            
            if (window.WindowState == WindowState.Minimized)
                return;
            aspect_ratio = (float)window.ClientSize.Width / (float)window.ClientSize.Height;
            pixelScale = window.ClientSize.Width < window.ClientSize.Height ? 2f / window.ClientSize.Width : 2f / window.ClientSize.Height;
            Matrix4 mtrans = Matrix4.CreateTranslation(-(float) ScreenCenterPoint.Longitude, -(float) ScreenCenterPoint.Latitude, 0.0f);
            Matrix4 mscale = Matrix4.CreateScale((float)((60d / scale) * Math.Cos(MathHelper.DegreesToRadians(ScreenCenterPoint.Latitude))), (float) (60d / scale), 1.0f);
            Matrix4 mrot = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float) ScreenRotation));
            geoToScreen = mtrans * mscale * mrot;
            rotscale = (mscale * mrot).Inverted();
            pixeltransform = Matrix4.CreateTranslation(-window.ClientSize.Width / 2, -window.ClientSize.Height / 2, 0);
            pixeltransform *= Matrix4.CreateScale(2f / window.ClientSize.Width, -2f / window.ClientSize.Height, 1.0f);
            dataBlockOffsetScale = Font.Height * pixelScale;
            dataBlockOffset = (0.5f + CurrentPrefSet.LeaderLength) * dataBlockOffsetScale;
            dataBlockDiagonalOffset = (float)(dataBlockOffset * Math.Sqrt(2)) / 2;
            GL.ClearColor(AdjustedColor(BackColor, CurrentPrefSet.Brightness.Background));
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.LoadIdentity();
            GL.PushMatrix();
            if (window.ClientSize.Width < window.ClientSize.Height)
            {
                GL.Scale(1.0f, aspect_ratio, 1.0f);
                arscale = Matrix4.CreateScale(1 / aspect_ratio, 1.0f, 1.0f);
                pixeltransform *= Matrix4.CreateScale(1, 1 / aspect_ratio, 1);
            }
            else if (window.ClientSize.Width > window.ClientSize.Height)
            {
                GL.Scale(1 / aspect_ratio, 1.0f, 1.0f);
                arscale = Matrix4.CreateScale(1.0f, aspect_ratio, 1.0f);
                pixeltransform *= Matrix4.CreateScale(aspect_ratio, 1, 1);
            }
            else
            {
                arscale = Matrix4.Identity;
            }
            if (!hidewx)
            {
                DrawNexrad();
            }
            DrawRangeRings();
            DrawVideoMapLines();
            DrawStatic();
            DrawCompass();
            DrawRBLs();
            UpdateDCB();
            dcb.Draw(window.ClientSize.Width, window.ClientSize.Height, ref pixeltransform, CurrentPrefSet.Brightness.DCB);
            if (ATPA.Active)
            {
                ATPA.Calculate(Aircraft, radar);
                DrawATPAVolumes();
            }
            GenerateTargets();
            DrawTargets();
            DrawMinSeps();
            GL.PopMatrix();
            GL.Flush();
            window.SwapBuffers();
            fps = (int)(1f / e.Time);
            if (UseADSBCallsigns || UseADSBCallsignsAssociated || UseADSBCallsigns1200)
            {
                lock(Aircraft)
                    ADSBtoFlightPlanCallsigns(Aircraft.ToList());
            }
            oldar = aspect_ratio;
            
        }
        bool centeredmouse;
        Vector4 mouseprev = Vector4.Zero;
        Vector4 mousemove = Vector4.Zero;
        int mousescrollcount;
        private void ProcessMouse()
        {
            var button = activeDcbButton as DCBAdjustmentButton;
            if (button != null)
            {
                int mousethreshold = 20;
                if ((mousemove.Y >= 0) == (mousescrollcount >= 0) || (mousemove.Y <= 0) == (mousescrollcount <= 0))
                {
                    mousescrollcount += (int)mousemove.Y;
                }
                else
                {
                    mousescrollcount = 0;
                }
                if (button == dcbPlaceCntrButton)
                {
                    var centervec = new Vector4((float)ScreenCenterPoint.Longitude, (float)ScreenCenterPoint.Latitude, 0, 1);
                    var mousevec = mousemove * pixeltransform;
                    mousevec *= geoToScreen.Inverted();
                    mousevec *= 0.5f;
                    centervec += mousevec;
                    CurrentPrefSet.ScreenCenterPoint = new GeoPoint(centervec.Y, centervec.X);
                    CurrentPrefSet.ScopeCentered = false;
                }
                else if (button == dcbHistoryNumButton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;


                    var newnum = CurrentPrefSet.HistoryNum + d;
                    if (newnum > 10)
                    {
                        CurrentPrefSet.HistoryNum = 10;
                    }
                    else if (newnum < 0)
                    {
                        CurrentPrefSet.HistoryNum = 0;
                    }
                    else
                    {
                        CurrentPrefSet.HistoryNum = newnum;
                    }
                }
                else if (button == dcbHistoryRateButton)
                {
                    double d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 0.5;
                    else if (mousescrollcount < -mousethreshold)
                        d = -0.5;


                    var newnum = CurrentPrefSet.HistoryRate + d;
                    if (newnum > 4.5)
                    {
                        CurrentPrefSet.HistoryRate = 4.5;
                    }
                    else if (newnum < 0)
                    {
                        CurrentPrefSet.HistoryRate = 0;
                    }
                    else
                    {
                        CurrentPrefSet.HistoryRate = newnum;
                    }
                }
                else if (button == dcbLdrLenButton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;


                    var newnum = CurrentPrefSet.LeaderLength + d;
                    if (newnum > 8)
                    {
                        CurrentPrefSet.LeaderLength = 8;
                    }
                    else if (newnum < 0)
                    {
                        CurrentPrefSet.LeaderLength = 0;
                    }
                    else
                    {
                        CurrentPrefSet.LeaderLength = newnum;
                    }
                }
                else if (button == dcbPtlLengthButton)
                {
                    double d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 0.5;
                    else if (mousescrollcount < -mousethreshold)
                        d = -0.5;


                    var newnum = CurrentPrefSet.PTLLength + d;
                    if (newnum > 5)
                    {
                        CurrentPrefSet.PTLLength = 5;
                    }
                    else if (newnum < 0)
                    {
                        CurrentPrefSet.PTLLength = 0;
                    }
                    else
                    {
                        CurrentPrefSet.PTLLength = newnum;
                    }
                }
                else if (button == dcbRangeButton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;


                    var newnum = CurrentPrefSet.Range + d;
                    if (newnum > 512)
                    {
                        CurrentPrefSet.Range = 512;
                    }
                    else if (newnum < 6)
                    {
                        CurrentPrefSet.Range = 6;
                    }
                    else
                    {
                        CurrentPrefSet.Range = (int)newnum;
                    }
                }
                else if (button == briteDCBbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 25;
                    var newnum = CurrentPrefSet.Brightness.DCB + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.DCB = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.DCB = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.DCB = newnum;
                    }
                }
                else if (button == briteBKCbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.Background + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.Background = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.Background = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.Background = newnum;
                    }
                }
                else if (button == briteMPAbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 5;
                    var newnum = CurrentPrefSet.Brightness.MapA + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.MapA = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.MapA = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.MapA = newnum;
                    }
                }
                else if (button == briteMPBbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 5;
                    var newnum = CurrentPrefSet.Brightness.MapB + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.MapB = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.MapB = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.MapB = newnum;
                    }
                }
                else if (button == briteFDBbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.FullDataBlocks + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.FullDataBlocks = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.FullDataBlocks = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.FullDataBlocks = newnum;
                    }
                }
                else if (button == briteLSTbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 25;
                    var newnum = CurrentPrefSet.Brightness.Lists + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.Lists = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.Lists = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.Lists = newnum;
                    }
                }
                else if (button == britePOSbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.PositionSymbols + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.PositionSymbols = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.PositionSymbols = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.PositionSymbols = newnum;
                    }
                }
                else if (button == briteLDBbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.LimitedDataBlocks + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.LimitedDataBlocks = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.LimitedDataBlocks = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.LimitedDataBlocks = newnum;
                    }
                }
                else if (button == briteOTHbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.OtherFDBs + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.OtherFDBs = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.OtherFDBs = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.OtherFDBs = newnum;
                    }
                }
                else if (button == briteTLSbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.Tools + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.Tools = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.Tools = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.Tools = newnum;
                    }
                }
                else if (button == briteRRbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.RangeRings + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.RangeRings = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.RangeRings = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.RangeRings = newnum;
                    }
                }
                else if (button == briteCMPbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.Compass + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.Compass = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.Compass = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.Compass = newnum;
                    }
                }
                else if (button == briteBCNbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.BeaconTargets + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.BeaconTargets = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.BeaconTargets = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.BeaconTargets = newnum;
                    }
                }
                else if (button == britePRIbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.PrimaryTargets + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.PrimaryTargets = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.PrimaryTargets = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.PrimaryTargets = newnum;
                    }
                }
                else if (button == briteHSTbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 0;
                    var newnum = CurrentPrefSet.Brightness.History + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.History = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.History = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.History = newnum;
                    }
                }
                else if (button == briteWXbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 5;
                    var newnum = CurrentPrefSet.Brightness.Weather + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.Weather = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.Weather = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.Weather = newnum;
                    }
                }
                else if (button == briteWXCbutton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;
                    var step = 5;
                    var max = 100;
                    var min = 5;
                    var newnum = CurrentPrefSet.Brightness.WeatherContrast + (d * step);
                    if (newnum > max)
                    {
                        CurrentPrefSet.Brightness.WeatherContrast = max;
                    }
                    else if (newnum < min)
                    {
                        CurrentPrefSet.Brightness.WeatherContrast = min;
                    }
                    else
                    {
                        CurrentPrefSet.Brightness.WeatherContrast = newnum;
                    }
                }

                else if (button == dcbRRButton)
                {
                    int d = 0;
                    if (mousescrollcount > mousethreshold)
                        d = 1;
                    else if (mousescrollcount < -mousethreshold)
                        d = -1;


                    if (d != 0)
                    {
                        switch (CurrentPrefSet.RangeRingSpacing)
                        {
                            case 2 when d > 0:
                                CurrentPrefSet.RangeRingSpacing = 5;
                                break;
                            case 5 when d > 0:
                                CurrentPrefSet.RangeRingSpacing = 10;
                                break;
                            case 5 when d < 0:
                                CurrentPrefSet.RangeRingSpacing = 2;
                                break;
                            case 10 when d > 0:
                                CurrentPrefSet.RangeRingSpacing = 20;
                                break;
                            case 10 when d < 0:
                                CurrentPrefSet.RangeRingSpacing = 5;
                                break;
                            case 20 when d < 0:
                                CurrentPrefSet.RangeRingSpacing = 10;
                                break;
                            case 20 when d > 0:
                                break;
                            default:
                                CurrentPrefSet.RangeRingSpacing = 2;
                                break;
                        }
                    }
                }
                if (mousescrollcount > mousethreshold)
                    button.OnAdjustUp(null);
                else if (mousescrollcount < -mousethreshold)
                    button.OnAdjustDown(null);
                
                if (Math.Abs(mousescrollcount) > mousethreshold)
                    mousescrollcount = 0;
                if (activeDcbButton != dcbPlaceCntrButton)
                {
                    var relativeloc = new Point(window.Location.X + activeDcbButton.DrawnBounds.X, window.Location.Y + activeDcbButton.DrawnBounds.Y);
                    System.Windows.Forms.Cursor.Clip = new Rectangle(relativeloc, activeDcbButton.DrawnBounds.Size);
                }
                CenterMouse();
            }
        }
        private void CenterMouse()
        {
            if (centeredlast)
                return;
            Rectangle clip = window.Bounds;
            if (activeDcbButton != null && activeDcbButton.GetType() == typeof(DCBAdjustmentButton) && activeDcbButton != dcbPlaceCntrButton)
                clip = System.Windows.Forms.Cursor.Clip;
            Point mousecenter = new Point(clip.Location.X + clip.Width / 2, clip.Location.Y + clip.Height / 2);
            Mouse.SetPosition(mousecenter.X, mousecenter.Y);
            centeredmouse = true;
        }
        private PointF GeoToScreenPoint(GeoPoint geoPoint)
        {
            Vector4 vec = new Vector4((float)geoPoint.Longitude, (float)geoPoint.Latitude, 0.0f, 1.0f);
            
            vec *= geoToScreen;
            return new PointF(vec.X, vec.Y);
        }

        private Point GeoToPixel(GeoPoint geoPoint)
        {
            Vector4 vec = new Vector4((float)geoPoint.Longitude, (float)geoPoint.Latitude, 0, 1);
            vec *= geoToScreen;
            vec *= pixeltransform.Inverted();
            return new Point((int)vec.X, (int)vec.Y);
        }

        private GeoPoint ScreenToGeoPoint(PointF Point)
        {
            Vector4 vec = new Vector4(Point.X, Point.Y, 0.0f, 1.0f);
            vec *= geoToScreen.Inverted();
            return new GeoPoint(vec.Y, vec.X);

            double r = Math.Sqrt(Math.Pow(Point.X, 2) + Math.Pow(Point.Y, 2));
            double angle = Math.Atan(Point.Y / Point.X);
            if (Point.X < 0)
                angle += Math.PI;
            double bearing = 90 - (angle * 180 / Math.PI) + ScreenRotation;
            double distance = r * scale;
            return ScreenCenterPoint.FromPoint(distance, bearing);
        }
        private GeoPoint ScreenToGeoPoint(Point Point)
        {
            Vector4 vec = new Vector4(Point.X, Point.Y, 0.0f, 1.0f);
            vec *= pixeltransform;
            vec *= geoToScreen.Inverted();
            return new GeoPoint(vec.Y, vec.X);
            return ScreenToGeoPoint(LocationFromScreenPoint(Point));
        }

        private TransparentLabel[] cmp_labels = new TransparentLabel[36];
        float cmp_ar;
        private void DrawCompass()
        {
            if (CurrentPrefSet.Brightness.Compass == 0)
                return;
            var color = AdjustedColor(Color.FromArgb(140, 140, 140), CurrentPrefSet.Brightness.Compass);
            var linelength = 15 * pixelScale;
            var w = arscale.Column1.Length - pixelScale;
            var h = arscale.Column0.Length - pixelScale;
            GL.PushMatrix();
            if (!dcb.Visible)
            {
                
            }
            else if (dcb.Location == DCBLocation.Left || dcb.Location == DCBLocation.Right)
            {
                w -= dcb.Size * pixelScale / 2;
                if (dcb.Location == DCBLocation.Left)
                {
                    GL.Translate(dcb.Size / 2 * pixelScale, 0, 0);
                }
                else
                {
                    GL.Translate(-dcb.Size / 2 * pixelScale, 0, 0);
                }
            }
            else
            {
                h -= dcb.Size * pixelScale / 2;
                if (dcb.Location == DCBLocation.Top)
                {
                    GL.Translate(0, -dcb.Size / 2 * pixelScale, 0);
                }
                else
                {
                    GL.Translate(0, dcb.Size / 2 * pixelScale, 0);
                }
            }
            float aspect_ratio = w / h;
            DrawLine(new PointF(w, -h), new PointF(-w,-h), color);
            DrawLine(new PointF(-w, -h), new PointF(-w, h), color);
            DrawLine(new PointF(-w, h), new PointF(w, h), color);
            DrawLine(new PointF(w, h), new PointF(w, -h), color);
            var atan = MathHelper.RadiansToDegrees(Math.Atan(aspect_ratio));
            int i;
            var h1 = h - linelength;
            var w1 = w - linelength;
            var hr = (h1 / h);
            var wr = (w1 / w);
            for (i = 0; i < atan; i += 5)
            {
                var x = (float)Math.Tan(MathHelper.DegreesToRadians(i)) * h;
                var x1 = x * hr;
                DrawLine(new PointF(x, h), new PointF(x1, h1), color);
                DrawLine(new PointF(-x, h), new PointF(-x1, h1), color);
                DrawLine(new PointF(x, -h), new PointF(x1, -h1), color);
                DrawLine(new PointF(-x, -h), new PointF(-x1, -h1), color);
                var line = (i / 10);
                if (i / 10f == line )
                {
                    if (cmp_labels[line] == null)
                    {
                        cmp_labels[line] = new TransparentLabel { Font = Font, Text = i.ToString(), ForeColor = color };
                        cmp_labels[line + 18] = new TransparentLabel { Font = Font, Text = (i + 180).ToString(), ForeColor = color };
                        if (line > 0)
                        {
                            cmp_labels[36 - line] = new TransparentLabel { Font = Font, Text = (360 - i).ToString(), ForeColor = color };
                            cmp_labels[18 - line] = new TransparentLabel { Font = Font, Text = (180 - i).ToString(), ForeColor = color };
                        }
                        else
                        {
                            cmp_labels[line].Text = "360";
                        }
                    }
                    cmp_labels[line].ForeColor = color;
                    cmp_labels[line + 18].ForeColor = color;
                    DrawLabel(cmp_labels[line]);
                    DrawLabel(cmp_labels[line + 18]);
                    cmp_labels[line].CenterOnPoint(new PointF(x1, h1 - cmp_labels[line].SizeF.Height));
                    cmp_labels[line + 18].CenterOnPoint(new PointF(-x1, -h1 + cmp_labels[line].SizeF.Height));
                    if (line > 0)
                    {
                        cmp_labels[18 - line].ForeColor = color;
                        cmp_labels[36 - line].ForeColor = color;
                        DrawLabel(cmp_labels[18 - line]);
                        DrawLabel(cmp_labels[36 - line]);
                        cmp_labels[18 - line].CenterOnPoint(new PointF(x1, cmp_labels[line].SizeF.Height - h1));
                        cmp_labels[36 - line].CenterOnPoint(new PointF(-x1, h1 - cmp_labels[line].SizeF.Height));
                    }
                }
            }
            for (; i <= 90; i += 5)
            {
                var y = (float)Math.Tan(MathHelper.DegreesToRadians(90 - i)) * w;
                var y1 = y * wr;
                DrawLine(new PointF(w, y), new PointF(w1, y1), color);
                DrawLine(new PointF(-w, y), new PointF(-w1, y1), color);
                DrawLine(new PointF(w, -y), new PointF(w1, -y1), color);
                DrawLine(new PointF(-w, -y), new PointF(-w1, -y1), color);
                var line = (i / 10);
                if (i / 10f == line)
                {
                    if (cmp_labels[line] == null)
                    {
                        cmp_labels[line] = new TransparentLabel { Font = Font, Text = i.ToString(), ForeColor = color };
                        cmp_labels[line + 18] = new TransparentLabel { Font = Font, Text = (i + 180).ToString(), ForeColor = color };
                        cmp_labels[36 - line] = new TransparentLabel { Font = Font, Text = (360 - i).ToString(), ForeColor = color };
                        cmp_labels[18 - line] = new TransparentLabel { Font = Font, Text = (180 - i).ToString(), ForeColor = color };
                    }
                    cmp_labels[line].ForeColor = color;
                    cmp_labels[line + 18].ForeColor = color;
                    DrawLabel(cmp_labels[line]);
                    DrawLabel(cmp_labels[line + 18]);
                    cmp_labels[line].CenterOnPoint(new PointF(w1 - cmp_labels[line].SizeF.Width, y1));
                    cmp_labels[line + 18].CenterOnPoint(new PointF(cmp_labels[line].SizeF.Width - w1, -y1));
                    if (line > 0)
                    {
                        cmp_labels[18 - line].ForeColor = color;
                        cmp_labels[36 - line].ForeColor = color;
                        DrawLabel(cmp_labels[18 - line]);
                        DrawLabel(cmp_labels[36 - line]);
                        cmp_labels[36 - line].CenterOnPoint(new PointF(cmp_labels[line].SizeF.Width - w1, y1));
                        cmp_labels[18 - line].CenterOnPoint(new PointF(w1 - cmp_labels[line].SizeF.Width, -y1));
                    }
                }
            }
            GL.PopMatrix();
            if (cmp_ar != aspect_ratio)
            {
                cmp_ar = aspect_ratio;
                foreach(var label in cmp_labels)
                {
                    label.ForceRedraw();
                }
            }
            //for (int l = 0; l < 36; l++)
            //{
            //    var label = cmp_labels[l];
            //   label.ForeColor = color;
            //    DrawLabel(label);
            //}
        }
        private void DrawATPAVolumes()
        {
            foreach (var volume in ATPA.Volumes.Where(v => v.Draw))
            {
                GeoPoint one, two, three, four;
                one = volume.RunwayThreshold.FromPoint(volume.WidthLeft / 6076d, volume.TrueHeading - 90);
                two = one.FromPoint(volume.Length, volume.TrueHeading + 180);
                three = two.FromPoint((volume.WidthLeft / 6076d) + (volume.WidthRight / 6076d), volume.TrueHeading + 90);
                four = volume.RunwayThreshold.FromPoint(volume.WidthRight / 6076d, volume.TrueHeading + 90);
                Line l1, l2, l3, l4;
                l1 = new Line(one, two);
                l2 = new Line(two, three);
                l3 = new Line(three, four);
                l4 = new Line(four, one);
                DrawLine(l1, Color.Aqua);
                DrawLine(l2 , Color.Aqua);
                DrawLine(l3, Color.Aqua);
                DrawLine(l4, Color.Aqua);
            }
        }
        private void DrawMinSeps()
        {
            List<MinSep> seps;
            lock (minSeps)
                seps = minSeps.ToList();
            foreach (var minsep in seps)
            {
                if (minsep.Line1.End1 != null && minsep.Line2.End1 != null && 
                    minsep.Line1.End2 != null && minsep.Line2.End2 != null)
                {
                    DrawLine(minsep.Line1, AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools));
                    DrawLine(minsep.Line2, AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools));
                    var point1 = GeoToScreenPoint(minsep.Line1.End2);
                    var point2 = GeoToScreenPoint(minsep.Line2.End2);
                    DrawCircle(point1.X, point1.Y, 4 * pixelScale, 1, 3, AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools), true);
                    DrawCircle(point2.X, point2.Y, 4 * pixelScale, 1, 3, AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools), true);
                }
                if (minsep.SepLine.End1 != null && minsep.SepLine.End2 != null)
                {
                    DrawLine(minsep.SepLine, AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools));
                }
                else
                    continue;
                if (minsep.SepLine.MidPoint == null)
                    continue;
                var labelloc = GeoToScreenPoint(minsep.SepLine.MidPoint);
                if (minsep.MinSepDistance != null)
                    minsep.Label.Text = ((double)(minsep.MinSepDistance)).ToString("0.00") + " NM";
                if (minsep.NoXing == true)
                    minsep.Label.Text += "\r\nNO XING";
                minsep.Label.ForeColor = AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools);
                minsep.Label.Font = this.Font;
                minsep.Label.CenterOnPoint(labelloc);
                DrawLabel(minsep.Label);
            }
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
                    line.StartGeo = line.StartPlane.SweptLocation(radar);
                    line.Line.End1 = line.StartPlane.SweptLocation(radar);
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
                    line.EndGeo = line.EndPlane.SweptLocation(radar);
                    line.Line.End2 = line.EndPlane.SweptLocation(radar);
                }
                else if (line.EndGeo != null)
                {
                    line.End = GeoToScreenPoint((GeoPoint)line.EndGeo);
                    line.Line.End2 = line.EndGeo;
                }
                
                DrawLine(line.Start, line.End, AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools));
                line.Label.ForeColor = AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools);
                line.Label.LocationF = line.End;
                double bearing = 0;
                double range = 0;
                if (line.EndGeo != null)
                {
                    bearing = line.StartGeo.BearingTo(line.EndGeo) - ScreenRotation;
                    if (bearing == 000)
                        bearing = 360;
                    range = line.StartGeo.DistanceTo(line.EndGeo);
                }
                else
                {
                    GeoPoint tempEndGeo = ScreenToGeoPoint(line.End);
                    bearing = line.StartGeo.BearingTo(tempEndGeo) - ScreenRotation;
                    if (bearing == 000)
                        bearing = 360;
                    range = line.StartGeo.DistanceTo(tempEndGeo);
                }
                bearing = (int)((bearing + 720.5) % 360);
                if ((line.StartPlane != null && line.EndPlane == null) || (line.StartPlane == null && line.EndPlane != null))
                {
                    Aircraft plane;
                    if (line.StartPlane == null)
                        plane = line.EndPlane;
                    else
                        plane = line.StartPlane;
                    var traversalTime = range * 60 / plane.GroundSpeed;
                    int time;
                    if (traversalTime - (int)traversalTime >= 0.5)
                        time = (int)traversalTime + 1;
                    else
                        time = (int)traversalTime;
                    line.Label.Text = string.Format("{0}/{1}/{3}-{2}", bearing.ToString("000"), range.ToString("0.00"), index, time);
                }
                else
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
            StartReceivers();
            SetupDCB();
            LoadVideoMapFile();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            StopReceivers();
            
        }

        public void SaveSettings(string path)
        {
            var settingsxml = "";
            try
            {
                settingsxml = XmlSerializer<RadarWindow>.Serialize(this);
                if (settingsxml == null || settingsxml.Length == 0)
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
            if (!ShowRangeRings)
                return;
            /*
            double bearing = radar.Location.BearingTo(RangeRingCenter) - ScreenRotation;
            float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
            float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale));
            */
            double distance = ScreenCenterPoint.DistanceTo(RangeRingCenter);
            var rrr = (aspect_ratio > 1 ? CurrentPrefSet.Range * 1.414 * aspect_ratio : CurrentPrefSet.Range * 1.414 / aspect_ratio) + distance;
            var x = RangeRingCenter.Longitude;
            var y = RangeRingCenter.Latitude;
            var latfactor = Math.Cos(MathHelper.DegreesToRadians(ScreenCenterPoint.Latitude));
            GL.PushMatrix();
            GL.MultMatrix(ref geoToScreen);

            for (double i = CurrentPrefSet.RangeRingSpacing; i <= rrr && CurrentPrefSet.RangeRingSpacing > 0; i += CurrentPrefSet.RangeRingSpacing)
            {
                DrawCircle(x,y, (i / 60d) / latfactor, latfactor, 1000, AdjustedColor(RangeRingColor, CurrentPrefSet.Brightness.RangeRings));
            }
            GL.PopMatrix();
        }


        private void DrawCircle (double cx, double cy, double r, double aspect_ratio, int num_segments, Color color, bool fill = false)
        {
            GL.Begin(fill ? PrimitiveType.Polygon : PrimitiveType.LineLoop);
            GL.Color4(color);
            for (int ii = 0; ii < num_segments; ii++)
            {
                float theta = 2.0f * (float)Math.PI * ii / num_segments;
                var x = r * Math.Cos(theta);
                var y = r * Math.Sin(theta) * aspect_ratio;

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
            DrawCircle(x, y, r, 1, 100, color, fill);
        }
        private void DrawTPA(Aircraft plane)
        {
            if (plane.Location == null)
                return;
            if (plane.ATPAStatus == ATPAStatus.Caution || plane.ATPAStatus == ATPAStatus.Alert)
            {
                switch (plane.ATPAStatus)
                {
                    case ATPAStatus.Caution when plane.PositionInd == ThisPositionIndicator || plane.ATPAVolume.TcpDisplay.Any(x => x.TCP == ThisPositionIndicator):
                        if(plane.ATPACone == null)
                            plane.ATPACone = new TPACone(plane, (decimal)plane.ATPARequiredMileage, ATPACautionColor, Font, true, plane.ATPATrackToLeader);
                        else
                        {
                            plane.ATPACone.Miles = (decimal)plane.ATPARequiredMileage;
                            plane.ATPACone.Color = ATPACautionColor;
                            plane.ATPACone.Track = plane.ATPATrackToLeader;
                        }
                        break;
                    case ATPAStatus.Alert when plane.PositionInd == ThisPositionIndicator || plane.ATPAVolume.TcpDisplay.Any(x => x.TCP == ThisPositionIndicator):
                        if (plane.ATPACone == null)
                            plane.ATPACone = new TPACone(plane, (decimal)plane.ATPARequiredMileage, ATPAAlertColor, Font, true, plane.ATPATrackToLeader);
                        else
                        {
                            plane.ATPACone.Miles = (decimal)plane.ATPARequiredMileage;
                            plane.ATPACone.Color = ATPAAlertColor;
                            plane.ATPACone.Track = plane.ATPATrackToLeader;
                        }
                        break;
                }
                DrawATPACone(plane);
            }
            if (plane.TPA == null)
            {
                if (((plane.PositionInd == ThisPositionIndicator && DrawATPAMonitorCones) ||
                    plane.ATPAVolume.TcpDisplay.Any(x => x.TCP == ThisPositionIndicator && x.ConeType == ATPAStatus.Monitor))
                    && plane.ATPAStatus == ATPAStatus.Monitor && plane.ATPATrackToLeader != null && plane.ATPARequiredMileage != null)
                {
                    if (plane.ATPACone == null )
                        plane.ATPACone = new TPACone(plane, (decimal)plane.ATPARequiredMileage, TPAColor, Font, true, plane.ATPATrackToLeader);
                    else
                    {
                        plane.ATPACone.Miles = (decimal)plane.ATPARequiredMileage;
                        plane.ATPACone.Color = TPAColor;
                        plane.ATPACone.Track = plane.ATPATrackToLeader;
                    }
                    DrawATPACone(plane);
                }
                
                return;
            }
            else if (plane.TPA.Type == TPAType.JRing)
            {
                //DrawCircle(plane.SweptLocation, (float)plane.TPA.Miles, plane.TPA.Color, false);
                DrawJRing(plane);
            }
            else if (plane.TPA.Type == TPAType.PCone)
            {
                DrawPCone(plane);
            }
            return;
        }

        private void DrawJRing(Aircraft plane)
        {
            PointF location = GeoToScreenPoint(plane.SweptLocation(radar));
            var x = RoundUpToNearest(location.X, pixelScale);
            var y = RoundUpToNearest(location.Y, pixelScale);

            if (plane.TPA.Miles < 10)
            {
                plane.TPA.Label.Text = plane.TPA.Miles.ToString();
            }
            else
            {
                plane.TPA.Label.Text = ((int)plane.TPA.Miles).ToString();
            }
            PointF labellocation;
            var circlesize = (float)plane.TPA.Miles / scale;
            double angle;
            LeaderDirection ldr;
            if (plane.LDRDirection != null)
                ldr = plane.LDRDirection.Value;
            else if (plane.PositionInd == ThisPositionIndicator) // owned LDR direction
                ldr = CurrentPrefSet.OwnedDataBlockPosition;
            else if (!plane.Associated) // Unassociated LDR direction
                ldr = CurrentPrefSet.UnassociatedDataBlockPosition;
            else
                ldr = CurrentPrefSet.UnownedDataBlockPosition;

            switch (ldr)
            {
                case LeaderDirection.N:
                    angle = Math.PI;
                    break;
                case LeaderDirection.NW:
                    angle = Math.PI * .75;
                    break;
                case LeaderDirection.W:
                    angle = Math.PI * .5;
                    break;
                case LeaderDirection.SW:
                    angle = Math.PI * .25;
                    break;
                case LeaderDirection.S:
                    angle = 0; 
                    break;
                case LeaderDirection.SE:
                    angle = Math.PI * 1.75;
                    break;
                case LeaderDirection.E:
                    angle = Math.PI * 1.5;
                    break;
                case LeaderDirection.NE:
                    angle = Math.PI * 1.25;
                    break;
                default: 
                    angle = 0;
                    break;
            }

            
            var labelsize = Math.Sqrt(Math.Pow(plane.TPA.Label.Width, 2) + Math.Pow(plane.TPA.Label.Height, 2));
            labelsize *= pixelScale;
            labelsize /= 2;
            labellocation = new PointF((float)((Math.Sin(angle)) * (circlesize - labelsize)), (float)(Math.Cos(angle) * (circlesize - labelsize)));
            plane.TPA.Label.CenterOnPoint(labellocation);
            GL.Translate(x, y, 0.0);
            GL.PushMatrix();
            //GL.Rotate(-ScreenRotation, 0, 0, 1);
            DrawCircle(0, 0, circlesize, 1, 500, plane.TPA.Color, false);
            plane.TPA.Label.ForeColor = plane.TPA.Color;
            if (plane.TPA.ShowSize)
                DrawLabel(plane.TPA.Label);
            GL.PopMatrix();
            GL.Translate(-x, -y, 0.0);
        }

        private void DrawPCone(Aircraft plane)
        {
            DrawPCone(plane.TPA as TPACone);
        }
        private void DrawATPACone(Aircraft plane)
        {
            DrawPCone(plane.ATPACone);
        }
        private void DrawPCone(TPACone cone)
        {
            if (cone == null || cone.ParentAircraft == null)
            {
                return;
            }
            Aircraft plane = cone.ParentAircraft;
            double track = 0;
            if (cone.Track == null)
                track = plane.SweptTrack(radar);
            else
                track = (double)cone.Track;
            GeoPoint planelocation = plane.SweptLocation(radar);
            PointF location = GeoToScreenPoint(planelocation);
            var textline = new Line(planelocation, planelocation.FromPoint((double)cone.Miles, track));
            
            if (cone.Miles < 10)
            {
                cone.Label.Text = cone.Miles.ToString();
            }
            else
            {
                cone.Label.Text = ((int)cone.Miles).ToString();
            }
            
            var endwidth = pixelScale * TPAConeWidth;
            var y = (float)cone.Miles / scale;
            //var endwidth = y * (2d / 30);
            var x1 = endwidth / 2;
            var x2 = -x1;
            var clearanceWidth = cone.ShowSize ? (float)(Math.Sqrt(Math.Pow(cone.Label.Height, 2) + Math.Pow(cone.Label.Width, 2)) * pixelScale) : 0;
            if (y >  clearanceWidth)
            {
                GL.Translate(location.X, location.Y, 0.0);
                GL.PushMatrix();
                GL.Rotate(-track + ScreenRotation, 0, 0, 1);
                var y1 = y / 2 - clearanceWidth / 2;
                var y2 = y1 + clearanceWidth;
                var x3 = x1 * (y1 / y);
                var x4 = -x3;
                var x5 = (y2 / y1) * x3;
                var x6 = -x5;
                var color = AdjustedColor(cone.Color, CurrentPrefSet.Brightness.Tools);
                DrawLine(0, 0, x3, y1, color);
                DrawLine(0, 0, x4, y1, color);
                DrawLine(x5, y2, x1, y, color);
                DrawLine(x6, y2, x2, y, color);
                DrawLine(x1, y, x2, y, color);
                GL.PopMatrix();
                GL.Translate(-location.X, -location.Y, 0.0);
                cone.Label.ForeColor = cone.Color;
                cone.Label.CenterOnPoint(GeoToScreenPoint(textline.MidPoint));
                if (cone.ShowSize)
                    DrawLabel(cone.Label);
            }
        }
        private void DrawVideoMapLines()
        {
            List<Line> lines = new List<Line>();
            if (VideoMaps.Count > 0)
            {
                CurrentPrefSet.DisplayedMaps = VideoMaps.Where(x => x.Visible).Select(x => x.Number).ToArray();
            }
            foreach (var map in VideoMaps.Where(map => map.Category == MapCategory.A))
            {
                if (CurrentPrefSet.DisplayedMaps.Contains(map.Number))
                {
                    lines.AddRange(map.Lines);
                }
            }
            var colora = AdjustedColor(VideoMapLineColor, CurrentPrefSet.Brightness.MapA);
            DrawLines(lines, colora);
            lines.Clear();
            foreach (var map in VideoMaps.Where(map => map.Category == MapCategory.B))
            {
                if (CurrentPrefSet.DisplayedMaps.Contains(map.Number))
                {
                    lines.AddRange(map.Lines);
                }
            }
            var colorb = AdjustedColor(VideoMapBLineColor, CurrentPrefSet.Brightness.MapB);
            DrawLines(lines, colorb);
        }

        private void DrawLines (List<Line> lines, Color color)
        {
            GL.PushMatrix();
            GL.MultMatrix(ref geoToScreen);
            foreach (Line line in lines)
            {
                DrawLine(line.End1.Longitude, line.End1.Latitude, line.End2.Longitude, line.End2.Latitude, color);
            }
            GL.PopMatrix();
        }

        private void DrawLine(PointF Point1, PointF Point2, Color color)
        {
            DrawLine(Point1.X, Point1.Y, Point2.X, Point2.Y, color);
        }
        private void DrawLine(Line line, Color color)
        {

            /*var end1 = GeoToScreenPoint(line.End1);
            var end2 = GeoToScreenPoint(line.End2);
            float x1 = end1.X;
            float x2 = end2.X;
            float y1 = end1.Y;
            float y2 = end2.Y;*/
            GL.PushMatrix();
            GL.MultMatrix(ref geoToScreen);
            DrawLine((float)line.End1.Longitude, (float)line.End1.Latitude, (float)line.End2.Longitude, (float)line.End2.Latitude, color);
            GL.PopMatrix();
        }

        private void DrawPolygon (Polygon polygon)
        {
            //GL.Scale(aspect_ratio, 1.0f, 1.0f);
            GL.Begin(PrimitiveType.Polygon);
            var color = AdjustedColor(polygon.Color, CurrentPrefSet.Brightness.Weather);
            GL.Color4(color);
            for (int i = 0; i < polygon.vertices.Length; i++)
            {
                GL.Vertex2(polygon.vertices[i].X, polygon.vertices[i].Y);
            }
            GL.End();
            if (polygon.StippleColor != null && polygon.StipplePattern.Length == 128)
            {
                GL.Enable(EnableCap.PolygonStipple);
                GL.PolygonStipple(polygon.StipplePattern);
                GL.Begin(PrimitiveType.Polygon);
                var scolor = AdjustedColor(polygon.StippleColor, CurrentPrefSet.Brightness.Weather);
                GL.Color4(scolor);
                for (int i = 0; i < polygon.vertices.Length; i++)
                {
                    GL.Vertex2(polygon.vertices[i].X, polygon.vertices[i].Y);
                }
                GL.End();
                GL.Disable(EnableCap.PolygonStipple);
            }
        }

        private void DrawNexrad()
        {
            //convert old nexrads list to new nexrad object
            if (Nexrads != null && Nexrads.Count > 0)
            {
                Nexrad = Nexrads[0];
                Nexrad.ColorTable = new List<WXColor>();
                Nexrads = null;
            }
        
            var polygons = Nexrad.Polygons();
            GL.PushMatrix();
            GL.MultMatrix(ref geoToScreen);

            for (int i = 0; i < polygons.Length; i++)
            {
                if(polygons[i].Color.A > 0)
                    DrawPolygon(polygons[i]);
            }

            GL.PopMatrix();
            //}
        }
        private void DrawLine (double x1, double y1, double x2, double y2, Color color, float width = 1)
        {
            //x1 = RoundUpToNearest(x1, pixelScale);
            //x2 = RoundUpToNearest(x2, pixelScale);
            //y1 = RoundUpToNearest(y1, pixelScale);
            //y2 = RoundUpToNearest(y2, pixelScale);
            GL.Begin(PrimitiveType.Lines);
            GL.LineWidth(width);
            GL.Color4(color);
            GL.Vertex2(x1, y1);
            GL.Vertex2(x2, y2);
            GL.End();
        }
        private List<TransparentLabel> dataBlocks = new List<TransparentLabel>();
        private List<TransparentLabel> posIndicators = new List<TransparentLabel>();
        private async Task GenerateDataBlock(Aircraft aircraft)
        {
            lock (aircraft)
            {
                if (aircraft.Deleted)
                    return;
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
                else if (aircraft.ForceQuickLook)
                {
                    aircraft.DataBlock.ForeColor = PointoutColor;
                    aircraft.DataBlock2.ForeColor = PointoutColor;
                    aircraft.DataBlock3.ForeColor = PointoutColor;
                }
                else if (aircraft.Owned || aircraft.QuickLookPlus)
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
                aircraft.RedrawDataBlock(radar);
                var realWidth = aircraft.DataBlock.Width * pixelScale;
                var realHeight = aircraft.DataBlock.Height * pixelScale;
                
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

        }

        private async Task GenerateTargetAsync(Aircraft aircraft)
        {
            if (aircraft == debugPlane)
                Console.Write("");
            var extrapolatedpos = aircraft.SweptLocation(radar);
            /*
            double bearing = radar.Location.BearingTo(extrapolatedpos) - ScreenRotation;
            double distance = radar.Location.DistanceTo(extrapolatedpos);
            float x = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
            float y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale));
            var location = new PointF(x, y);
            */
            if (extrapolatedpos == null)
                return;
            //var location = GeoToScreenPoint(extrapolatedpos);
            //if (location.X == 0 || location.Y == 0)
             //   return;
            if (aircraft.LastHistoryTimes.ContainsKey(radar) && 
                (radar.SweptTimes[aircraft] - aircraft.LastHistoryTimes[radar]).TotalSeconds >= CurrentPrefSet.HistoryRate)
            {
                aircraft.TargetReturn.ForeColor = HistoryColors[0];
                //aircraft.TargetReturn.ShapeHeight = HistoryHeight;
                //aircraft.TargetReturn.ShapeWidth = HistoryWidth;
                //aircraft.TargetReturn.Shape = HistoryShape;
                if (!HistoryFade)
                {
                    aircraft.TargetReturn.Fading = false;
                    aircraft.TargetReturn.Intensity = 1;
                    var lastHistory = aircraft.History.Length - 1;
                    lock (aircraft.History)
                    {
                        if (aircraft.History[lastHistory] != null)
                            aircraft.History[lastHistory].Dispose();
                        for (int i = lastHistory; i > 0; i--)
                        {
                            aircraft.History[i] = aircraft.History[i - 1];
                            if (aircraft.History[i] == null)
                                continue;
                            if (i >= HistoryColors.Length)
                                aircraft.History[i].ForeColor = HistoryColors[HistoryColors.Length - 1];
                            else
                                aircraft.History[i].ForeColor = HistoryColors[i];
                        }
                    }
                }
                else
                {
                    aircraft.TargetReturn.Fading = true;
                }
                PrimaryReturn newreturn = new PrimaryReturn();
                aircraft.History[0] = aircraft.TargetReturn;
                aircraft.TargetReturn = newreturn;
                newreturn.ParentAircraft = aircraft;
                newreturn.Fading = PrimaryFade;
                newreturn.FadeTime = FadeTime;
                //newreturn.NewLocation = location;
                newreturn.GeoLocation = extrapolatedpos;
                newreturn.Intensity = 1;
                newreturn.ForeColor = ReturnColor;
                //newreturn.ShapeHeight = TargetHeight;
                //newreturn.ShapeWidth = TargetWidth;
                //newreturn.Shape = radar.TargetShape;
                aircraft.LastHistoryTimes[radar] = CurrentTime;
            }

            if (aircraft.LastMessageTime > CurrentTime.AddSeconds(-LostTargetSeconds))
            {
                lock(aircraft) aircraft.RedrawTarget(extrapolatedpos, radar);
                aircraft.PTL.End1 = aircraft.SweptLocation(radar);
                double ptldistance = (aircraft.SweptSpeed(radar) / 60) * CurrentPrefSet.PTLLength;
                aircraft.PTL.End2 = extrapolatedpos.FromPoint(ptldistance, aircraft.SweptTrack(radar));

                if (InFilter(aircraft) ||
                    aircraft.Owned || aircraft.QuickLook || aircraft.PendingHandoff == ThisPositionIndicator || aircraft.ShowCallsignWithNoSquawk || aircraft.FDB)
                    GenerateDataBlock(aircraft);
                else if (!aircraft.Owned && !aircraft.FDB)
                    lock (dataBlocks)
                        dataBlocks.Remove(aircraft.DataBlock);


                var realWidth = aircraft.PositionIndicator.Width * pixelScale;
                var realHeight = aircraft.PositionIndicator.Height * pixelScale;
                aircraft.PositionIndicator.SizeF = new SizeF(realWidth, realHeight);
                var posindlocation = GeoToScreenPoint(TargetExtentSymbols.PositionSymbolLocation(aircraft, radar));
                aircraft.PositionIndicator.CenterOnPoint(posindlocation);
                if (!string.IsNullOrEmpty(aircraft.PositionInd))
                    aircraft.PositionIndicator.Text = aircraft.PositionInd.Last().ToString();
                if (aircraft.Marked)
                    aircraft.PositionIndicator.ForeColor = SelectedColor;
                else if (aircraft.Owned)
                    aircraft.PositionIndicator.ForeColor = OwnedColor;
                else
                    aircraft.PositionIndicator.ForeColor = DataBlockColor;

                if (!posIndicators.Contains(aircraft.PositionIndicator))
                {
                    lock (aircraft)
                    {
                        if (aircraft.Deleted)
                            return;
                        aircraft.PositionIndicator.ParentAircraft = aircraft;
                        lock (posIndicators)
                            posIndicators.Add(aircraft.PositionIndicator);

                    }
                }
                lock (minSeps)
                {
                    minSeps.Where(x => x.Plane1 == aircraft || x.Plane2 == aircraft).ToList().ForEach(x => x.CalculateMinSep(radar));
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
        private async Task ADSBtoFlightPlanCallsigns(List<Aircraft> aircraft)
        {
            aircraft.ForEach(x => ADSBtoFlightPlanCallsign(x));
        }
        private async Task ADSBtoFlightPlanCallsign(Aircraft aircraft)
        {
            if (string.IsNullOrWhiteSpace(aircraft.FlightPlanCallsign) && !string.IsNullOrWhiteSpace(aircraft.Callsign))
            {
                var associated = aircraft.Associated;
                if (UseADSBCallsigns && !associated && aircraft.Squawk != null && aircraft.Squawk != "1200")
                {
                    aircraft.FlightPlanCallsign = aircraft.Callsign;
                }
                else if (UseADSBCallsigns1200 && !associated && aircraft.Squawk != null && aircraft.Squawk == "1200")
                {
                    aircraft.FlightPlanCallsign = aircraft.Callsign;
                }
                else if (UseADSBCallsignsAssociated && associated)
                {
                    aircraft.FlightPlanCallsign = aircraft.Callsign;
                }
            }
        }
        private void GenerateTarget(Aircraft aircraft)
        {
            Task.Run(() => GenerateTargetAsync(aircraft));
        }
        bool generating = false;
        private async Task GenerateTargets()
        {
            if (generating)
                return;
            generating = true;
            List<Task> tasks = new List<Task>();
            var time = CurrentTime;
            Aircraft[] ac;
            lock (Aircraft)
            {
                ac = Aircraft.ToArray();
                RadarSites.ForEach(x => x.Scan(time));
                if (!RadarSites.Contains(radar))
                    radar.Scan(time);
            }
            
            for (int i = 0; i < ac.Length; i++)
            {
                var aircraft = ac[i];
                var associated = !(string.IsNullOrEmpty(aircraft.PositionInd) || aircraft.PositionInd == "*");
                var qlall = associated && QuickLookList.Contains("ALL");
                var qlallplus = associated && QuickLookList.Contains("ALL+");
                if (QuickLookList.Contains(aircraft.PositionInd) || qlall)
                {
                    aircraft.QuickLookPlus = false;
                    aircraft.QuickLook = true;
                }
                else if (QuickLookList.Contains(aircraft.PositionInd + "+") || qlallplus)
                {
                    aircraft.QuickLook = true;
                    aircraft.QuickLookPlus = true;
                }
                else if (QuickLook && InFilter(aircraft) &&
                    aircraft.LastMessageTime >= CurrentTime.AddSeconds(-LostTargetSeconds))
                {
                    aircraft.QuickLook = true;
                    aircraft.QuickLookPlus = false;
                }
                else
                {
                    aircraft.QuickLookPlus = false;
                    aircraft.QuickLook = false;
                }
                if (aircraft.Location != null)
                    GenerateTargetAsync(aircraft);
                for (int j = 0; j < aircraft.History.Length; j++)
                {
                    var target = aircraft.History[j];
                    if (target == null)
                        continue;
                    if (target.Intensity < .001)
                    {
                        if (target.ParentAircraft.TargetReturn == target)
                        {
                            lock (dataBlocks)
                                dataBlocks.Remove(target.ParentAircraft.DataBlock);
                            lock (posIndicators)
                                posIndicators.Remove(target.ParentAircraft.PositionIndicator);
                        }
                    }
                }
            }    
            generating = false;
        }

        private PointF OffsetDatablockLocation(Aircraft thisAircraft, LeaderDirection direction)
        {
            PointF blockLocation = new PointF();
            blockLocation.X = thisAircraft.LocationF.X;
            blockLocation.Y = thisAircraft.LocationF.Y;
            
            switch (direction)
            {
                case LeaderDirection.N:
                    blockLocation.Y = thisAircraft.PositionIndicator.BoundsF.Bottom + dataBlockOffset;
                    //blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Right;
                    break;
                case LeaderDirection.S:
                    blockLocation.Y = thisAircraft.PositionIndicator.BoundsF.Top - dataBlockOffset;
                    //blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Right;
                    break;
                case LeaderDirection.E:
                    blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Right + dataBlockOffset;
                    break;
                case LeaderDirection.W:
                    blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Left - dataBlockOffset;
                    if (thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock2.SizeF.Width &&
                        thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock3.SizeF.Width)
                        blockLocation.X -= thisAircraft.DataBlock.SizeF.Width;
                    else if (thisAircraft.DataBlock2.SizeF.Width > thisAircraft.DataBlock3.SizeF.Width)
                        blockLocation.X -= thisAircraft.DataBlock2.SizeF.Width;
                    else
                        blockLocation.X -= thisAircraft.DataBlock3.SizeF.Width;
                    break;
                case LeaderDirection.NE:
                    
                    blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Right + dataBlockDiagonalOffset;
                    //if (offset > 0)
                    //    blockLocation.X += offsetScale;
                    blockLocation.Y = thisAircraft.PositionIndicator.BoundsF.Bottom + dataBlockDiagonalOffset;
                    break;
                case LeaderDirection.SE:
                    blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Right + dataBlockDiagonalOffset;
                    blockLocation.Y = thisAircraft.PositionIndicator.BoundsF.Top - dataBlockDiagonalOffset;
                    break;
                case LeaderDirection.NW:
                    blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Left - dataBlockDiagonalOffset;
                    //if (offset > 0)
                    //    blockLocation.X -= offsetScale;
                    if (thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock2.SizeF.Width &&
                        thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock3.SizeF.Width)
                        blockLocation.X -= thisAircraft.DataBlock.SizeF.Width;
                    else if (thisAircraft.DataBlock2.SizeF.Width > thisAircraft.DataBlock3.SizeF.Width)
                        blockLocation.X -= thisAircraft.DataBlock2.SizeF.Width;
                    else
                        blockLocation.X -= thisAircraft.DataBlock3.SizeF.Width;
                    blockLocation.Y = thisAircraft.PositionIndicator.BoundsF.Bottom +  dataBlockDiagonalOffset;
                    break;
                case LeaderDirection.SW:
                    blockLocation.X = thisAircraft.PositionIndicator.BoundsF.Left - dataBlockDiagonalOffset;
                    if (thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock2.SizeF.Width &&
                        thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock3.SizeF.Width)
                        blockLocation.X -= thisAircraft.DataBlock.SizeF.Width;
                    else if (thisAircraft.DataBlock2.SizeF.Width > thisAircraft.DataBlock3.SizeF.Width)
                        blockLocation.X -= thisAircraft.DataBlock2.SizeF.Width;
                    else
                        blockLocation.X -= thisAircraft.DataBlock3.SizeF.Width;
                    blockLocation.Y = thisAircraft.PositionIndicator.BoundsF.Top - dataBlockDiagonalOffset;
                    break;
            }

            /*
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
            }*/

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
            //if (LeaderLength != 0)// && !(direction == LeaderDirection.N || direction == LeaderDirection.NE || direction == LeaderDirection.NW))
            //if (thisAircraft.DataBlock.SizeF.Height * 0.75 <= offset && direction != LeaderDirection.N)
            blockLocation.Y -= dataBlockOffsetScale * 2.5f;
            PointF leaderStart = new PointF(thisAircraft.LocationF.X, thisAircraft.LocationF.Y);
            
            switch (direction)
            {
                case LeaderDirection.NE:
                    leaderStart.Y = thisAircraft.PositionIndicator.BoundsF.Bottom;
                    leaderStart.X = thisAircraft.PositionIndicator.BoundsF.Right;
                    break;
                case LeaderDirection.N:
                    leaderStart.Y = thisAircraft.PositionIndicator.BoundsF.Bottom;
                    break;
                case LeaderDirection.NW:
                    leaderStart.Y = thisAircraft.PositionIndicator.BoundsF.Bottom;
                    leaderStart.X = thisAircraft.PositionIndicator.BoundsF.Left;
                    break;
                case LeaderDirection.SE:
                    leaderStart.Y = thisAircraft.PositionIndicator.BoundsF.Top;
                    leaderStart.X = thisAircraft.PositionIndicator.BoundsF.Right;
                    break;
                case LeaderDirection.S:
                    leaderStart.Y = thisAircraft.PositionIndicator.BoundsF.Top;
                    break;
                case LeaderDirection.SW:
                    leaderStart.Y = thisAircraft.PositionIndicator.BoundsF.Top;
                    leaderStart.X = thisAircraft.PositionIndicator.BoundsF.Left;
                    break;
                case LeaderDirection.E:
                    leaderStart.X = thisAircraft.PositionIndicator.BoundsF.Right;
                    break;
                case LeaderDirection.W:
                    leaderStart.X = thisAircraft.PositionIndicator.BoundsF.Left;
                    break;
                default:
                    Console.Write("wat");
                    break;

            }
            if (blockLocation.X < thisAircraft.LocationF.X)
            {
                if (thisAircraft.DataBlock.SizeF.Width > thisAircraft.DataBlock2.SizeF.Width)
                    thisAircraft.ConnectingLine.End = new PointF(blockLocation.X + thisAircraft.DataBlock.SizeF.Width,
                        blockLocation.Y + (dataBlockOffsetScale * 2.5f));
                else
                    thisAircraft.ConnectingLine.End = new PointF(blockLocation.X + thisAircraft.DataBlock2.SizeF.Width,
                        blockLocation.Y + (dataBlockOffsetScale * 2.5f));
            }
            else
            {
                thisAircraft.ConnectingLine.End = new PointF(blockLocation.X, blockLocation.Y + (dataBlockOffsetScale * 2.5f));
            }
            thisAircraft.ConnectingLine.Start = leaderStart;
            if (direction != thisAircraft.LastDrawnDirection)
                lock (thisAircraft) thisAircraft.RedrawDataBlock(radar, direction);

            return blockLocation;
        }
        private PointF OffsetDatablockLocation(Aircraft thisAircraft)
        {
            LeaderDirection newDirection = CurrentPrefSet.UnassociatedDataBlockPosition;
            LeaderDirection oldDirection = CurrentPrefSet.UnassociatedDataBlockPosition;
            if (thisAircraft.LDRDirection != null)
            {
                oldDirection = thisAircraft.LDRDirection.Value;
                newDirection = thisAircraft.LDRDirection.Value;
            }
            else if (thisAircraft.PositionInd == null)
            {

            }
            else if (thisAircraft.PositionInd == ThisPositionIndicator && thisAircraft.OwnerLeaderDirection != null)
            {
                oldDirection = thisAircraft.OwnerLeaderDirection.Value;
                newDirection = thisAircraft.OwnerLeaderDirection.Value;
            }
            else if (thisAircraft.PositionInd == ThisPositionIndicator)
            {
                oldDirection = CurrentPrefSet.OwnedDataBlockPosition;
                newDirection = CurrentPrefSet.OwnedDataBlockPosition;
            }
            else if (CurrentPrefSet.OtherOwnersLeaderDirections.TryGetValue(thisAircraft.PositionInd, out LeaderDirection dir))
            {
                oldDirection = dir;
                newDirection = dir;
            }
            else if (thisAircraft.Associated)
            {
                oldDirection = CurrentPrefSet.UnownedDataBlockPosition; 
                newDirection = CurrentPrefSet.UnownedDataBlockPosition;
            }



            PointF blockLocation = OffsetDatablockLocation(thisAircraft, newDirection);
            
            
            if (AutoOffset &&  thisAircraft.FDB && thisAircraft.LDRDirection == null)
            {
                
                RectangleF bounds = new RectangleF(blockLocation, thisAircraft.DataBlock.SizeF);
                int minconflicts = int.MaxValue;
                LeaderDirection bestDirection = newDirection;
                int[] sequence = new int[] {0, 2, 6, 7, 1, 3, 5, 4};
                for (int i = 0; i < 8; i++)
                {
                    int conflictcount = 0;
                    List<TransparentLabel> otherDataBlocks = new List<TransparentLabel>();
                    lock (dataBlocks)
                        otherDataBlocks.AddRange(dataBlocks);
                    var d = (int)newDirection;
                    if (d >= 5)
                    {
                        d--;
                    }
                    d = (d + sequence[i]) % 8;
                    if (d >= 5)
                    {
                        d++;
                    }
                    newDirection = (LeaderDirection)d;
                    blockLocation = OffsetDatablockLocation(thisAircraft, newDirection);
                    
                    bounds.Location = blockLocation;

                    foreach (var otherDataBlock in otherDataBlocks)
                    {
                        //if (!otherDataBlock.ParentAircraft.FDB)
                            //continue; //both must be fdbs
                        var otherPlane = otherDataBlock.ParentAircraft;
                        if (thisAircraft != otherPlane)
                        {
                            RectangleF otherBounds = new RectangleF(otherPlane.DataBlock.LocationF, otherPlane.DataBlock.SizeF);
                            
                            if (bounds.IntersectsWith(otherBounds) && otherPlane.FDB)
                            {
                                conflictcount+=2;
                            }
                            if (bounds.IntersectsWith(otherPlane.TargetReturn.BoundsF))
                            {
                                conflictcount++;
                            }
                            if (thisAircraft.ConnectingLine.IntersectsWith(otherPlane.ConnectingLine) ||
                                thisAircraft.ConnectingLine.IntersectsWith(otherPlane.PositionIndicator.BoundsF) ||
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
        
        

        private bool InFilter(Aircraft aircraft)
        {
            if (!aircraft.Associated)
                return (aircraft.TrueAltitude <= MaxAltitude && aircraft.TrueAltitude >= MinAltitude);
            return (aircraft.TrueAltitude <= MaxAltitudeAssociated && aircraft.TrueAltitude >= MinAltitudeAssociated);
        }

        private void DrawTarget(PrimaryReturn target)
        {
            if (target == null)
                return;
            if (target.ParentAircraft.Location == null)
                return;
            if ((target.LastDrawnScreenCenter != ScreenCenterPoint || target.LastDrawnRange != CurrentPrefSet.Range || target.LastDrawnScreenRotation != ScreenRotation) && target.GeoLocation != null)
            {
                target.LocationF = GeoToScreenPoint(target.GeoLocation);
                target.LastDrawnScreenRotation = ScreenRotation;
                target.LastDrawnScreenCenter = ScreenCenterPoint;
                target.LastDrawnRange = CurrentPrefSet.Range;
                if (target.ParentAircraft != null && target == target.ParentAircraft.TargetReturn)
                {
                    target.ParentAircraft.LocationF = target.LocationF;
                }
            }
            if (target.LocationF.X == 0 || target.LocationF.Y == 0)
                return;
            var history = target != target.ParentAircraft.TargetReturn;
            if (history) // history
            {
                if (!InFilter(target.ParentAircraft) && !target.ParentAircraft.FDB)
                    return;
                if (Array.IndexOf(target.ParentAircraft.History, target) >= CurrentPrefSet.HistoryNum)
                    return;
            }
            var primarycolor = AdjustedColor(ReturnColor, CurrentPrefSet.Brightness.PrimaryTargets);
            var beaconcolor = AdjustedColor(BeaconTargetColor, CurrentPrefSet.Brightness.BeaconTargets);
            var rt = radar.RadarType;
            if (history)
                rt = RadarType.FUSED;
            var location = target.ParentAircraft.SweptLocation(radar);
            if (location == null)
                return;
            float targetWidth;
            float targetHeight;
            float targetHypotenuse;
            float x1;
            float angle;
            switch (rt)
            {
                case RadarType.SLANT_RANGE:
                    //float targetHeight = target.ShapeHeight * pixelScale; // (window.ClientRectangle.Height/2);
                    targetWidth = (float)TargetExtentSymbols.TargetWidth(target.ParentAircraft, radar, scale, pixelScale);
                    targetHeight = (TargetExtentSymbols.SearchTargets.RangeExtent / 32f) / scale;    // (window.ClientRectangle.Width/2);
                    targetHypotenuse = (float)(Math.Sqrt((targetHeight * targetHeight) + (targetWidth * targetWidth)) / 2);
                    float beaconoffset = TargetExtentSymbols.BeaconTargets.RangeOffset / (32f * scale);
                    x1 = (float)(targetWidth / 2);
                    float y1 = (float)(targetHeight / 2);
                    float x2 = x1 * (TargetExtentSymbols.BeaconTargets.AzimuthExtentFactor / 10f);
                    float y2 = (TargetExtentSymbols.BeaconTargets.RangeExtent / 32f) / scale;

                    target.SizeF = new SizeF(targetHypotenuse * 2, targetHypotenuse * 2 );

                    GL.PushMatrix();
                    angle = (float)(-(location.BearingTo(radar.Location) + 360) % 360) + (float)ScreenRotation;
                    GL.Translate(target.LocationF.X, target.LocationF.Y, 0.0f);
                    GL.Rotate(angle, 0.0f, 0.0f, 1.0f);
                    GL.Ortho(-1.0f, 1.0f, -1.0f, 1.0f, 0.1f, 0.0f);

                    GL.Begin(PrimitiveType.Polygon);
                        
                    GL.Color4(primarycolor);
                    GL.Vertex2(x1, y1);
                    GL.Vertex2(-x1, y1);
                    GL.Vertex2(-x1, -y1);
                    GL.Vertex2(x1, -y1);
                    GL.End();
                    if (!target.ParentAircraft.PrimaryOnly && !history)
                    {
                        GL.Begin(PrimitiveType.Polygon);
                        GL.Color4(beaconcolor);
                        GL.Vertex2(x2, y2 - beaconoffset);
                        GL.Vertex2(-x2, y2 - beaconoffset);
                        GL.Vertex2(-x2, -y2 - beaconoffset);
                        GL.Vertex2(x2, -y2 - beaconoffset);
                        GL.End();
                    }

                    GL.Translate(-target.LocationF.X, -target.LocationF.Y, 0.0f);


                    GL.PopMatrix();
                    break;
                case RadarType.FUSED:
                    float size;
                    if (!target.ParentAircraft.PrimaryOnly && !history)
                    {
                        size = (float)TargetExtentSymbols.TargetWidth(target.ParentAircraft, radar, scale, pixelScale);
                        target.SizeF = new SizeF(size, size);
                        DrawCircle(target.LocationF.X, target.LocationF.Y, size / 2, 1, 30, primarycolor, true);
                        
                    }
                    else if (!history)
                    {
                        targetWidth = TargetExtentSymbols.MultiRadarTargets.UncorrSymbolPlotSize * pixelScale;
                        targetHypotenuse = (float)(Math.Sqrt((targetWidth * targetWidth) + (targetWidth * targetWidth)) / 2);
                        x1 = (float)(Math.Sqrt(2) * targetHypotenuse);
                        target.SizeF = new SizeF(targetHypotenuse * 2, targetHypotenuse * 2);
                        GL.PushMatrix();

                        angle = 0; // -(float)ScreenRotation;
                        GL.Translate(target.LocationF.X, target.LocationF.Y, 0.0f);
                        GL.Rotate(angle, 0.0f, 0.0f, 1.0f);
                        GL.Ortho(-1.0f, 1.0f, -1.0f, 1.0f, 0.1f, 0.0f);
                        GL.Begin(PrimitiveType.Polygon);

                        GL.Color4(AdjustedColor(ReturnColor, CurrentPrefSet.Brightness.PrimaryTargets));
                        GL.Vertex2(x1, x1);
                        GL.Vertex2(-x1, x1);
                        GL.Vertex2(-x1, -x1);
                        GL.Vertex2(x1, -x1);


                        GL.End();
                        GL.Translate(-target.LocationF.X, -target.LocationF.Y, 0.0f);


                        GL.PopMatrix();
                    }
                    else
                    {
                        size = (float)TargetExtentSymbols.FMATargetSymbols.Radius * pixelScale;
                        target.SizeF = new SizeF(size, size);
                        DrawCircle(target.LocationF.X, target.LocationF.Y, size, 1, 30, AdjustedColor(target.ForeColor, CurrentPrefSet.Brightness.History), true);
                    }
                    break;
            }
            



        }

        private void DrawTargets()
        {
            List<Aircraft> aclist;
            lock (Aircraft)
            {
                aclist = Aircraft.Where(x=> x.LastMessageTime > CurrentTime.AddSeconds(-LostTargetSeconds)).ToList();
            }
            aclist.Where(x => x.PositionInd == ThisPositionIndicator).ToList().ForEach(x => x.Owned = true);
            aclist.Where(x => x.TPA != null || x.ATPAFollowing != null).ToList().ForEach(x => DrawTPA(x));
            foreach (var handoffPlane in aclist.Where(x => x.PendingHandoff == ThisPositionIndicator))
            {
                if (handoffPlane.Owned && handoffPlane.DataBlock.Flashing)
                    continue;
                handoffPlane.Owned = true;
                handoffPlane.DataBlock.Flashing = true;
                handoffPlane.DataBlock2.Flashing = true;
                //if (handoffPlane.LastPositionTime > CurrentTime.AddSeconds(-LostTargetSeconds))
                    //GenerateDataBlock(handoffPlane);
            }
            foreach (var handedoffPlane in aclist.Where(x => x.PositionInd == x.PendingHandoff))
            {
                if (handedoffPlane.PendingHandoff != null)
                    handedoffPlane.PendingHandoff = null;
                if (handedoffPlane.DataBlock.Flashing)
                {
                    handedoffPlane.DataBlock.Flashing = false;
                    handedoffPlane.DataBlock2.Flashing = false;
                    handedoffPlane.DataBlock3.Flashing = false;
                    //if (handedoffPlane.LastPositionTime > CurrentTime.AddSeconds(-LostTargetSeconds))
                        //GenerateDataBlock(handedoffPlane);
                }
            }
            foreach (var flashingPlane in aclist.Where(x => x.DataBlock.Flashing))
            {
                if (flashingPlane.PendingHandoff != ThisPositionIndicator)
                {
                    flashingPlane.DataBlock.Flashing = false;
                    flashingPlane.DataBlock2.Flashing = false;
                    flashingPlane.DataBlock3.Flashing = false;
                    //if (flashingPlane.LastPositionTime > CurrentTime.AddSeconds(-LostTargetSeconds))
                        //GenerateDataBlock(flashingPlane);
                }
            }
            foreach (var beaconatorplane in aclist.Where(x=> x.ShowCallsignWithNoSquawk != showAllCallsigns && x.LocationF.X != 0))
            {
                beaconatorplane.ShowCallsignWithNoSquawk = showAllCallsigns;
                //GenerateDataBlock(beaconatorplane);
                lock (beaconatorplane) beaconatorplane.RedrawDataBlock(radar);
            }
            aclist.ForEach(x =>
            {
                if ((!x.PrimaryOnly || x.Associated))
                {
                    lock (x.History)
                    {
                        for (var i = x.History.Length - 1; i >= 0; i--)
                        {
                            if (x.History[i] != null && x.History[i].ParentAircraft == null)
                                x.History[i].ParentAircraft = x;
                            if (x.History[i] != null)
                                DrawTarget(x.History[i]);
                        }
                    }
                }
            });
            aclist.ForEach(x =>
            {
                if (x.TargetReturn.ParentAircraft == null)
                    x.TargetReturn.ParentAircraft = x;
                DrawTarget(x.TargetReturn);
            });
            lock (posIndicators)
                posIndicators.ForEach(x => { if (!x.ParentAircraft.FDB && aclist.Contains(x.ParentAircraft)) DrawLabel(x); });
            lock (dataBlocks)
            {
                foreach (var block in dataBlocks.Where(x => x.ParentAircraft != null && (x.ParentAircraft.FDB || x.ParentAircraft.Associated || !x.ParentAircraft.PrimaryOnly)).ToList().OrderBy(x => x.ParentAircraft.FDB).ThenBy(x => x.ParentAircraft.Owned))
                {
                    if (block.ParentAircraft == debugPlane)
                        debugPlane = null; 
                    if (block.ParentAircraft != null && !aclist.Contains(block.ParentAircraft))
                    {
                        dataBlocks.Remove(block);
                        dataBlocks.Remove(block.ParentAircraft.DataBlock2);
                        dataBlocks.Remove(block.ParentAircraft.DataBlock3);
                        continue;
                    }
                    if (CurrentPrefSet.PTLLength > 0 && (block.ParentAircraft.ShowPTL || (block.ParentAircraft.Owned && CurrentPrefSet.PTLOwn) || (block.ParentAircraft.FDB && CurrentPrefSet.PTLAll)))
                    {
                        DrawLine(block.ParentAircraft.PTL, AdjustedColor(RBLColor, CurrentPrefSet.Brightness.Tools));
                    }
                    if (ClockPhase.Phase == 0)
                        DrawLabel(block);
                    else if (ClockPhase.Phase == 1)
                        DrawLabel(block.ParentAircraft.DataBlock2);
                    else if (ClockPhase.Phase == 2)
                        DrawLabel(block.ParentAircraft.DataBlock3);
                }
            }
            lock (posIndicators)
                posIndicators.ForEach(x => 
                { 
                    if (x.ParentAircraft.FDB && aclist.Contains(x.ParentAircraft)) 
                        DrawLabel(x); 
                });
        }
        private void DrawLabel(TransparentLabel Label)
        {
            /*if (!Aircraft.Contains(Label.ParentAircraft))
                return;*/
            lock (Label)
            {
                bool outline = false;
                if (string.IsNullOrEmpty(Label.Text))
                    return;
                if (Label.ParentAircraft != null && Label == Label.ParentAircraft.PositionIndicator) 
                {
                    outline = true;

                }
                GL.Enable(EnableCap.Texture2D);
                if (Label.TextureID == 0)
                    Label.TextureID = GL.GenTexture();
                var text_texture = Label.TextureID;
                Color color = Label.DrawColor;

                if (Label.Redraw)
                {
                    Label.Font = Font;
                    Bitmap text_bmp = Label.NewTextBitmap(outline);
                    var realWidth = (float)text_bmp.Width * pixelScale;
                    var realHeight = (float)text_bmp.Height * pixelScale;
                    Label.SizeF = new SizeF(realWidth, realHeight);
                    GL.BindTexture(TextureTarget.Texture2D, text_texture);
                    BitmapData data = text_bmp.LockBits(new Rectangle(0, 0, text_bmp.Width, text_bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                    text_bmp.UnlockBits(data);

                    //text_bmp.Save($"{text_texture}.bmp");
                }
                if (Label.ParentAircraft != null)
                {
                    if (Label.ParentAircraft.Owned && Label == Label.ParentAircraft.PositionIndicator)
                    {
                        color = AdjustedColor(Label.DrawColor, CurrentPrefSet.Brightness.PositionSymbols);
                    }
                    else if (Label.ParentAircraft.Owned)
                    {
                        color = AdjustedColor(Label.DrawColor, CurrentPrefSet.Brightness.FullDataBlocks);
                    }
                    else if (Label.ParentAircraft.FDB)
                    {
                        color = AdjustedColor(Label.DrawColor, CurrentPrefSet.Brightness.OtherFDBs);
                    }
                    else
                    {
                        color = AdjustedColor(Label.DrawColor, CurrentPrefSet.Brightness.LimitedDataBlocks);
                    }
                    if (Label.ParentAircraft.LocationF.X != 0 || Label.ParentAircraft.LocationF.Y != 0)
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
                    
                }
            
                GL.BindTexture(TextureTarget.Texture2D, text_texture);
            
                var Location = Label.LocationF;
                var x = RoundUpToNearest(Location.X, pixelScale);
                var y = RoundUpToNearest(Location.Y, pixelScale);
                var width = Label.SizeF.Width;
                var height = Label.SizeF.Height;
                var w1 = Label.SizeF.Width + (4 * pixelScale);
                var h1 = Label.SizeF.Height + (4 * pixelScale);
                var x1 = x - (2 * pixelScale);
                var y1 = y - (2 * pixelScale);
                
                /*GL.Begin(PrimitiveType.Quads);
                GL.Color4(Color.Black);
                GL.TexCoord2(0, 0);
                GL.Vertex2(x1, h1 + y1);
                GL.TexCoord2(1, 0);
                GL.Vertex2(w1 + x1, h1 + y1);
                GL.TexCoord2(1, 1);
                GL.Vertex2(w1 + x1, y1);
                GL.TexCoord2(0, 1);
                GL.Vertex2(x1, y1);
                GL.End();*/

                GL.Begin(PrimitiveType.Quads);
                GL.Color3(color);
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
                GL.Color4(color);
            }
            if (Label.ParentAircraft != null && CurrentPrefSet.LeaderLength > 0)
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
            return passednumber;
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
