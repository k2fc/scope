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

        public List<ATPAVolume> ActiveVolumes => Volumes.Where(x => x.Active).ToList();
        private bool calculating = false;
        public async Task Calculate(List<Aircraft> aircraft)
        {
            if (!calculating)
            {
                calculating = true;
                Task[] tasks;
                lock (Volumes)
                {
                    List<Task> tasklist = new List<Task>();
                    foreach (var volume in ActiveVolumes)
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
                                    volume.LDRDirection = (int)lld;
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
