using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        public int MinAltitude { get; set; } = 0;
        [Browsable(false)]
        public int Width { get; set; }
        [Browsable(false)]
        public int Height { get; set; }
        public double Size { get => Math.Sqrt(((Width / 2) * (Width / 2) + ((Height / 2) * (Height / 2)))); }
        //public double Size { get => Math.Min(Width, Height); }
        [XmlIgnore]
        [Browsable(false)]
        public List<Aircraft> Aircraft { get; } = new List<Aircraft>();
        public ListOfIReceiver Receivers { get; set; } = new ListOfIReceiver();
        public Font DataBlockFont { get; set; }

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
            foreach (Receiver receiver in Receivers)
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
            foreach (Receiver receiver in Receivers)
                receiver.Stop();
        }

       public List<Aircraft> Scan()
        {
            List<Aircraft> TargetsScanned = new List<Aircraft>();
            foreach (Receiver receiver in Receivers)
            {
                lock(Aircraft)
                TargetsScanned.AddRange(receiver.Scan());
            }
            return TargetsScanned;
        }
        public Radar()
        {

        }

    }
}
