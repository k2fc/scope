using NexradDecoder;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace DGScope
{
    public class NexradDisplay
    {
        [XmlIgnore]
        [DisplayName("Colors"), Description("Weather Radar Colors by Value")]
        public Color[] Colors { get; set; } = new Color[16];
        [XmlElement("Colors")]
        [Browsable(false)]
        public int[] ColorsAsArgb
        {
            get 
            {
                var colors = new int[Colors.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Colors[i].ToArgb();
                }
                return colors; 
            }
            set 
            {
                Colors = new Color[value.Length];
                for (int i = 0; i < Colors.Length; i++)
                {
                    Colors[i] = Color.FromArgb(value[i]);
                }
            }
        }
        public string URL { get; set; }
        public int DownloadInterval { get; set; } = 300;
        public int Range { get; set; } = 124;
        RadialPacketDecoder decoder = new RadialPacketDecoder();
        SymbologyBlock symbology;
        DescriptionBlock description;
        void GetRadarData(string url)
        {
            //var client = new WebClient();
            //Stream response = client.OpenRead("file://e/users/dennis/downloads/KOKX_SDUS51_N0ROKX_202009292354");
            decoder.setFileResource("e:\\users\\dennis\\downloads\\KOKX_SDUS51_N0ROKX_202009292354");
            decoder.parseMHB();
            description = decoder.parsePDB();
            symbology = decoder.parsePSB();
            polygons = Polygons((RadialSymbologyBlock)symbology);
        }

        public NexradDisplay() 
        {
            GetRadarData("REMOVE THIS DENNIS");
        }

        public Polygon[] Polygons()
        {
            GetRadarData("REMOVE THIS DENNIS");
            return polygons;
        }
        Polygon[] polygons;
        Polygon[] Polygons (RadialSymbologyBlock block)
        {
            GeoPoint radarLocation = new GeoPoint(description.Latitude, description.Longitude);
            var polygons = new List<Polygon>();
            double resolution = (double)Range / block.LayerNumberOfRangeBins;
            for (int i = 0; i < block.NumberOfRadials; i++)
            {
                var radial = block.Radials[i];
                for (int j = 0; j < radial.ColorValues.Length; j++)
                {
                    Polygon polygon = new Polygon();
                    polygon.Points.Add(radarLocation.FromPoint(resolution * j, radial.StartAngle));
                    polygon.Points.Add(radarLocation.FromPoint(resolution * j, radial.StartAngle + radial.AngleDelta));
                    polygon.Points.Add(radarLocation.FromPoint(resolution * (j + 1), radial.StartAngle + radial.AngleDelta));
                    polygon.Points.Add(radarLocation.FromPoint(resolution * (j + 1), radial.StartAngle));
                    polygon.Color = Colors[radial.ColorValues[j]];
                    polygons.Add(polygon);
                }
            }
            return polygons.ToArray();
        }

        // ftp://tgftp.nws.noaa.gov/SL.us008001/DF.of/DC.radar/DS.p19r0/SI.kokx/sn.last
    }

    public class Polygon
    {
        public List<GeoPoint> Points { get; set; } = new List<GeoPoint>();
        public Color Color { get; set; }
    }
}
