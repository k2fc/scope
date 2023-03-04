using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DGScope.Receivers;
using libmetar;
using libmetar.Services;

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
        public double Range { get; set; } = 200;
        [DisplayName("Update Rate"), Description("The rate at which to draw targets. The rate at which the radar sweep rotates, if rotating is true")] 
        public double UpdateRate { get; set; } = 1;
        [DisplayName("Inhibit Extrapolation"), Description("Inhibit extrapolation of aircraft positions")]
        public bool InhibitExtrapolation { get; set; } = false;
        public bool Rotating { get; set; } = false;
        [DisplayName("Primary Target Shape"), Description("Shape of primary targets")]
        public TargetShape TargetShape { get; set; } = TargetShape.Circle;
        [Category("Identity")]
        public string Name { get; set; }
        [Category("Identity")]
        public char Char { get; set; }
        [XmlIgnore]
        private Airports Airports { get; set; } = new Airports();
        [XmlIgnore]
        private Waypoints Waypoints { get; set; } = new Waypoints();
        

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
        private Stopwatch Stopwatch = new Stopwatch();
        private double lastazimuth = 0;
        public async Task Scan(DateTime time)
        {
            List<Aircraft> TargetsScanned = new List<Aircraft>();
            List<Task> tasks = new List<Task>();
            if (RadarWindow.Aircraft == null)
                return;
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();
            double newazimuth = (lastazimuth + ((Stopwatch.ElapsedTicks / (UpdateRate * 10000000)) * 360)) % 360;
            double slicewidth = (lastazimuth - newazimuth) % 360;
            if (!Rotating && (Stopwatch.ElapsedTicks / (UpdateRate * 10000000)) < 1)
                return;
            if (RadarWindow.Aircraft == null)
                return;
            Stopwatch.Restart();
            lock (RadarWindow.Aircraft)
                TargetsScanned.AddRange(from x in RadarWindow.Aircraft
                                        where (BearingIsBetween(x.Bearing(Location), lastazimuth, newazimuth) || !Rotating) && !x.IsOnGround 
                                        && x.Location != null
                                        select x);
            TargetsScanned.ForEach(x => tasks.Add(ScanTarget(x, time)));
            //Console.WriteLine("Scanned method returned {0} aircraft", TargetsScanned.Count);
            lastazimuth = newazimuth;
            if (lastazimuth == 360)
                lastazimuth = 0;
            Task.WaitAll(tasks.ToArray());
            return; 
        }

        private async Task ScanTarget(Aircraft plane, DateTime time)
        {
            GeoPoint location;
            if (plane.PrimaryOnly || InhibitExtrapolation)
            {
                location = plane.Location;
            }
            else
            {
                location = plane.ExtrapolatePosition(time);
            }
            lock (plane.SweptLocations)
            {
                if (plane.SweptLocations.ContainsKey(this))
                {
                    plane.SweptLocations[this] = location;
                }
                else
                {
                    plane.SweptLocations.Add(this, location);
                }
            }
            lock (plane.SweptTracks)
            {
                int track;

                if (InhibitExtrapolation)
                {
                    track = plane.Track;
                }
                else
                {
                    track = (int)plane.ExtrapolateTrack(time);
                }
                if (plane.SweptTracks.ContainsKey(this))
                {
                    plane.SweptTracks[this] = (int)plane.ExtrapolateTrack(time);
                }
                else
                {
                    plane.SweptTracks.Add(this, (int)plane.ExtrapolateTrack(time));
                }
            }
            lock (plane.SweptAltitudes)
            {
                if (plane.SweptAltitudes.ContainsKey(this))
                {
                    plane.SweptAltitudes[this] = plane.TrueAltitude;
                }
                else
                {
                    plane.SweptAltitudes.Add(this, plane.TrueAltitude);
                }
            }
            lock (plane.SweptSpeeds)
            {
                if (plane.SweptSpeeds.ContainsKey(this))
                {
                    plane.SweptSpeeds[this] = plane.GroundSpeed;
                }
                else
                {
                    plane.SweptSpeeds.Add(this, plane.GroundSpeed);
                }
            }
        }

        public bool BearingIsBetween(double bearing, double az1, double az2)
        {
            if (az2 == az1)
            {
                return bearing == az1;
            }
            if (az2 > az1)
            {
                return bearing >= az1 && bearing <= az2;
            }
            else
            {
                return (bearing >= az1 && bearing <= 360) || bearing <= az2;
            }
        }

        public bool InRange(GeoPoint location, double altitude)
        {
            if (location == null)
                return false;
            var distance = location.DistanceTo(Location, altitude);
            if (distance <= Range)
                return true;
            return false;
        }
        public Radar()
        {
        }

    }
}
