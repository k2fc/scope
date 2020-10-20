using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using DGScope.Receivers.ADSBX;

namespace DGScope.Receivers
{
    public class ADSBExchageReceiver : Receiver
    {
        public string APIKey { get; set; }
        public int RefreshInterval { get; set; } = 5;
        Timer timer;
        object lockObject = new object();

        public override void Start()
        {
            timer = new Timer(new TimerCallback(cbTimerElapsed),null,0,RefreshInterval * 1000);
        }

        public override void Stop()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        private void cbTimerElapsed(object state)
        {
            lock (lockObject)
            {
                GetAirplanes();
            }
        }
        private void GetAirplanes()
        {
            var url = string.Format("https://adsbexchange.com/api/aircraft/json/lat/{0}/lon/{1}/dist/{2}/",Location.Latitude, Location.Longitude, Range);
            var webRequest = WebRequest.Create(url);
            webRequest.Headers.Add("api-auth", APIKey);
            webRequest.Headers.Add("accept-encoding", "gzip");
            webRequest.ContentType = "application/json";

            using (var s = webRequest.GetResponse().GetResponseStream())
            {
                using (GZipStream stream = new GZipStream(s, CompressionMode.Decompress))
                {
                    using (var sr = new StreamReader(stream))
                    {
                        var jsonData = sr.ReadToEnd();
                        var response = JsonConvert.DeserializeObject<Response>(jsonData);
                        foreach (var jsonPlane in response.Aircraft)
                        {
                            if (jsonPlane.LocationSource == Source.ADSB)
                            {
                                Aircraft plane = GetPlane(jsonPlane.ModeSCode);
                                if (plane.LastMessageTime < response.Time)
                                    plane.LastMessageTime = response.Time;
                                bool posDataUpdate = false;
                                if (jsonPlane.PosTime > plane.LastPositionTime)
                                {
                                    posDataUpdate = true;
                                    plane.LastPositionTime = jsonPlane.PosTime;
                                    plane.LocationReceivedBy = this;
                                }
                                plane.ModeSCode = jsonPlane.ModeSCode;
                                if (jsonPlane.Callsign != "")
                                {
                                    plane.Callsign = jsonPlane.Callsign;
                                }
                                if (posDataUpdate)
                                {
                                    plane.Altitude = (int)jsonPlane.Altitude;
                                    plane.Location = jsonPlane.Location;
                                    plane.GroundSpeed = (int)jsonPlane.Speed;
                                    plane.VerticalRate = (int)jsonPlane.VerticalSpeed;
                                    plane.IsOnGround = jsonPlane.OnGround;
                                    plane.Squawk = jsonPlane.Squawk;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
