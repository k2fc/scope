using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace DGScope.Receivers
{
    public abstract class Receiver
    {
        public string Name { get; set;  }
        public bool Enabled { get; set; }

        protected List<Aircraft> aircraft;
        protected double minel => MinElevation * Math.PI / 180;
        protected double maxel => MaxElevation * Math.PI / 180;
        public GeoPoint Location { get; set; } = new GeoPoint(0, 0);
        public double Altitude { get; set; } = 0;
        public double MaxElevation { get; set; } = 90;
        public double MinElevation { get; set; } = -90;
        public double RotationPeriod { get; set; } = 4.8;
        public double Range { get; set; } = 100;
        public abstract void Start();
        public abstract void Stop();
        public void Restart(int sleep = 0)
        {
            Stop();
            System.Threading.Thread.Sleep(sleep);
            Start();
        }

        public bool InRange(GeoPoint location, double altitude)
        {
            var distance = location.DistanceTo(Location, altitude - Altitude);
            double elevation;
            if (location != Location)
                elevation = Math.Atan(((altitude - Altitude) / 6076.12) / distance);
            else if (altitude < Altitude)
                elevation = -90;
            else elevation = 90;
            if (distance <= Range && elevation > minel && elevation < maxel && distance < Range)
                return true;
            return false;
        }
        public void SetAircraftList(List<Aircraft> Aircraft)
        {
            aircraft = Aircraft;
        }
        Stopwatch Stopwatch = new Stopwatch();
        double lastazimuth = 0;
        public List<Aircraft> Scan()
        {
            if (aircraft == null)
                return new List<Aircraft>();
            double newazimuth = (lastazimuth + ((Stopwatch.ElapsedTicks / (RotationPeriod * 10000000)) * 360)) % 360;
            double slicewidth = (lastazimuth - newazimuth) % 360;
            Stopwatch.Restart();
            List<Aircraft> TargetsScanned = new List<Aircraft>();
            lock (aircraft)
                        TargetsScanned.AddRange(from x in aircraft
                                            where x.Bearing(Location) >= lastazimuth &&
                                            x.Bearing(Location) <= newazimuth && !x.IsOnGround && x.LastPositionTime > DateTime.UtcNow.AddSeconds(-RotationPeriod) 
                                            && InRange(x.Location, x.Altitude)
                                            select x);
            lastazimuth = newazimuth;
            if (lastazimuth == 360)
                lastazimuth = 0;

            return TargetsScanned;
        }

        public Aircraft GetPlane(int icaoID)
        {
            Aircraft plane;
            lock (aircraft)
            {
                plane = (from x in aircraft where x.ModeSCode == icaoID select x).FirstOrDefault();
                if (plane == null)
                {
                    plane = new Aircraft(icaoID);
                    aircraft.Add(plane);
                    Debug.WriteLine("Added airplane {0} from {1}", icaoID.ToString("X"), Name);
                }
            }
            return plane;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

   public class SBSReceiver : Receiver
    {
        
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 30003;

        EventDrivenTCPClient client;

        public SBSReceiver() { }
        public SBSReceiver(string Host)
        {
            this.Host = Host;
        }

        public SBSReceiver(string Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
        }
        bool running = false;
        public override void Start()
        {
            if (running)
                return;
            Enabled = true;
            client = new EventDrivenTCPClient(Host, Port, true);
            client.DataReceived += Client_DataReceived;
            client.Connect();
            running = true;
        }
        public override void Stop()
        {
            if (!running)
                return;
            Enabled = false;
            client.Disconnect();
            while (client.ConnectionState != EventDrivenTCPClient.ConnectionStatus.DisconnectedByUser) { }
            //client.DataReceived -= Client_DataReceived;

            running = false;
        }
        
        
        string rxBuffer;
        private void Client_DataReceived(EventDrivenTCPClient sender, object data)
        {
            rxBuffer += data;
            rxBuffer.Replace("\r\n", "\n");
            while (rxBuffer.Contains("\n"))
            {
                string message = rxBuffer.Substring(0, rxBuffer.IndexOf("\n"));
                string[] sbs_data = message.ToString().Split(',');
                rxBuffer = rxBuffer.Substring(rxBuffer.IndexOf("\n") + 1);
                try
                {
                    switch (sbs_data[0])
                    {
                        case "MSG":
                            int icaoID = Convert.ToInt32(sbs_data[4], 16);
                            Aircraft plane = GetPlane(icaoID);
                            
                            DateTime messageTime = DateTime.Parse(sbs_data[6] + " " + sbs_data[7]);
                            //DateTime messageTime = DateTime.UtcNow;
                            if (plane.LastMessageTime < messageTime)
                                plane.LastMessageTime = messageTime;
                            plane.ModeSCode = icaoID;
                            switch (sbs_data[1])
                            {

                                case "1":
                                    plane.Callsign = sbs_data[10];
                                    break;
                                case "2":
                                    if (sbs_data[11] != "")
                                        plane.Altitude = Int32.Parse(sbs_data[11]);
                                    if (sbs_data[12] != "")
                                        plane.GroundSpeed = (int)Double.Parse(sbs_data[12]);
                                    if (sbs_data[13] != "")
                                        plane.Track = (int)Double.Parse(sbs_data[13]);
                                    if (sbs_data[14] != "" && sbs_data[15] != "" && messageTime > plane.LastPositionTime)
                                    {
                                        var latitude = Double.Parse(sbs_data[14]);
                                        var longitude = Double.Parse(sbs_data[15]);
                                        var temploc = new GeoPoint(latitude, longitude);
                                        if (InRange(temploc, plane.Altitude))
                                        {
                                            plane.Location = temploc;
                                            plane.LastPositionTime = messageTime;
                                            plane.LocationReceivedBy = this;
                                        }
                                    }
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "3":
                                    if (sbs_data[11] != "")
                                        plane.Altitude = Int32.Parse(sbs_data[11]);
                                    if (sbs_data[14] != "" && sbs_data[15] != "" && messageTime > plane.LastPositionTime)
                                    {
                                        var latitude = Double.Parse(sbs_data[14]);
                                        var longitude = Double.Parse(sbs_data[15]);
                                        var temploc = new GeoPoint(latitude, longitude);
                                        if (InRange(temploc, plane.Altitude))
                                        {
                                            plane.Location = temploc;
                                            plane.LastPositionTime = messageTime;
                                            plane.LocationReceivedBy = this;
                                        }
                                    }
                                    plane.Alert = sbs_data[18] == "-1";
                                    plane.Emergency = sbs_data[19] == "-1";
                                    plane.Ident = sbs_data[20] == "-1";
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "4":
                                    if (sbs_data[12] != "")
                                        plane.GroundSpeed = (int)Double.Parse(sbs_data[12]);
                                    if (sbs_data[13] != "")
                                        plane.Track = (int)Double.Parse(sbs_data[13]);
                                    if (sbs_data[16] != "")
                                        plane.VerticalRate = Int32.Parse(sbs_data[16]);
                                    break;
                                case "5":
                                    if (sbs_data[11] != "")
                                        plane.Altitude = Int32.Parse(sbs_data[11]);
                                    plane.Alert = sbs_data[18] == "-1";
                                    plane.Ident = sbs_data[20] == "-1";
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "6":
                                    if (sbs_data[11] != "")
                                        plane.Altitude = Int32.Parse(sbs_data[11]);
                                    plane.Squawk = sbs_data[17];
                                    plane.Alert = sbs_data[18] == "-1";
                                    plane.Emergency = sbs_data[19] == "-1";
                                    plane.Ident = sbs_data[20] == "-1";
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "7":
                                    if (sbs_data[11] != "")
                                        plane.Altitude = Int32.Parse(sbs_data[11]);
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "8":
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    rxBuffer = "";
                }


            }

        }
        public override string ToString()
        {
            return Name;
        }
    }
    public class BeastReceiver : Receiver
    {
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 30005;
        private EventDrivenTCPClient client;


        public BeastReceiver() { }
        public BeastReceiver(string Host)
        {
            this.Host = Host;
        }

        public BeastReceiver(string Host, int Port)
        {
            this.Host = Host;
            this.Port = Port;
        }

        public override void Start()
        {
            if (running)
                return;
            Enabled = true;
            client = new EventDrivenTCPClient(Host, Port, true);
            client.Connect();
            client.DataReceived += Client_DataReceived;
            running = true;
        }
        public override void Stop()
        {
            if (!running)
                return;
            Enabled = false;
            client.DataReceived -= Client_DataReceived;
            client.Disconnect();
            running = false;
        }
        
        bool running;

        public override string ToString()
        {
            return Name;
        }
   
        

        ReadWriteBuffer buffer = new ReadWriteBuffer(5000);
        private void Client_DataReceived(EventDrivenTCPClient sender, object data)
        {
            byte[] newdata;
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, data);
                newdata = ms.ToArray();
            }
            buffer.Write(newdata);
            byte[] message;
            byte[] timestamp;
            byte signalLevel;
            for (int i = 0; i < buffer.Count; i++)
            {
                if (buffer[i] == 0x1a)
                {
                    buffer.Read(i);
                    i = 0;
                    switch (buffer[1])
                    {
                        case 0x31:
                            buffer.Read(1);
                            timestamp = buffer.Read(6);
                            signalLevel = buffer.Read(1)[0];
                            message = buffer.Read(2);
                            break;
                        case 0x32:
                            buffer.Read(31);
                            timestamp = buffer.Read(6);
                            signalLevel = buffer.Read(1)[0];
                            message = buffer.Read(7);
                            ParseModeS(message);
                            break;
                        case 0x33:
                            buffer.Read(1);
                            timestamp = buffer.Read(6);
                            signalLevel = buffer.Read(1)[0];
                            message = buffer.Read(14);
                            ParseModeS(message);
                            break;
                        default:
                            buffer.Read(1);
                            break;
                    }
                }
            }

        }

        private void ParseModeS(byte[] message)
        {
            uint linkFmt = (uint)(message[0] & 0xF8 >> 3);
            int icaoAddr = int.MaxValue;
            if (linkFmt == 11 || linkFmt == 17 || linkFmt == 18)
            {
                icaoAddr = ((message[1] << 16) + (message[2] << 8) + message[3]);
            }
            Aircraft plane;

            if (icaoAddr != int.MaxValue)
            {
                lock (aircraft) plane = (from x in aircraft where x.ModeSCode == (int)icaoAddr select x).FirstOrDefault();
                {
                    if (plane == null)
                    {
                        plane = new Aircraft((int)icaoAddr);
                        aircraft.Add(plane);
                        Debug.WriteLine("Added airplane " + icaoAddr.ToString("X") + " from " + Host);
                    }
                    plane.LastMessageTime = DateTime.UtcNow;
                }
            }

        }
    }

}