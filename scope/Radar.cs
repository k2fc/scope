using System;
using System.Collections.Generic;
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
        public GeoPoint Location { get; 
            set; } = new GeoPoint();
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
                if (lastMetarUpdate < DateTime.Now.AddMinutes(-5))
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
        
        public int AltimeterCorrection
        {
            get
            {
                if (!correctioncalculated || lastMetarUpdate < DateTime.Now.AddMinutes(-5))
                {
                    double totalaltimeter = 0;
                    if (Metars.Count > 0)
                    {
                        foreach (var metar in Metars)
                        {
                            totalaltimeter += Converter.Pressure(metar.Pressure, libmetar.Enums.PressureUnit.inHG).Value;
                        }
                        totalaltimeter /= Metars.Count;
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
                if (receiver.Enabled)
                    TargetsScanned.AddRange(receiver.Scan());
            }
            return TargetsScanned;
        }
        public Radar()
        {
        }

    }
}
