using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BAMCIS.GeoJSON;

namespace DGScope
{
    public static class GeoJSONMapExporter
    {
        public static string MapsToGeoJSON(VideoMapList maps)
        {
            List<Feature> features = new List<Feature>();
            foreach (var map in maps)
            {
                Feature feature = new Feature(MapToGeometryCollection(map));
                feature.Properties.Add("name", map.Name);
                feature.Properties.Add("number", map.Number);
                feature.Properties.Add("category", map.Category);
                features.Add(feature);
            }
            FeatureCollection fc = new FeatureCollection(features);
            return fc.ToJson();
        }
        public static string MapToGeoJSON(VideoMap map)
        {
            
            FeatureCollection fc = new FeatureCollection(MapToFeatureList(map));
            return fc.ToJson();
        }

        private static List<Feature> MapToFeatureList(VideoMap map)
        {
            List<Feature> features = new List<Feature>();
            foreach (var line in map.Lines)
            {
                var lineString = LineToLineString(line);
                if (lineString != null)
                {
                    Feature feature = new Feature(lineString);
                    features.Add(feature);
                }
            }
            return features;
        }

        private static GeometryCollection MapToGeometryCollection(VideoMap map)
        {
            List<Geometry> linestrings = new List<Geometry>();
            foreach (var line in map.Lines)
            {
                var lineString = LineToLineString(line);
                if (lineString != null)
                    linestrings.Add(lineString);
            }
            return new GeometryCollection(linestrings);
        }

        private static LineString LineToLineString(Line line)
        {
            List<Position> positions = new List<Position>();
            if (Math.Abs(line.End1.Latitude) > 90 || Math.Abs(line.End2.Latitude) > 90 || Math.Abs(line.End1.Longitude) > 180 || Math.Abs(line.End2.Longitude) > 180)
                return null;
            positions.Add(new Position(line.End1.Longitude, line.End1.Latitude));
            positions.Add(new Position(line.End2.Longitude, line.End2.Latitude));
            LineString lineString = new LineString(positions);
            return lineString;
        }

        public static void MapToGeoJSONFile(VideoMap map, string filename)
        {
            File.WriteAllText(filename, MapToGeoJSON(map));
        }
        public static void MapsToGeoJSONFile(VideoMapList maps, string filename)
        {
            File.WriteAllText(filename, MapsToGeoJSON(maps));
        }

        public static VideoMapList GeoJSONFileToMaps(string path)
        {
            var json = File.ReadAllText(path);
            try
            {
                return GeoJSONToMaps(json);
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show("Error with video map " + path + "\r\n" + ex.Message); }
            return null;
        }

        public static VideoMapList GeoJSONToMaps(string json)
        {
            VideoMapList maps = new VideoMapList();
            var data = GeoJson.FromJson(json);
            switch (data.Type)
            {
                case GeoJsonType.FeatureCollection:
                    var featureCollection = data as FeatureCollection;
                    if (featureCollection.Features.Any(x => x.Geometry.Type == GeoJsonType.LineString))
                    {
                        VideoMap map = new VideoMap();
                        var fcl = featureCollection.Features.ToList();
                        foreach (var feature in featureCollection.Features.Where(x => x.Geometry != null && x.Geometry.Type == GeoJsonType.LineString))
                        {
                            var geometry = feature.Geometry as LineString;
                            var lines = LineStringToLines(geometry);
                            if (lines != null)
                            {
                                map.Lines.AddRange(lines);
                            }
                        }
                        map.Name = "Imported map - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                        if (map.Lines.Count > 0)
                        {
                            maps.Add(map);
                        }
                    }
                    else if (featureCollection.Features.Any(x => x.Geometry != null && x.Geometry.Type == GeoJsonType.GeometryCollection))
                    {
                        foreach (var feature in featureCollection.Features)
                        {
                            VideoMap newmap = new VideoMap();
                            var geometryCollection = feature.Geometry as GeometryCollection;
                            if (feature.Properties.ContainsKey("name"))
                                newmap.Name = feature.Properties["name"];
                            if (feature.Properties.ContainsKey("number"))
                                newmap.Number = (int)feature.Properties["number"];
                            if (feature.Properties.ContainsKey("category"))
                                newmap.Category = (MapCategory)(int)feature.Properties["category"];
                            foreach (var geometry in geometryCollection.Geometries)
                            {
                                if (geometry.Type != GeoJsonType.LineString)
                                    continue;
                                var lines = LineStringToLines(geometry as LineString);
                                if (lines != null)
                                {
                                    newmap.Lines.AddRange(lines);
                                }
                            }
                            if (newmap.Lines.Count > 0)
                            {
                                maps.Add(newmap);
                            }
                        }
                    }
                    break;
            }
            return maps;
        }

        private static List<Line> LineStringToLines(LineString lineString)
        {
            if (lineString == null) return null;
            var points = lineString.Coordinates.ToArray();
            var lines = new List<Line>();
            for (int i = 1; i < points.Length; i++)
            {
                var end1 = new GeoPoint(points[i].Latitude, points[i].Longitude);
                var end2 = new GeoPoint(points[i - 1].Latitude, points[i - 1].Longitude);
                lines.Add(new Line(end1, end2));
            }
            return lines;
        }
    }
}
