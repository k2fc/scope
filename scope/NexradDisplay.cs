using NexradDecoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

namespace DGScope
{
    public class NexradDisplay
    {
        [DisplayName("Color Table"), Description("Weather Radar value to color mapping table")]
        public List<WXColor> ColorTable { get; set; } = new List<WXColor>();
        double intensity = 1;
        [DisplayName("Color Intensity"), Description("Weather Radar Color intensity")]
        public int ColorIntensity {
            get
            {
                return (int)(255 * intensity);
            }
            set
            {
                if (value > 255)
                    intensity = 1;
                else if (value < 0)
                    intensity = 0;
                else
                    intensity = value / 255d;
            }
        }
        double alphafactor = .12156;
        [DisplayName("Transparency"), Description("Weather Radar Transparency")]
        public int Transparency 
        { 
            get
            {
                return (int)(255 * (1 - alphafactor));
            }
            set
            {
                if (value > 255)
                    alphafactor = 0;
                else if (value < 0)
                    alphafactor = 1;
                else
                    alphafactor = 1 - (value / 255d);
            }
        }
        public string Name { get; set; }
        bool enabled = true;
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
        public string URL { get; set; }
        public int DownloadInterval { get; set; } = 300;
        public int Range { get; set; } = 248;
        RadialPacketDecoder decoder = new RadialPacketDecoder();
        RadialSymbologyBlock symbology;
        DescriptionBlock description;
        bool gotdata = false;
        void GetRadarData(string url)
        {
            if (!Enabled)
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

            //RecomputeVertices(_center, _scale, _rotation);
            if (!recomputeVerticesThread.IsAlive)
            {
                recomputeVerticesThread.Start();
            }
            recompute = true;
        }
        Thread recomputeVerticesThread;
        public NexradDisplay() 
        {
            recomputeVerticesThread = new Thread(new ThreadStart(RecomputeVertices));
            recomputeVerticesThread.IsBackground = true;
        }
        System.Threading.Timer timer;
        private void cbTimerElapsed(object state)
        {
            GetRadarData(URL);
        }
        bool recompute = true;
        public Polygon[] Polygons(GeoPoint center, double scale, double rotation = 0)
        {
            if (_center != center || _scale != scale || _rotation != rotation)
            {
                _center = center;
                _scale = scale;
                _rotation = rotation;
                recompute = true;
            }
            if (timer == null)
                timer = new Timer(new TimerCallback(cbTimerElapsed), null,0,DownloadInterval * 1000);
            
            if (polygons == null || !Enabled)
                return new Polygon[0];
            return polygons;
        }
        
        public void RescalePolygons(float scalechange, float ar_change)
        {
            if (polygons == null)
                return;
            for (int i = 0; i < polygons.Length; i++)
            {
                for (int j = 0; j < polygons[i].vertices.Length; j++)
                {
                    polygons[i].vertices[0].X *= scalechange;
                    polygons[i].vertices[0].Y *= scalechange / ar_change;
                }
            }
        }
        public void MovePolygons(float xChange, float yChange)
        {
            if (polygons == null)
                return;
            for (int i = 0; i < polygons.Length; i++)
            {
                for (int j = 0; j < polygons[i].vertices.Length; j++)
                {
                    polygons[i].vertices[0].X += xChange;
                    polygons[i].vertices[0].Y -= yChange;
                }
            }
        }
        Polygon[] polygons;
        
        GeoPoint _center = new GeoPoint();
        double _scale;
        double _rotation;

        public void RecomputeVertices()
        {
            while(true) 
            {
                if (recompute)
                {
                    RecomputeVertices(_center, _scale, _rotation);
                }
                else
                    Thread.Sleep(100);
            }
        }
        public void RecomputeVertices(GeoPoint center, double scale, double rotation = 0)
        {
            if (ColorTable.Count == 0)
                ColorTable = new List<WXColor>()
                    {
                        new WXColor(30, Color.FromArgb(0, 255, 0)),
                        new WXColor(40, Color.FromArgb(255, 255, 0)),
                        new WXColor(50, Color.FromArgb(255, 0, 0)),
                        new WXColor(60, Color.FromArgb(255, 0, 255)),
                        new WXColor(70, Color.FromArgb(255, 255, 255)),
                        new WXColor(80, Color.FromArgb(128, 128, 128)),
                    };
            var colortable = new WXColorTable(ColorTable);
            if (!gotdata)
                return;
            _center = center;
            _scale = scale;
            _rotation = rotation;
            recompute = false;
            var polygons = new List<Polygon>();
            GeoPoint radarLocation = new GeoPoint(description.Latitude, description.Longitude);
            var scanrange = (double)Range * Math.Cos(description.ProductSpecific_3 * (Math.PI / 180 ));
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
                        var color = colortable.GetWXColor(symbology.Radials[i].Values[j]);
                        if (color != null)
                        {
                            polygon.Color = Color.FromArgb((int)(color.MinColor.A * alphafactor), (int)(color.MinColor.R * intensity), (int)(color.MinColor.G * intensity),
                                (int)(color.MinColor.B * intensity));
                            if (color.StippleColor != null)
                                polygon.StippleColor = Color.FromArgb((int)(color.StippleColor.A * alphafactor), (int)(color.StippleColor.R * intensity),
                                    (int)(color.StippleColor.G * intensity), (int)(color.StippleColor.B * intensity));
                            if (color.StipplePattern != null)
                                polygon.StipplePattern = color.StipplePattern;
                            polygon.ComputeVertices(center, scale, rotation);
                            polygons.Add(polygon);
                        }
                    }
                }
            }
            this.polygons = polygons.ToArray();
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

        public void ComputeVertices(GeoPoint center, double scale, double ScreenRotation = 0)
        {
            GeoPoint[] points;
            lock (Points)
            {
                points = Points.ToArray();
            }
            vertices = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                double bearing = center.BearingTo(points[i]) - ScreenRotation;
                double distance = center.DistanceTo(points[i]);
                vertices[i].X = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                vertices[i].Y = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale));
            }
        }
    }
}
