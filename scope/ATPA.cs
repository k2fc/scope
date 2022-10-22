using BAMCIS.GeoJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class ATPA
    {
        public SeparationTable RequiredSeparation { get; set; } = new SeparationTable();
        public List<ATPAVolume> Volumes { get; set; } = new List<ATPAVolume>();
        public List<string> ActiveVolumeNames { get; set; } = new List<string>();

        public ATPA()
        {
            // Set up 7110.126B CWT table
            // B Following
            var bbehind = new SerializableDictionary<string, double>();
            bbehind.Add("A", 5);
            bbehind.Add("B", 3);
            bbehind.Add("D", 3);
            RequiredSeparation.Add("B", bbehind);

            // C Following
            var cbehind = new SerializableDictionary<string, double>();
            cbehind.Add("A", 6);
            cbehind.Add("B", 4);
            cbehind.Add("D", 4);
            RequiredSeparation.Add("C", cbehind);

            // D Following
            var dbehind = new SerializableDictionary<string, double>();
            dbehind.Add("A", 6);
            dbehind.Add("B", 4);
            dbehind.Add("D", 4);
            RequiredSeparation.Add("D", dbehind);

            // E Following
            var ebehind = new SerializableDictionary<string, double>();
            ebehind.Add("A", 7);
            ebehind.Add("B", 5);
            ebehind.Add("C", 3.5);
            ebehind.Add("D", 5);
            RequiredSeparation.Add("E", ebehind);

            // F Following
            var fbehind = new SerializableDictionary<string, double>();
            fbehind.Add("A", 7);
            fbehind.Add("B", 5);
            fbehind.Add("C", 3.5);
            fbehind.Add("D", 5);
            RequiredSeparation.Add("F", fbehind);

            // G Following
            var gbehind = new SerializableDictionary<string, double>();
            gbehind.Add("A", 7);
            gbehind.Add("B", 5);
            gbehind.Add("C", 3.5);
            gbehind.Add("D", 5);
            RequiredSeparation.Add("G", gbehind);

            // G Following
            var hbehind = new SerializableDictionary<string, double>();
            hbehind.Add("A", 8);
            hbehind.Add("B", 5);
            hbehind.Add("C", 5);
            hbehind.Add("D", 5);
            RequiredSeparation.Add("H", hbehind);

            // I Following
            var ibehind = new SerializableDictionary<string, double>();
            ibehind.Add("A", 8);
            ibehind.Add("B", 5);
            ibehind.Add("C", 5);
            ibehind.Add("D", 5);
            ibehind.Add("E", 4);
            RequiredSeparation.Add("I", ibehind);
        }
        private bool calculating = false;
        public async Task Calculate(ICollection<Aircraft> aircraftList)
        {
            List<Aircraft> aircraft;
            lock (aircraftList)
                aircraft = aircraftList.ToList();
            if (!calculating)
            {
                calculating = true;
                Task[] tasks;
                lock (Volumes)
                {
                    List<Task> tasklist = new List<Task>();
                    foreach (var volume in Volumes)
                    {
                        tasklist.Add(CalculateATPA(volume, aircraft, RequiredSeparation));
                    }
                    tasks = tasklist.ToArray();
                }
                Task.WaitAll(tasks);
                calculating = false;
            }
        }

        private static Task CalculateATPA(ATPAVolume volume, List<Aircraft> aircraft, SeparationTable requiredSeparation)
        {
            return Task.Run(() => volume.CalculateATPA(aircraft, requiredSeparation));
        }

        public void DeserializeVolumesFromJsonFile(string filename)
        {
            Volumes = DeserializeFromJsonFile(filename);
        }
        public static List<ATPAVolume> DeserializeFromJsonFile(string filename)
        {
            string json = File.ReadAllText(filename);
            return GeoJSONToVolumes(json);
        }
        public static List<ATPAVolume> DeserializeFromJson(string jsonString)
        {
            return GeoJSONToVolumes(jsonString);
        }
        public static List<ATPAVolume> GeoJSONToVolumes(string json)
        {
            List<ATPAVolume> volumes = new List<ATPAVolume>();
            var data = GeoJson.FromJson(json);
            switch (data.Type)
            {
                case GeoJsonType.FeatureCollection:
                    var featureCollection = data as FeatureCollection;
                    foreach (var feature in featureCollection.Features)
                    {
                        switch (feature.Geometry.Type)
                        {
                            case GeoJsonType.Polygon:
                                ATPAVolume volume = new ATPAVolume();
                                if (feature.Properties.TryGetValue("name", out var name))
                                    volume.Name = name;
                                else
                                    continue;
                                if (feature.Properties.TryGetValue("mergepoint_lat", out var mplat))
                                    if (feature.Properties.TryGetValue("mergepoint_lon", out var mplon))
                                        volume.MergePoint = new GeoPoint(mplat, mplon);
                                    else
                                        continue;
                                else
                                    continue;
                                if (feature.Properties.TryGetValue("minalt", out var minalt))
                                    volume.MinAltitude = (int)minalt;
                                else
                                    continue;
                                if (feature.Properties.TryGetValue("maxalt", out var maxalt))
                                    volume.MaxAltitude = (int)maxalt;
                                else
                                    continue;
                                if (feature.Properties.TryGetValue("runway", out var runway))
                                    if (runway != "NULL" && runway != null)
                                        volume.Runway = runway;
                                if (feature.Properties.TryGetValue("destination", out var dest))
                                    if (dest != "NULL" && dest != null)
                                        volume.Destination = dest;
                                if (feature.Properties.TryGetValue("lld", out var lld))
                                    volume.LDRDirection = (int?)lld;
                                if (feature.Properties.TryGetValue("minsep", out var minsep))
                                    volume.MinimumSeparation = minsep;
                                var geometry = feature.Geometry as BAMCIS.GeoJSON.Polygon;
                                foreach (var ring in geometry.Coordinates)
                                {
                                    if (ring.Coordinates.Count() < 3)
                                        continue;
                                    foreach (var point in ring.Coordinates)
                                    {
                                        volume.Points.Add(new GeoPoint(point.Latitude, point.Longitude));
                                    }
                                }
                                volumes.Add(volume);
                                break;
                        }
                    }
                    break;
            }
            return volumes;
        }
    }

    public class SeparationTable : SerializableDictionary<string, SerializableDictionary<string, double>> { }
    public enum ATPAStatus
    {
        Monitor = 1, Caution = 2, Alert = 3
    }
}
