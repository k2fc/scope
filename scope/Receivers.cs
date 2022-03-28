using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace DGScope.Receivers
{
   public abstract class Receiver
    {
        public string Name { get; set;  }
        public bool Enabled { get; set; }

        protected ObservableCollection<Aircraft> aircraft;
        protected double minel => MinElevation * Math.PI / 180;
        protected double maxel => MaxElevation * Math.PI / 180;
        public GeoPoint Location { get; set; } = new GeoPoint(0, 0);
        public double Altitude { get; set; } = 0;
        public double MaxElevation { get; set; } = 90;
        public double MinElevation { get; set; } = -90;
        public double RotationPeriod { get; set; } = 4.8;
        public double Range { get; set; } = 100;
        public bool Rotating { get; set; } = true;
        public abstract void Start();
        public abstract void Stop();
        public void Restart(int sleep = 0)
        {
            Stop();
            System.Threading.Thread.Sleep(sleep);
            Start();
        }

        public bool InRange(GeoPoint location, double altitude)
        {
            var distance = location.DistanceTo(Location, altitude - Altitude);
            double elevation;
            if (location != Location)
                elevation = Math.Atan(((altitude - Altitude) / 6076.12) / distance);
            else if (altitude < Altitude)
                elevation = -90;
            else elevation = 90;
            if (distance <= Range && elevation > minel && elevation < maxel && distance < Range)
                return true;
            return false;
        }
        public void SetAircraftList(ObservableCollection<Aircraft> Aircraft)
        {
            aircraft = Aircraft;
        }
        Stopwatch Stopwatch = new Stopwatch();
        double lastazimuth = 0;
        public List<Aircraft> Scan()
        {
            
            return new List<Aircraft>();
        }

        public Aircraft GetPlane(int icaoID)
        {
            Aircraft plane;
            lock (aircraft)
            {
                plane = (from x in aircraft where x.ModeSCode == icaoID select x).FirstOrDefault();
                if (plane == null)
                {
                    plane = new Aircraft(icaoID);
                    aircraft.Add(plane);
                    Debug.WriteLine("Added airplane {0} from {1}", icaoID.ToString("X"), Name);
                }
            }
            return plane;
        }

        public Aircraft GetPlaneBySquawk(string squawk)
        {
            Aircraft plane = null;
            lock (aircraft)
            {
                if (aircraft.Where(x => x.Squawk == squawk).Count() == 1)
                    plane = (from x in aircraft where x.Squawk == squawk select x).FirstOrDefault();
            }
            return plane;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

   
}