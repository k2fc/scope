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
        public List<string> ExcludedACIDs { get; set; } = new List<string>();
        public List<BeaconCodeRange> ExcludedSSRCodes { get; set; } = new List<BeaconCodeRange>();
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
            RequiredSeparation.Add("B", new SerializableDictionary<string, double> {{ "A", 5 },{ "B", 3 },{ "D", 3 }});

            // C Following
            RequiredSeparation.Add("C", new SerializableDictionary<string, double> {{ "A", 6 },{ "B", 4 },{ "D", 4 }});

            // D Following
            RequiredSeparation.Add("D", new SerializableDictionary<string, double> {{ "A", 6 },{ "B", 4 },{ "D", 4 }});

            // E Following
            RequiredSeparation.Add("E", new SerializableDictionary<string, double> {{ "A", 7 },{ "B", 5 },{ "C", 3.5 },{ "D", 5 }});

            // F Following
            RequiredSeparation.Add("F", new SerializableDictionary<string, double> {{ "A", 7 },{ "B", 5 },{ "C", 3.5 },{ "D", 5 }});

            // G Following
            RequiredSeparation.Add("G", new SerializableDictionary<string, double> {{ "A", 7 },{ "B", 5 },{ "C", 3.5 },{ "D", 5 }});

            // H Following
            RequiredSeparation.Add("H", new SerializableDictionary<string, double> {{ "A", 8 },{ "B", 5 },{ "C", 5 },{ "D", 5 }});

            // I Following
            RequiredSeparation.Add("I", new SerializableDictionary<string, double> {{ "A", 8 },{ "B", 5 },{ "C", 5 },{ "D", 5 },{ "E", 4 }});
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
                        tasklist.Add(CalculateATPA(volume, aircraft, this, radar));
                    }
                    tasks = tasklist.ToArray();
                }
                Task.WaitAll(tasks);
                calculating = false;
            }
        }

        private static Task CalculateATPA(ATPAVolume volume, List<Aircraft> aircraft, ATPA atpa, Radar radar)
        {
            return Task.Run(() => volume.CalculateATPA(aircraft, atpa, radar));
        }

    }

    public class SeparationTable : SerializableDictionary<string, SerializableDictionary<string, double>> { }
    public enum ATPAStatus
    {
        Monitor = 1, Caution = 2, Alert = 3
    }
}
