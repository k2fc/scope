﻿using System;
using System.Collections.Generic;
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

        protected List<Aircraft> aircraft;
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
        public void SetAircraftList(List<Aircraft> Aircraft)
        {
            aircraft = Aircraft;
        }
        Stopwatch Stopwatch = new Stopwatch();
        double lastazimuth = 0;
        public List<Aircraft> Scan()
        {
            if (aircraft == null)
                return new List<Aircraft>();
            double newazimuth = (lastazimuth + ((Stopwatch.ElapsedTicks / (RotationPeriod * 10000000)) * 360)) % 360;
            double slicewidth = (lastazimuth - newazimuth) % 360;
            Stopwatch.Restart();
            List<Aircraft> TargetsScanned = new List<Aircraft>();
            lock (aircraft)
                        TargetsScanned.AddRange(from x in aircraft
                                            where ((x.Bearing(Location) >= lastazimuth &&
                                            x.Bearing(Location) <= newazimuth) || !Rotating) && !x.IsOnGround && !x.Drawn 
                                            && InRange(x.Location, x.Altitude) && x.LocationReceivedBy == this
                                            select x);
            lastazimuth = newazimuth;
            if (lastazimuth == 360)
                lastazimuth = 0;
            return TargetsScanned;
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

        public override string ToString()
        {
            return base.ToString();
        }
    }

   
}