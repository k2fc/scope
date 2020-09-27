using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace DGScope.Receivers
{
    class VatsimReceiver : IReceiver
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<Aircraft> Aircraft { get; private set; }
        public GeoPoint Location { get; set; }

        public void Start()
        {
            FetchVatsimData();
            timer.Start();
        }
        public void Stop()
        {
            timer.Stop();
        }
        public void Restart()
        {
            Stop();
            Start();
        }
        public string Url { get; set; }
        public int UpdateInterval { get; set; }
        Stopwatch stopwatch = new Stopwatch();
        System.Timers.Timer timer = new System.Timers.Timer(500);
        public VatsimReceiver(string url = "http://cluster.data.vatsim.net/vatsim-data.txt", int updateinterval = 120) 
        {
            Url = url;
            UpdateInterval = updateinterval;
            timer.Elapsed += Timer_Elapsed;
        }
        public void SetAircraftList(List<Aircraft> Aircraft)
        {
            this.Aircraft = Aircraft;
        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            if(stopwatch.Elapsed > TimeSpan.FromMinutes(2))
            {
                FetchVatsimData();
            }
            else
            {
                ParseVatsimData(stopwatch.ElapsedMilliseconds);
            }
        }
        string vatsimdata;
        private bool FetchVatsimData()
        {
            
            using (var client = new WebClient())
            {
                vatsimdata = client.DownloadString(Url);
                ParseVatsimData();
                stopwatch.Restart();
                try
                {
                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            }
            
            return false;
        }

        private void ParseVatsimData (long coastmilliseconds = 0)
        {
            string[] vatsimlines = vatsimdata.Split('\n');
            string sectionname = "";
            foreach (string line in vatsimlines)
            {
                if (line.Length > 0)
                {
                    switch (line.Substring(0, 1))
                    {
                        case ";":
                            //comment. do nothing
                            break;
                        case "!":
                            //section id
                            sectionname = line.Substring(1, line.IndexOf(':') - 1);
                            break;
                        default:
                            switch (sectionname)
                            {
                                case "CLIENTS":
                                    ParseClient(line, coastmilliseconds);
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
            }
        }
        private void ParseClient (string ClientData, long coastmilliseconds = 0)
        {
            string[] clientdata = ClientData.Split(':');
            double hours = (double)coastmilliseconds / (double)3600000;
            switch (clientdata[3])
            {
                case "PILOT":
                    int VatsimCID = Convert.ToInt32(clientdata[1]);
                    Aircraft test = (from x in Aircraft where x.Callsign == "UAL1447" select x).FirstOrDefault();
                    Aircraft plane = (from x in Aircraft where x.ModeSCode == VatsimCID select x).FirstOrDefault();
                    if (plane == null)
                    {
                        plane = new Aircraft(VatsimCID);
                        Aircraft.Add(plane);
                        Debug.WriteLine("Added airplane " + clientdata[1] + " from VATSIM");
                    }
                    plane.LastMessageTime = DateTime.UtcNow;
                    plane.LastPositionTime = DateTime.UtcNow;
                    plane.ModeSCode = VatsimCID;
                    plane.Callsign = clientdata[0];
                    plane.Altitude = Convert.ToInt32(clientdata[7]);
                    plane.GroundSpeed = Convert.ToInt32(clientdata[8]);
                    plane.Track = Convert.ToInt32(clientdata[38]);
                    double distancetravelled = plane.GroundSpeed * hours;
                    plane.Location = new GeoPoint(double.Parse(clientdata[5]), double.Parse(clientdata[6])).FromPoint(distancetravelled, plane.Track);
                    plane.Squawk = clientdata[17];
                    break;
                default:
                    break;
            }
        }
    }
}
