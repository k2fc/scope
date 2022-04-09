using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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
        public double Range { get; set; } = 20;
        public int MaxAltitude { get; set; } = 1000000;
        public int MinAltitude { get; set; } = 0;
        public bool Rotating { get; set; } = true;
        public double UpdateRate { get; set; } = 4.8;
        public int TransitionAltitude { get; set; } = 18000;
        [Browsable(false)]
        public int Width { get; set; }
        [Browsable(false)]
        public int Height { get; set; }
        public double Size { get => Math.Sqrt(((Width / 2) * (Width / 2) + ((Height / 2) * (Height / 2)))); }
        //public double Size { get => Math.Min(Width, Height); }
        [XmlIgnore]
        [Browsable(false)]
        public ObservableCollection<Aircraft> Aircraft { get; } = new ObservableCollection<Aircraft>();
        public ListOfIReceiver Receivers { get; set; } = new ListOfIReceiver();
        [XmlIgnore]
        public Airports Airports { get; set; } = new Airports();
        [XmlIgnore]
        public Waypoints Waypoints { get; set; } = new Waypoints();
        public Font DataBlockFont { get; set; }
        private List<string> _altimeterStations = new List<string>();
        public List<string> AltimeterStations 
        {
            get
            {
                return _altimeterStations;
            }
            set
            {
                _altimeterStations = value;
                correctioncalculated = false;
            }
        }

        private int _altimeterCorrection = 0;
        private MetarService metarService = new MetarService();
        
        public List<Metar> AllMetars
        {
            get 
            {
                GetWeather();
                return allMetars;
            }
        }
        private List<Metar> allMetars;
        public List<Metar> Metars
        {
            get
            {
                return AllMetars.Where(metar => AltimeterStations.Contains(metar.Icao)).ToList();
            }
        }
        private bool correctioncalculated = false;
        Pressure altimeter = new Pressure("");
        private Pressure cachedalt => Altimeter;
        public Pressure Altimeter
        {
            get
            {
                if (!correctioncalculated || lastMetarUpdate < DateTime.Now.AddMinutes(-5))
                {
                    double totalaltimeter = 0;
                    int metarscount = Metars.Count;
                    if (Metars.Count > 0)
                    {
                        foreach (var metar in Metars)
                        {
                            try
                            {
                                totalaltimeter += Converter.Pressure(metar.Pressure, libmetar.Enums.PressureUnit.inHG).Value;
                            }
                            catch
                            {
                                metarscount--;
                            }

                        }
                        totalaltimeter /= metarscount;
                    }
                    else
                    {
                        totalaltimeter = 29.92;
                    }
                    altimeter = new Pressure("")
                    {
                        Value = totalaltimeter,
                        Unit = libmetar.Enums.PressureUnit.inHG,
                        Type = libmetar.Enums.PressureType.QNH
                    };
                }
                return altimeter;
            }
        }
        
        public int AltimeterCorrection
        {
            get
            {
                if (!correctioncalculated || lastMetarUpdate < DateTime.Now.AddMinutes(-5))
                {
                    double totalaltimeter = 0;
                    int metarscount = Metars.Count;
                    if (Metars.Count > 0)
                    {
                        foreach (var metar in Metars)
                        {
                            try
                            {
                                totalaltimeter += Converter.Pressure(metar.Pressure, libmetar.Enums.PressureUnit.inHG).Value;
                            }
                            catch
                            {
                                metarscount--;
                            }

                            }
                        totalaltimeter /= metarscount;
                    }
                    else
                    {
                        totalaltimeter = 29.92;
                    }
                    _altimeterCorrection = (int)((totalaltimeter - 29.92) * 1000);
                    correctioncalculated = true;
                }
                return _altimeterCorrection;
            }
        }

        DateTime lastMetarUpdate = DateTime.MinValue;
        public void GetWeather(bool force = false)
        {
            if (lastMetarUpdate < DateTime.Now.AddMinutes(-5) || force)
            {
                List<Metar> tempMetars = new List<Metar>();
                foreach (var metar in metarService.GetBulk())
                {
                    try
                    {
                        metar.Parse();
                        tempMetars.Add(metar);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Excluded {0} because: {1}", metar.Icao, ex.Message);
                    }
                }
                lastMetarUpdate = DateTime.Now;
                correctioncalculated = false;
                allMetars = tempMetars;
            }
        }
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
                    try
                    {
                        receiver.Start();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("An error occured starting receiver {0}.\r\n{1}", 
                            receiver.Name, ex.Message),"Error starting receiver", System.Windows.Forms.MessageBoxButtons.OK, 
                            System.Windows.Forms.MessageBoxIcon.Warning); 
                    }
            }
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
            foreach (Receiver receiver in Receivers)
                receiver.Stop();
        }
        private Stopwatch Stopwatch = new Stopwatch();
        private double lastazimuth = 0;
        public List<Aircraft> Scan()
        {
            if (Aircraft == null)
                return new List<Aircraft>();
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();
            double newazimuth = (lastazimuth + ((Stopwatch.ElapsedTicks / (UpdateRate * 10000000)) * 360)) % 360;
            double slicewidth = (lastazimuth - newazimuth) % 360;
            List<Aircraft> TargetsScanned = new List<Aircraft>();
            if (!Rotating && (Stopwatch.ElapsedTicks / (UpdateRate * 10000000)) < 1)
            {
                return TargetsScanned;
            }
            Stopwatch.Restart();
            lock (Aircraft)
                TargetsScanned.AddRange(from x in Aircraft
                                        where ((x.Bearing(Location) >= lastazimuth &&
                                        x.Bearing(Location) <= newazimuth) || !Rotating) && !x.IsOnGround 
                                        && x.Location != null
                                        select x);
            //Console.WriteLine("Scanned method returned {0} aircraft", TargetsScanned.Count);
            lastazimuth = newazimuth;
            if (lastazimuth == 360)
                lastazimuth = 0;
            return TargetsScanned;
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
