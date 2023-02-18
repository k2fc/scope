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
        public double Range { get; set; } = 20;
        public int MaxAltitude { get; set; } = 99900;
        public int MinAltitude { get; set; } = -9900;
        public bool Rotating { get; set; } = false;
        public double UpdateRate { get; set; } = 1;
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
        
        
        public List<Metar> Metars
        {
            get
            {
                Task.Run(() => GetWeather(false));
                return parsedMetars.Where(x => this.AltimeterStations.Contains(x.Icao)).ToList();
            }
        }
        private bool correctioncalculated = false;
        Pressure altimeter;
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
                                if (metar.Pressure != null)
                                    totalaltimeter += Converter.Pressure(metar.Pressure, libmetar.Enums.PressureUnit.inHG).Value;
                                else
                                    metarscount--;
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
                    if (altimeter == null)
                        altimeter = new Pressure("");
                    altimeter.Value = totalaltimeter;
                    altimeter.Unit = libmetar.Enums.PressureUnit.inHG;
                    altimeter.Type = libmetar.Enums.PressureType.QNH;
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
        List<Metar> parsedMetars = new List<Metar>();
        bool gettingWx = false;
        public async Task<bool> GetWeather(bool force = false)
        {
            if ((lastMetarUpdate < DateTime.Now.AddMinutes(-5) || force) && !gettingWx) 
            {
                gettingWx = true;
                List<Metar> tempMetars = new List<Metar>();
                var metars = await metarService.GetBulkAsync();
                metars.ToList().ForEach(metar =>
                {
                    if (AltimeterStations.Contains(metar.Icao))
                    {
                        try
                        {
                            metar.Parse();
                        } catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                });
                lastMetarUpdate = DateTime.Now;
                parsedMetars = metars.Where(x => x.IsParsed).ToList();
                correctioncalculated = false;
            }
            gettingWx = false;
            return true;
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
                                        where (BearingIsBetween(x.Bearing(Location), lastazimuth, newazimuth) || !Rotating) && !x.IsOnGround 
                                        && x.Location != null
                                        select x);
            //Console.WriteLine("Scanned method returned {0} aircraft", TargetsScanned.Count);
            lastazimuth = newazimuth;
            if (lastazimuth == 360)
                lastazimuth = 0;
            return TargetsScanned;
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
