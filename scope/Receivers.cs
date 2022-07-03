using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DGScope.Receivers
{
   public abstract class Receiver
    {
        private bool enabled;
        public string Name { get; set;  }
        public bool Enabled 
        {
            get
            {
                return enabled;
            }
            set
            {
                if (value && aircraft != null)
                {
                    Start();
                }
                else
                    Stop();
                enabled = value;
            }
        }

        protected ObservableCollection<Aircraft> aircraft;
        public GeoPoint Location { get; set; } = new GeoPoint(0, 0);
        public double Range { get; set; } = 250;
        public bool CreateNewAircraft { get; set; } = true;
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
            var distance = location.DistanceTo(Location);
            if (distance <= Range)
                return true;
            return false;
        }
        public void SetAircraftList(ObservableCollection<Aircraft> Aircraft)
        {
            aircraft = Aircraft;
            if (Enabled)
                Start();
        }
        
        public Aircraft GetPlane(Guid guid, bool createnew = false)
        {
            Aircraft plane;
            lock (aircraft)
            {
                plane = (from x in aircraft where x.TrackGuid == guid select x).FirstOrDefault();
                if (plane == null && createnew)
                {
                    plane = new Aircraft(guid);
                    aircraft.Add(plane);
                    Debug.WriteLine("Added airplane {0} from {1}", guid.ToString(), Name);
                }
            }
            return plane;
        }
        public Aircraft GetPlane(int icaoID, bool createnew = true)
        {
            Aircraft plane;
            lock (aircraft)
            {
                plane = (from x in aircraft where x.ModeSCode == icaoID select x).FirstOrDefault();
                if (plane == null && createnew)
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

        public void Remove(Aircraft plane)
        {
            lock (aircraft)
            {
                aircraft.Where(x => x == plane).ToList().ForEach(y => aircraft.Remove(y));
            }
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }

   
}