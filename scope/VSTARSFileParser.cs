using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace scope
{
    public static class VSTARSFileParser
    {
        public static List<VideoMap> GetMapsFromFile (string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(VSTARSElements));
            // Declare an object variable of the type to be deserialized.
            VSTARSElements i;

            using (Stream reader = new FileStream(filename, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                i = (VSTARSElements)serializer.Deserialize(reader);
            }
            return (from map in i select map.Map).ToList();
        }
    }
    [XmlRoot("VideoMaps")]
    public class VSTARSElements : List<VSTARSVideoMap> { }
    [XmlRoot("VideoMap")]
    public class VSTARSVideoMap
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string STARSGroup { get; set; }
        public bool STARSTDMOnly { get; set; }
        public bool VisibleInList { get; set; }
        public List<NamedColor> Colors { get; set; }
        public List<VSTARSLine> Elements { get; set; }
        public List<Line> Lines 
        { 
            get
            {
                return (from element in Elements select element.Line).ToList();
            } 
        }
        public VideoMap Map
        {
            get
            {
                return new VideoMap() { Name = LongName, Lines = Lines };
            }
        }
    }

    [XmlRoot("Color")]
    public class NamedColor
    {
        public string Name { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
        public Color Color
        {
            get
            {
                return Color.FromArgb(Red, Green, Blue);
            }
        }
    }

    public class VSTARSLine
    {
        [XmlIgnore]
        public Color LineColor { get; private set; }
        private string colorname;
        public string Color
        {
            get
            {
                return colorname;
            }
            set
            {
                colorname = value;

            }
        }
        public double StartLon 
        { 
            get
            {
                return Line.End1.Longitude;
            }
            set
            {
                Line.End1.Longitude = value;
            }
        }
        public double StartLat
        {
            get
            {
                return Line.End1.Latitude;
            }
            set
            {
                Line.End1.Latitude = value;
            }
        }
        public double EndLong
        {
            get
            {
                return Line.End2.Longitude;
            }
            set
            {
                Line.End2.Longitude = value;
            }
        }
        public double EndLat
        {
            get
            {
                return Line.End2.Latitude;
            }
            set
            {
                Line.End2.Latitude = value;
            }
        }
        public Line Line { get; set; } = new Line();
        public string Style { get; set; }
        public int Thickness { get; set; }
    }
}
