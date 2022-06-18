using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class VRCFileParser
    {
        public static List<VideoMap> GetMapsFromFile(string filename)
        {
            List<VideoMap> maps = new List<VideoMap>();
            try
            {
                // Open the text file using a stream reader.
                using (var sr = new StreamReader(filename))
                {
                    // Read the stream as a string, and write the string to the console.
                    string sectionName = "";
                    bool shortened = false;
                    VideoMap currentMap = null;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string linedata;

                        if (line.Contains(";"))
                            linedata = line.Substring(0, line.IndexOf(";"));
                        else
                            linedata = line;
                        linedata = linedata;
                        bool issectionheader = false;
                        if (linedata.Contains("[") && linedata.Contains("]"))
                        {
                            sectionName = (linedata.Substring(line.IndexOf("[") + 1, linedata.IndexOf("]") - linedata.IndexOf("[") - 1)).ToUpper();
                            issectionheader = true;
                        }
                        if (issectionheader)
                            continue;
                        if (linedata.Length == 0)
                            continue;
                        if (sectionName != "SID" && sectionName != "STAR")
                            continue;
                        string linename;
                        if (linedata.Substring(0, 1).Trim().Length != 0 && linedata.Length > 25)
                        {
                            linename = linedata.Substring(0, 25).Trim();
                            linedata = linedata.Substring(26);
                            currentMap = new VideoMap() { Name = linename };
                            maps.Add(currentMap);
                        }
                        if (currentMap != null && TryParseLine(linedata, out Line newline))
                            currentMap.Lines.Add(newline);



                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);

            }
            return maps;
        }
        public static bool TryParseLine(string linestring, out Line line)
        {
            linestring = linestring.Trim();
            line = new Line();
            var lineparts = linestring.Split(' ');
            if (lineparts.Length < 4)
                return false;
            string point1 = string.Concat(lineparts[0], " ", lineparts[1]);
            string point2 = string.Concat(lineparts[2], " ", lineparts[3]);
            if (TryParsePoint(point1, out GeoPoint geoPoint1))
                if (TryParsePoint(point2, out GeoPoint geoPoint2))
                {
                    line.End1 = geoPoint1;
                    line.End2 = geoPoint2;
                    return true;
                }
            return false;
        }
        public static bool TryParsePoint(string pointString, out GeoPoint point)
        {
            point = new GeoPoint();
            pointString = pointString.Trim();
            var pointSplit = pointString.Split(' ');
            if (pointSplit.Length < 2)
                return false;
            var lat = pointSplit[0].Split('.');
            var lon = pointSplit[1].Split('.');
            double value = 0;
            double latitude = 0;
            if (lat.Length == 2 && double.TryParse(pointSplit[0], out latitude))
            {

            }
            else //  Try VRC format
            {
                if (lat[0].Length >= 2 && double.TryParse(lat[0].Substring(1), out value))
                    latitude += value;
                else
                    return false;
                if (lat.Length > 2 && double.TryParse(lat[1], out value))
                    latitude += value / 60;
                if (lat.Length >= 3 && double.TryParse(lat[2], out value))
                    latitude += value / 3600;
                if (lat.Length >= 4 && double.TryParse(lat[3], out value))
                    latitude += value / 3600000;
            }
            if (pointSplit[0].Contains('S'))
                latitude *= -1;

            double longitude = 0;
            if (lon.Length == 2 && double.TryParse(pointSplit[1], out longitude))
            {

            }
            else //  Try VRC format
            {
                if (double.TryParse(lon[0].Substring(1), out value))
                    longitude += value;
                else
                    return false;
                if (lon.Length > 2 && double.TryParse(lon[1], out value))
                    longitude += value / 60;
                if (lon.Length >= 3 && double.TryParse(lon[2], out value))
                    longitude += value / 3600;
                if (lon.Length >= 4 && double.TryParse(lon[3], out value))
                    longitude += value / 3600000;
            }
            if (pointSplit[1].Contains('W'))
                longitude *= -1;
            point.Latitude = latitude;
            point.Longitude = longitude;
            return true;
        }
    }
}
