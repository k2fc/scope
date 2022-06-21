using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public static class FAAMapDATFileParser
    {
        public static VideoMap GetMapFromFile(string filename)
        {
            VideoMap map = new VideoMap();
            map.Name = "Imported map - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            try
            {
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(filename))
                {
                    List<GeoPoint> points = null;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.Length > 0 && line[0] != '!')
                        {
                            if (line.StartsWith("LINE"))
                            {
                                if (points != null && points.Count >= 2)
                                {
                                    for (int i = 1; i < points.Count; i++)
                                    {
                                        map.Lines.Add(new Line(points[i - 1], points[i]));
                                    }
                                }
                                points = new List<GeoPoint>();
                            }
                            else if (line.StartsWith("GP "))
                            {
                                if (TryParsePoint(line, out GeoPoint point))
                                    points.Add(point);
                            }
                        }

                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);

            }
            return map;
        }
        
        public static bool TryParsePoint(string pointString, out GeoPoint point)
        {
            var latstring = pointString.Substring(2, 14).Trim().Split();
            var lonstring = pointString.Substring(17, 15).Trim().Split();
            point = null;
            if (latstring.Length != 3 || lonstring.Length != 3)
                return false;
            if (!int.TryParse(latstring[0], out int latDeg))
                return false;
            if (!int.TryParse(latstring[1], out int latMin))
                return false;
            if (!double.TryParse(latstring[2], out double latSec))
                return false;
            if (!int.TryParse(lonstring[0], out int lonDeg))
                return false;
            if (!int.TryParse(lonstring[1], out int lonMin))
                return false;
            if (!double.TryParse(lonstring[2], out double lonSec))
                return false;
            var latitude = latDeg + (latMin / 60d) + (latSec / 3600);
            var longitude = -(lonDeg + (lonMin / 60d) + (lonSec / 3600));
            point = new GeoPoint(latitude, longitude);
            return true;
        }
    }

}
