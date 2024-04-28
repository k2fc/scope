using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers.Falcon
{
    internal class FalconFile
    {
        public List<FalconUpdate> Updates { get; } = new List<FalconUpdate>();

        public DateTime StartOfData
        {
            get
            {
                return Updates.First().Time;
            }
        }
        public DateTime EndOfData
        {
            get
            {
                return Updates.Last().Time;
            }
        }

        public TimeSpan LengthOfData
        {
            get
            {
                return EndOfData - StartOfData;
            }
        }
        public FalconFile() { }
        public static FalconFile FromFile(string filepath)
        {
            FalconFile newFile = new FalconFile();
            using (var stream = new FileStream(filepath, FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    bool started = false;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (started)
                        {
                            var items = line.Split('\t');
                            if (items.Length > 0 && DateTime.TryParse(items[0], out DateTime time))
                            {
                                var fu = new FalconUpdate();
                                fu.RawLine = line;
                                fu.Time = time;
                                if (!int.TryParse(items[15], out int tid))
                                {
                                    continue;
                                }
                                fu.TrackID = tid;
                                fu.ACID = items[3].Trim();
                                if (items[4].Length >=4 && int.TryParse(items[4].Substring(0,4), out int sq))
                                {
                                    fu.ReportedBeaconCode = sq.ToString("0000");
                                }
                                if (items[7].Length > 2 && int.TryParse(items[7].Substring(0, items[7].Length - 2), out int alt))
                                {
                                    fu.Altitude = new Altitude()
                                    {
                                        Value = alt * 100,
                                        AltitudeType = AltitudeType.True
                                    };
                                }
                                if (int.TryParse(items[8], out int req_alt))
                                {
                                    fu.RequestedAltitude = req_alt;
                                }
                                if (double.TryParse(items[17], out double gt))
                                {
                                    fu.Track = gt;
                                }
                                if (int.TryParse(items[18], out int speed))
                                {
                                    fu.Speed = speed;

                                }
                                fu.Owner = items[19].Trim();
                                fu.FlightRules = items[23].Trim();
                                fu.Category = items[24].Trim();
                                fu.Scratchpad1 = items[25].Trim();
                                fu.Scratchpad2 = items[26].Trim();
                                fu.Type = items[27].Split('-')[0].Trim();
                                fu.Destination = items[35].Trim();
                                fu.PendingHandoff = items[38].Trim();
                                if (items[40].Length == 7)
                                {
                                    if (int.TryParse(items[40].Substring(0, 2), out int lat_deg))
                                    {
                                        if (int.TryParse(items[40].Substring(2, 2), out int lat_min))
                                        {
                                            if (int.TryParse(items[40].Substring(4, 2), out int lat_sec))
                                            {
                                                double latitude, longitude;
                                                latitude = lat_deg + (lat_min / 60.0d) + (lat_sec / 3600.0d);
                                                if (items[40].Last() == 'S')
                                                {
                                                    latitude *= -1.0d;
                                                }
                                                var deg_len = items[41].Length - 5;
                                                if (deg_len == 3 || deg_len == 2)
                                                {
                                                    if (int.TryParse(items[41].Substring(0, deg_len), out int lon_deg))
                                                    {
                                                        if (int.TryParse(items[41].Substring(deg_len, 2), out int lon_min))
                                                        {
                                                            if (int.TryParse(items[41].Substring(deg_len + 2, 2), out int lon_sec))
                                                            {
                                                                longitude = lon_deg + (lon_min / 60.0d) + (lon_sec / 3600.0d);
                                                                if (items[41].Last() == 'W')
                                                                {
                                                                    longitude *= -1.0d;
                                                                }
                                                                fu.Location = new GeoPoint(latitude, longitude);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (items[48] != null && items[48].StartsWith("0x") && int.TryParse(items[48].Substring(2),
                                                    NumberStyles.AllowHexSpecifier,
                                                    null,
                                                    out int addr))
                                {
                                    fu.ModeSAddress = addr;
                                }
                                fu.BcastFLID = items[52].Trim();
                                newFile.Updates.Add(fu);
                            }
                        }
                        else if (line.Trim() == "Start Of Data")
                        {
                            started = true;
                        }
                    }
                }
            }
            return newFile;
        }
    }
}
