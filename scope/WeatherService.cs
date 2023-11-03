using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using csharp_metar_decoder;
using csharp_metar_decoder.entity;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;

namespace DGScope
{
    internal class WeatherService
    {
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



        public List<DecodedMetar> Metars
        {
            get
            {
                Task.Run(() => GetWeather(false));
                return parsedMetars.Where(x => this.AltimeterStations.Contains(x.ICAO)).ToList();
            }
        }
        private bool correctioncalculated = false;
        Altimeter altimeter;
        private Altimeter cachedalt => Altimeter;
        public Altimeter Altimeter
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
                                    totalaltimeter += metar.Pressure.GetConvertedValue(Value.Unit.MercuryInch);
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
                        altimeter = new Altimeter();

                    altimeter.Value = totalaltimeter;
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
                                totalaltimeter += metar.Pressure.GetConvertedValue(Value.Unit.MercuryInch);
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
        List<DecodedMetar> parsedMetars = new List<DecodedMetar>();
        bool gettingWx = false;
        public async Task<bool> GetWeather(bool force = false)
        {
            if ((lastMetarUpdate < DateTime.Now.AddMinutes(-5) || force) && !gettingWx)
            {
                gettingWx = true;
                List<DecodedMetar> tempMetars = new List<DecodedMetar>();
                var metars = await GetBulkAsync();
                lastMetarUpdate = DateTime.Now;
                parsedMetars = metars.Where(x => x.IsValid).ToList();
                correctioncalculated = false;
            }
            gettingWx = false;
            return true;
        }

        public async Task<List<DecodedMetar>> GetBulkAsync()
        {
            List<DecodedMetar> list = new List<DecodedMetar>();
            try
            {
                var url = "https://aviationweather.gov/api/data/metar?ids=" + string.Join(",", AltimeterStations);
                using (WebClient client = new WebClient())
                {
                    string s = client.DownloadString(url);
                    var metars = Regex.Split(s, "\r\n|\r|\n");
                    foreach (var metar in metars)
                    {
                        try
                        {
                            list.Add(MetarDecoder.ParseWithMode(metar));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return list;
        }
    }
    public class Altimeter
    {
        public double Value { get; set; } = 29.92;
    }
}
