using libmetar.Services;
using libmetar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
                        }
                        catch (Exception ex)
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
    }
}
