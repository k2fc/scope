using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using DGScope.Receivers;

namespace DGScope
{
    [Browsable(true)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Radar
    {
        public GeoPoint Location { get; set; } = new GeoPoint();
        [Browsable(false)]
        public double Latitude { get => Location.Latitude; }
        [Browsable(false)]
        public double Longitude { get => Location.Longitude; }
        public double Range { get; set; } = 20;
        public int MaxAltitude { get; set; } = 1000000;
        [Browsable(false)]
        public int Width { get; set; }
        [Browsable(false)]
        public int Height { get; set; }
        public double Size { get => Math.Sqrt(((Width / 2) * (Width / 2) + ((Height / 2) * (Height / 2)))); }
        //public double Size { get => Math.Min(Width, Height); }
        public double RotationPeriod { get; set; } = 4.8;
        public double Rotation { get; set; } = 0;
        [XmlIgnore]
        [Browsable(false)]
        public List<Aircraft> Aircraft { get; } = new List<Aircraft>();
        public ListOfIReceiver Receivers { get; set; } = new ListOfIReceiver();

        public double LatitudeOfTarget(double distance, double bearing)
        {
            double R = 3443.92; // nautical miles
            double brng = bearing * (Math.PI / 180);
            double d = distance;
            double φ1 = Latitude * (Math.PI / 180);

            return (Math.Asin(Math.Sin(φ1) * Math.Cos(d / R) +
                      Math.Cos(φ1) * Math.Sin(d / R) * Math.Cos(brng))) * (180 / Math.PI);
        }

        public double LongitudeOfTarget(double distance, double bearing)
        {
            double R = 3443.92; // nautical miles
            double brng = bearing * (Math.PI / 180);
            double d = distance;
            double φ1 = Latitude * (Math.PI / 180);
            double λ1 = Longitude * (Math.PI / 180);

            return (λ1 + Math.Atan2(Math.Sin(brng) * Math.Sin(d / R) * Math.Cos(φ1),
                                       Math.Cos(d / R) - Math.Sin(φ1) * Math.Sin(LatitudeOfTarget(distance, bearing)))) * (180 / Math.PI);
        }
        public bool isRunning = false;
        public void Start()
        {
            foreach (IReceiver receiver in Receivers)
            {
                receiver.SetAircraftList(Aircraft);
                if(receiver.Enabled)
                    receiver.Start();
            }
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
            foreach (IReceiver receiver in Receivers)
                receiver.Stop();
        }

        Stopwatch Stopwatch = new Stopwatch();
        double lastazimuth = 0;

        public List<Aircraft> Scan()
        {
            double newazimuth = (lastazimuth + ((Stopwatch.ElapsedTicks / (RotationPeriod * 10000000)) * 360)) % 360;
            double slicewidth = (lastazimuth - newazimuth) % 360;
            Stopwatch.Restart();
            List<Aircraft> temprx = Aircraft.ToList();
            List<Aircraft> TargetsScanned = new List<Aircraft>();
            foreach (IReceiver receiver in Receivers)
            {
                TargetsScanned.AddRange(from x in temprx
                                        where x.Bearing(receiver.Location) >= lastazimuth &&
                                        x.Bearing(receiver.Location) <= newazimuth && x.LocationReceivedBy == receiver && x.Altitude <= MaxAltitude
                                        select x);
            }


            lastazimuth = newazimuth;
            if (lastazimuth == 360)
                lastazimuth = 0;

            return TargetsScanned;
        }
        public Radar()
        {

        }

    }
}
