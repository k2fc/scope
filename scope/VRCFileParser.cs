using System;
using System.Collections.Generic;
using System.IO;
namespace DGScope
{
    public static class VRCFileParser
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
                        linedata = linedata.TrimEnd();
                        bool issectionheader = false;
                        if (linedata.Contains("[") && linedata.Contains("]"))
                        {
                            sectionName = linedata.Substring(line.IndexOf("[") + 1, linedata.IndexOf("]") - linedata.IndexOf("[") - 1);
                            issectionheader = true;
                        }
                        if (linedata.Length == 0)
                            continue;
                        else if (linedata.Substring(0, 1).Trim().Length != 0)
                            shortened = false;
                        if (shortened)
                            linedata = "                            " + linedata.Trim();
                        linedata = linedata.Replace("\t", "    ");
                        if (linedata.Contains("* Arrival North"))
                            Console.WriteLine();
                        if (linedata.Contains("O O O O"))
                        {
                            shortened = true;
                            linedata += "                                                   ";
                        }
                        if (!issectionheader && linedata.Length >= 85)
                        {
                            string linename = linedata.Substring(0, 25).Trim();
                            switch (sectionName.ToUpper())
                            {
                                case "SID":
                                    if (linename.Length > 0)
                                    {
                                        currentMap = new VideoMap() { Name = linename };
                                        maps.Add(currentMap);
                                    }
                                    if (currentMap != null && !linedata.Contains("O O O O"))
                                        currentMap.Lines.Add(Line.Parse(linedata.Substring(26)));
                                    break;
                                case "STAR":
                                    if (linename.Length > 0)
                                    {
                                        currentMap = new VideoMap() { Name = linename };
                                        maps.Add(currentMap);
                                    }
                                    if (currentMap != null && !linedata.Contains("O O O O"))
                                        currentMap.Lines.Add(Line.Parse(linedata.Substring(26)));
                                    break;
                                default:
                                    break;
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
            return maps;
        }
       }
}
