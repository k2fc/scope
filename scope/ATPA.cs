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
        bool active = false;
        public SeparationTable RequiredSeparation { get; set; } = new SeparationTable();
        public List<ATPAVolume> Volumes { get; set; } = new List<ATPAVolume>();
        public List<string> ActiveVolumeNames { get; set; } = new List<string>();
        public bool Active 
        {
            get => active;
            set
            {
                if (value == active)
                {
                    return;
                }
                else
                {
                    active = value;
                    if (!value)
                    {
                        lock (RadarWindow.Aircraft)
                        {
                            RadarWindow.Aircraft.ToList().ForEach(x => ATPAVolume.ResetAircraftATPAValues(x, true));
                        }
                    }
                }
            }
        }

        public ATPA()
        {
            // Set up 7110.126B CWT table
            // B Following
            var bbehind = new SerializableDictionary<string, double>
            {
                { "A", 5 },
                { "B", 3 },
                { "D", 3 }
            };
            RequiredSeparation.Add("B", bbehind);

            // C Following
            var cbehind = new SerializableDictionary<string, double>
            {
                { "A", 6 },
                { "B", 4 },
                { "D", 4 }
            };
            RequiredSeparation.Add("C", cbehind);

            // D Following
            var dbehind = new SerializableDictionary<string, double>
            {
                { "A", 6 },
                { "B", 4 },
                { "D", 4 }
            };
            RequiredSeparation.Add("D", dbehind);

            // E Following
            var ebehind = new SerializableDictionary<string, double>
            {
                { "A", 7 },
                { "B", 5 },
                { "C", 3.5 },
                { "D", 5 }
            };
            RequiredSeparation.Add("E", ebehind);

            // F Following
            var fbehind = new SerializableDictionary<string, double>
            {
                { "A", 7 },
                { "B", 5 },
                { "C", 3.5 },
                { "D", 5 }
            };
            RequiredSeparation.Add("F", fbehind);

            // G Following
            var gbehind = new SerializableDictionary<string, double>
            {
                { "A", 7 },
                { "B", 5 },
                { "C", 3.5 },
                { "D", 5 }
            };
            RequiredSeparation.Add("G", gbehind);

            // G Following
            var hbehind = new SerializableDictionary<string, double>
            {
                { "A", 8 },
                { "B", 5 },
                { "C", 5 },
                { "D", 5 }
            };
            RequiredSeparation.Add("H", hbehind);

            // I Following
            var ibehind = new SerializableDictionary<string, double>
            {
                { "A", 8 },
                { "B", 5 },
                { "C", 5 },
                { "D", 5 },
                { "E", 4 }
            };
            RequiredSeparation.Add("I", ibehind);
        }
        private bool calculating = false;
        public async Task Calculate(ICollection<Aircraft> aircraftList, Radar radar)
        {
            List<Aircraft> aircraft;
            if (!calculating)
            {
                lock (aircraftList)
                    aircraft = aircraftList.ToList();
                calculating = true;
                Task[] tasks;
                lock (Volumes)
                {
                    List<Task> tasklist = new List<Task>();
                    foreach (var volume in Volumes)
                    {
                        tasklist.Add(CalculateATPA(volume, aircraft, RequiredSeparation, radar));
                    }
                    tasks = tasklist.ToArray();
                }
                Task.WaitAll(tasks);
                calculating = false;
            }
        }

        private static Task CalculateATPA(ATPAVolume volume, List<Aircraft> aircraft, SeparationTable requiredSeparation, Radar radar)
        {
            return Task.Run(() => volume.CalculateATPA(aircraft, requiredSeparation, radar));
        }

    }

    public class SeparationTable : SerializableDictionary<string, SerializableDictionary<string, double>> { }
    public enum ATPAStatus
    {
        Monitor = 1, Caution = 2, Alert = 3
    }
}
