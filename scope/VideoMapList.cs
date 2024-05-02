using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

using System.Linq;
using System.IO;

namespace DGScope
{
    public class VideoMapList : List<VideoMap>
    {
        [XmlIgnore]
        public string Filename { get; set; }
        public VideoMapList() : base() { }
        public VideoMapList(object[] maps) : base() 
        {
            foreach (var item in maps)
            {
                if (item.GetType() != typeof(VideoMap))
                    throw new InvalidDataException("Passed object was type " + item.GetType().ToString() + " and not a Video Map.");
                Add(item as VideoMap);
            }
        }

        public VideoMapList(string filename) : base()
        {
            Filename = filename;
        }
        public new void Add(VideoMap map) 
        {
            while (Contains(map))
            {
                map.Number++;
            }
            base.Add(map);
        }

        public new void AddRange(IEnumerable<VideoMap> collection)
        {
            foreach (var map in collection)
            {
                Add(map);
            }
        }
        public static void SerializeToJsonFile(VideoMapList videoMaps, string filename)
        {
            File.WriteAllText(filename, SerializeToJson(videoMaps));
        }
        public static string SerializeToJson(VideoMapList videoMaps)
        {
            return GeoJSONMapExporter.MapsToGeoJSON(videoMaps);
        }
        public static VideoMapList DeserializeFromJsonFile(string filename)
        {
            string json = File.ReadAllText(filename);
            return GeoJSONMapExporter.GeoJSONToMaps(json);
        }
        public static VideoMapList DeserializeFromJson(string jsonString)
        {
            return GeoJSONMapExporter.GeoJSONToMaps(jsonString);
        }
    }
}
