using NexradDecoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace DGScope
{
    public class NexradDisplay
    {
        [DisplayName("Color Table"), Description("Weather Radar value to color mapping table")]
        public List<WXColor> ColorTable { get; set; } 
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
        double alphafactor = .5;
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
                if (!enabled && value)
                    timer.Change(0, DownloadInterval * 1000);
                enabled = value;
            }
        }
        public string URL { get; set; }
        public int DownloadInterval { get; set; } = 300;
        public int Range { get; set; } = 124;
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
                
            RecomputeVertices(_center, _scale, _rotation);
        }

        public NexradDisplay() 
        {
            
        }
        System.Threading.Timer timer;
        private void cbTimerElapsed(object state)
        {
            GetRadarData(URL);
        }
        public Polygon[] Polygons(GeoPoint center, double scale, double rotation = 0)
        {
            if (_center != center || _scale != scale || _rotation != rotation)
            {
                _center = center;
                _scale = scale;
                _rotation = rotation;
                RecomputeVertices(center, scale, rotation);
            }
            if (timer == null)
                timer = new System.Threading.Timer(new System.Threading.TimerCallback(cbTimerElapsed), null,0,DownloadInterval * 1000);
            
            if (polygons == null || !Enabled)
                return new Polygon[0];
            return polygons;
        }
        Polygon[] polygons;
        
        GeoPoint _center = new GeoPoint();
        double _scale;
        double _rotation;
        public void RecomputeVertices(GeoPoint center, double scale, double rotation = 0)
        {
            if (ColorTable.Count == 0)
                GenerateDefaultColorTable();
            var colortable = new WXColorTable(ColorTable);
            if (!gotdata)
                return;
            _center = center;
            _scale = scale;
            _rotation = rotation;
            var polygons = new List<Polygon>();
            GeoPoint radarLocation = new GeoPoint(description.Latitude, description.Longitude);
            var scanrange = (double)Range * Math.Cos(description.ProductSpecific_3 * (Math.PI / 180 ));
            double resolution = scanrange / symbology.LayerNumberOfRangeBins;
            for (int i = 0; i < symbology.NumberOfRadials; i++)
            {
                var radial = symbology.Radials[i];
                for (int j = 0; j < radial.ColorValues.Length; j++)
                {
                    Polygon polygon = new Polygon();
                    polygon.Points.Add(radarLocation.FromPoint(resolution * j, radial.StartAngle));
                    polygon.Points.Add(radarLocation.FromPoint(resolution * j, radial.StartAngle + radial.AngleDelta));
                    polygon.Points.Add(radarLocation.FromPoint(resolution * (j + 1), radial.StartAngle + radial.AngleDelta));
                    polygon.Points.Add(radarLocation.FromPoint(resolution * (j + 1), radial.StartAngle));
                    //var color = Colors[radial.ColorValues[j]];
                    var color = colortable.GetColor(radial.Values[j]);
                    polygon.Color = Color.FromArgb((int)(color.A * alphafactor), (int)(color.R * intensity), (int)(color.G * intensity), (int)(color.B * intensity));
                    polygon.ComputeVertices(center, scale, rotation);
                    polygons.Add(polygon);
                    
                }
            }
            this.polygons = polygons.ToArray();
        }

        public void GenerateDefaultColorTable()
        {
            ColorTable = new List<WXColor>()
            {
                new WXColor(10, Color.FromArgb(164,164,255), Color.FromArgb(100,100,192)),
                new WXColor(20, Color.FromArgb(64,128,255),  Color.FromArgb(32,64,128)),
                new WXColor(30, Color.FromArgb(0,255,0),   Color.FromArgb(0,128,0)),
                new WXColor(40, Color.FromArgb(255,255,0),   Color.FromArgb(255,128,0)),
                new WXColor(50, Color.FromArgb(255,0,0),   Color.FromArgb(160,0,0)),
                new WXColor(60, Color.FromArgb(255,0,255),   Color.FromArgb(128,0,128)),
                new WXColor(70, Color.FromArgb(255,255,255), Color.FromArgb(128,128,128)),
                new WXColor(80, Color.FromArgb(128,128,128), Color.FromArgb(128,128,128)),
            };
        }

        public override string ToString()
        {
            return Name;
        }

        // ftp://tgftp.nws.noaa.gov/SL.us008001/DF.of/DC.radar/DS.p19r0/SI.kokx/sn.last
    }

    public class Polygon
    {
        public float[][] vertices = new float[2][];
        public List<GeoPoint> Points { get; set; } = new List<GeoPoint>();
        public Color Color { get; set; }

        public void ComputeVertices(GeoPoint center, double scale, double ScreenRotation = 0)
        {
            GeoPoint[] points;
            lock (Points)
            {
                points = Points.ToArray();
            }
            vertices[0] = new float[points.Length];
            vertices[1] = new float[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                double bearing = center.BearingTo(points[i]) - ScreenRotation;
                double distance = center.DistanceTo(points[i]);
                vertices[0][i] = (float)(Math.Sin(bearing * (Math.PI / 180)) * (distance / scale));
                vertices[1][i] = (float)(Math.Cos(bearing * (Math.PI / 180)) * (distance / scale));
            }
        }
    }
}
