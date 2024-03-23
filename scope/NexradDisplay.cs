using NexradDecoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Xml.Serialization;

namespace DGScope
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class NexradDisplay
    {
        public WxRadarMode WxRadarMode { get; set; } = WxRadarMode.NWSNexrad;
        private Dictionary<string, ScopeServerWxRadarReport> scopeServerRadars = new Dictionary<string, ScopeServerWxRadarReport>();
        [DisplayName("Color Table"), Description("Weather Radar value to color mapping table")]
        public List<WXColor> ColorTable { get; set; } = new List<WXColor>();
        
        [Browsable(false)]
        public string Name { get; set; }
        bool enabled = false;
        public bool Enabled { 
            get
            {
                return enabled;
            }
            set 
            {
                if (timer == null && !enabled && value)
                    timer = new System.Threading.Timer(new System.Threading.TimerCallback(cbTimerElapsed), null, 0, DownloadInterval * 1000);
                else if (!enabled && value)
                    timer.Change(0, DownloadInterval * 1000);
                enabled = value;
            }
        }
        [Description("URL for NWS Radar Product File")]
        public string URL { get; set; }
        [Description("Product Download Interval (sec.)")]
        public int DownloadInterval { get; set; } = 300;
        public string SensorID { get; set; }
        [Browsable(false)]
        [XmlIgnore]
        public bool[] LevelsAvailable = new bool[6];
        public bool[] LevelsEnabled => colortable == null ? dummyLevelsE : colortable.LevelsEnabled;
        RadialPacketDecoder decoder = new RadialPacketDecoder();
        RadialSymbologyBlock symbology;
        DescriptionBlock description;
        bool[] dummyLevelsE = new bool[6];
        bool[] dummyLevelsA = new bool[6];
        bool gotdata = false;
        void GetRadarData(string url)
        {
            if (!Enabled)
                return;
            if (string.IsNullOrEmpty(url))
                return;
            using (var client = new WebClient())
            {
                try
                {
                    Debug.WriteLine("Downloading radar data from " + url);
                    Stream response = client.OpenRead(url);
                    MemoryStream stream = new MemoryStream();
                    response.CopyTo(stream);
                    decoder.setStreamResource(stream);
                    decoder.parseMHB();
                    description = decoder.parsePDB();
                    symbology = (RadialSymbologyBlock)decoder.parsePSB();
                    gotdata = true;
                }
                catch (Exception ex) 
                {

                }
            }
            //decoder.setFileResource("e:\\users\\dennis\\downloads\\KOKX_SDUS51_N0ROKX_202009292354");

            RecomputeVertices();
            
        }
        public NexradDisplay() 
        {

            
        }
        System.Threading.Timer timer;
        private void cbTimerElapsed(object state)
        {
            GetRadarData(URL);
        }
        bool recompute = true;
        public Polygon[] Polygons()
        {
            if (timer == null)
                timer = new Timer(new TimerCallback(cbTimerElapsed), null,0,DownloadInterval * 1000);
            
            if (polygons == null || !Enabled)
                return new Polygon[0];
            return polygons;
        }
        
        Polygon[] polygons;
        
        GeoPoint _center = new GeoPoint();
        double _scale;
        double _rotation;

        
        private static WXColorTable colortable;
        public void RecomputeVertices()
        {
            if (ColorTable.Count == 0)
            {
                ColorTable = new List<WXColor>()
                {
                    new WXColor(20, Color.FromArgb(38, 77, 77)),
                    new WXColor(30, Color.FromArgb(38, 77, 77), Color.White, WXColor.StippleType.LIGHT),
                    new WXColor(40, Color.FromArgb(38, 77, 77), Color.White, WXColor.StippleType.DENSE),
                    new WXColor(45, Color.FromArgb(100, 100, 51)),
                    new WXColor(50, Color.FromArgb(100, 100, 51), Color.White, WXColor.StippleType.LIGHT),
                    new WXColor(55, Color.FromArgb(100, 100, 51), Color.White, WXColor.StippleType.DENSE)
                };
            }
            if (colortable == null)
            {
                colortable = new WXColorTable(ColorTable);
            }
            if (!gotdata && WxRadarMode == WxRadarMode.NWSNexrad)
                return;
            var polygons = new List<Polygon>();
            GeoPoint radarLocation;
            bool[] availableLevels = new bool[6];
            switch (WxRadarMode)
            {
                case WxRadarMode.NWSNexrad:
                    radarLocation = new GeoPoint(description.Latitude, description.Longitude);
                    int range;
                    switch (description.Code)
                    {
                        case 94:
                            range = 248;
                            break;
                        case 180:
                            range = 48;
                            break;
                        default:
                            return;
                    }
                    var scanrange = range * Math.Cos(description.ProductSpecific_3 * (Math.PI / 180));
                    double resolution = scanrange / symbology.LayerNumberOfRangeBins;
                    for (int i = 0; i < symbology.NumberOfRadials; i++)
                    {
                        for (int j = 0; j < symbology.Radials[i].ColorValues.Length; j++)
                        {
                            if (symbology.Radials[i].ColorValues[j] > 0)
                            {
                                Polygon polygon = new Polygon();
                                polygon.Points.Add(radarLocation.FromPoint(resolution * j, symbology.Radials[i].StartAngle));
                                polygon.Points.Add(radarLocation.FromPoint(resolution * j, symbology.Radials[i].StartAngle + symbology.Radials[i].AngleDelta));
                                polygon.Points.Add(radarLocation.FromPoint(resolution * (j + 1), symbology.Radials[i].StartAngle + symbology.Radials[i].AngleDelta));
                                polygon.Points.Add(radarLocation.FromPoint(resolution * (j + 1), symbology.Radials[i].StartAngle));
                                //var color = Colors[radial.ColorValues[j]];
                                var color = colortable.GetWXColor(symbology.Radials[i].Values[j], ref availableLevels);
                                if (color != null)
                                {
                                    polygon.Color = color.MinColor;
                                    if (color.StippleColor != null)
                                        polygon.StippleColor = color.StippleColor;
                                    if (color.StipplePattern != null)
                                        polygon.StipplePattern = color.StipplePattern;
                                    polygon.ComputeVertices();
                                    polygons.Add(polygon);
                                }
                            }
                        }
                    }
                    LevelsAvailable = availableLevels;
                    break;
                case WxRadarMode.ScopeServer:
                    if (SensorID is null)
                    {
                        break;
                    }
                    if (scopeServerRadars.TryGetValue(SensorID, out var radar))
                    {
                        const double METER_TO_NM = 0.000539957;
                        double rotation = (double)radar.Rotation;
                        radarLocation = radar.ReferencePoint.FromPoint(radar.OriginOffset.Y * METER_TO_NM, 360 + rotation).FromPoint(radar.OriginOffset.X * METER_TO_NM, 90 + rotation);
                        GeoPoint rowStart = radarLocation.FromPoint(radar.GridSize.Y * radar.BoxSize.Y * METER_TO_NM, 360 + rotation);
                        for (int y = 0; y < radar.Values.Length; y++)
                        {
                            var row = radar.Values[y];
                            var lastPoint = rowStart;
                            for (int x = 0; x < row.Length; x++)
                            {
                                Polygon polygon = new Polygon();
                                polygon.Points.Add(lastPoint);
                                polygon.Points.Add(lastPoint.FromPoint(radar.BoxSize.Y * METER_TO_NM, rotation));
                                polygon.Points.Add(lastPoint.FromPoint(radar.BoxSize.Y * METER_TO_NM, rotation).FromPoint(radar.BoxSize.X * METER_TO_NM, 90 + rotation));
                                lastPoint = lastPoint.FromPoint(radar.BoxSize.X * METER_TO_NM, 90 + rotation);
                                polygon.Points.Add(lastPoint);
                                var color = colortable.GetWXColor(radar.Values[y][x], ref availableLevels);
                                if (color != null)
                                {
                                    polygon.Color = color.MinColor;
                                    if (color.StippleColor != null)
                                        polygon.StippleColor = color.StippleColor;
                                    if (color.StipplePattern != null)
                                        polygon.StipplePattern = color.StipplePattern;
                                    polygon.ComputeVertices();
                                    polygons.Add(polygon);
                                }
                            }
                            rowStart = rowStart.FromPoint(radar.BoxSize.Y * METER_TO_NM, 180 + rotation);
                        }
                    }
                    break;
            }
            LevelsAvailable = availableLevels;
            this.polygons = polygons.ToArray();
        }

        public void AddWeatherRadarReport(string RadarID, ScopeServerWxRadarReport report)
        {
            lock (scopeServerRadars)
            {
                if (!scopeServerRadars.ContainsKey(RadarID))
                {
                    scopeServerRadars.Add(RadarID, report);
                }
                else
                {
                    scopeServerRadars[RadarID] = report;
                }
            }
            RecomputeVertices();
        }
        public override string ToString()
        {
            return Name;
        }

        // ftp://tgftp.nws.noaa.gov/SL.us008001/DF.of/DC.radar/DS.p19r0/SI.kokx/sn.last
    }

    public class Polygon
    {
        public PointF[] vertices;
        public List<GeoPoint> Points { get; set; } = new List<GeoPoint>();
        public Color Color { get; set; }
        public Color StippleColor { get; set; }
        public byte[] StipplePattern { get; set; }

        public void ComputeVertices()
        {
            GeoPoint[] points;
            lock (Points)
            {
                points = Points.ToArray();
            }
            vertices = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vertices[i].X = (float)points[i].Longitude; // (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                vertices[i].Y = (float)points[i].Latitude; // (Math.Cos(bearing * (Math.PI / 180)) * (distance / scale));
            }
        }
    }
    public enum WxRadarMode
    {
        NWSNexrad,
        ScopeServer
    }
    public struct ScopeServerWxRadarReport
    {
        public GeoPoint ReferencePoint { get; set; }
        public Vector2 OriginOffset { get; set; }
        public Vector2 BoxSize { get; set; }
        public Vector2 GridSize { get; set; }
        public byte[][] Values { get; set; }
        public decimal Rotation { get; set; }
    }
}
