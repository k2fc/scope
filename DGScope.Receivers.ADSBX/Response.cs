using Newtonsoft.Json;
using System;

namespace DGScope.Receivers.ADSBX
{
    struct Response
    {
        [JsonProperty("ac")]
        public JsonAircraft[] Aircraft { get; private set; }
        [JsonProperty("total")]
        public int Count { get; private set; }
        [JsonProperty("ctime")]
        private Int64 ctime;
        [JsonProperty("ptime")]
        private int ptime;

        public DateTime Time
        {
            get
            {
                var time = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return time.AddMilliseconds(ctime);
            }
        }
    }
    struct JsonAircraft
    {
        [JsonProperty("postime")]
        private string posTime;
        [JsonProperty("icao")]
        private string icao;
        [JsonProperty("reg")]
        public string Registration { get; private set; }
        [JsonProperty("type")]
        public string Type { get; private set; }
        [JsonProperty("spd")]
        private string spd;
        [JsonProperty("altt")]
        private string altt;
        [JsonProperty("alt")]
        private string alt;
        [JsonProperty("galt")]
        private string galt;
        [JsonProperty("talt")]
        private string talt;
        [JsonProperty("lat")]
        private string lat;
        [JsonProperty("lon")]
        private string lon;
        [JsonProperty("vsit")]
        private string vsit;
        [JsonProperty("vsi")]
        private string vsi;
        [JsonProperty("call")]
        public string Callsign { get; private set; }
        [JsonProperty("gnd")]
        private string gnd;
        [JsonProperty("sqk")]
        public string Squawk { get; private set; }
        [JsonProperty("trkh")]
        private string trkh;
        [JsonProperty("trak")]
        private string trak;
        [JsonProperty("ttrk")]
        private string ttrk;
        [JsonProperty("trt")]
        private string trt;
        [JsonProperty("pos")]
        private string pos;
        [JsonProperty("mlat")]
        private string mlat;
        [JsonProperty("tisb")]
        private string tisb;
        [JsonProperty("sat")]
        private string sat;
        [JsonProperty("opicao")]
        public string OperatorICAO { get; private set; }
        [JsonProperty("cou")]
        public string Country { get; private set; }
        [JsonProperty("mil")]
        private string mil;
        [JsonProperty("interested")]
        private string interested;
        [JsonProperty("from")]
        public string Origin { get; private set; }
        [JsonProperty("to")]
        public string Destination { get; private set; }
        [JsonProperty("dst")]
        private string dst;
        public GeoPoint Location
        {
            get
            {
                var latitude = Double.Parse(lat);
                var longitude = Double.Parse(lon);
                return new GeoPoint(latitude, longitude);
            }
        }

        public double VerticalSpeed
        {
            get
            {
                if (vsi != "")
                {
                    return Double.Parse(vsi);
                }
                return 0;
            }
        }

        public double Altitude
        {
            get
            {
                if (alt != "")
                {
                    return Double.Parse(alt);
                }
                return 0;
            }
        }

        public double Speed
        {
            get
            {
                if (spd != "")
                {
                    return Double.Parse(spd);
                }
                return 0;
            }
        }
        public bool OnGround
        {
            get
            {
                return gnd == "1";
            }
        }

        public int ModeSCode
        {
            get
            {
                return Convert.ToInt32(icao, 16);
            }
        }

        public DateTime PosTime
        {
            get
            {
                var time = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return time.AddMilliseconds(Double.Parse(posTime));
            }
        }

        public Source LocationSource
        {
            get
            {
                if (tisb == "1")
                {
                    return Source.TISB;
                }
                else if (mlat == "1")
                {
                    return Source.MLAT;
                }
                else if (sat == "1")
                {
                    return Source.Satellite;
                }
                else
                {
                    return Source.ADSB;
                }
            }
        }

        public override string ToString()
        {
            return Callsign;
        }
    }
    public enum Source { ADSB, MLAT, TISB, Satellite }
}
