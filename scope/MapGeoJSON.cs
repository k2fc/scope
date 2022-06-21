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
                List<Position> positions = new List<Position>();
                positions.Add(new Position(line.End1.Longitude, line.End1.Latitude));
                positions.Add(new Position(line.End2.Longitude, line.End2.Latitude));
                LineString lineString = new LineString(positions);
                Feature feature = new Feature(lineString);
                features.Add(feature);
            }
            return features;
        }

        private static GeometryCollection MapToGeometryCollection(VideoMap map)
        {
            List<Geometry> linestrings = new List<Geometry>();
            foreach (var line in map.Lines)
            {
                List<Position> positions = new List<Position>();
                positions.Add(new Position(line.End1.Longitude, line.End1.Latitude));
                positions.Add(new Position(line.End2.Longitude, line.End2.Latitude));
                LineString lineString = new LineString(positions);
                linestrings.Add(lineString);
            }
            return new GeometryCollection(linestrings);
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
            return GeoJSONToMaps(json);
        }

        public static VideoMapList GeoJSONToMaps(string json)
        {
            VideoMapList maps = new VideoMapList();
            var data = GeoJson.FromJson(json);
            switch (data.Type)
            {
                case GeoJsonType.FeatureCollection:
                    var featureCollection = data as FeatureCollection;
                    switch (featureCollection.Features.First().Geometry.Type)
                    {
                        case GeoJsonType.LineString:
                            VideoMap map = new VideoMap();
                            foreach (var feature in featureCollection.Features)
                            {
                                var geometry = feature.Geometry as LineString;
                                map.Lines.AddRange(LineStringToLines(geometry));
                            }
                            map.Name = "Imported map - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                            maps.Add(map);
                            break;
                        case GeoJsonType.GeometryCollection:
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
                                    newmap.Lines.AddRange(LineStringToLines(geometry as LineString));
                                }
                                maps.Add(newmap);
                            }
                            break;
                    }
                    break;
            }
            return maps;
        }

        private static List<Line> LineStringToLines(LineString lineString)
        {
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
