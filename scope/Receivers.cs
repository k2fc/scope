using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace DGScope.Receivers
{
    public interface IReceiver
    {
        //List<Aircraft> Aircraft { get;  }
        GeoPoint Location { get; set; }
        void Start();
        void Stop();
        void Restart();
        void SetAircraftList(List<Aircraft> Aircraft);

        bool Enabled { get; set; }
        string Name { get; set; }
        
    }

   public class SBSReceiver : IReceiver
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 30003;
        public GeoPoint Location { get; set; } = new GeoPoint(0, 0);
        private List<Aircraft> aircraft;
        private EventDrivenTCPClient client;

        public void SetAircraftList(List<Aircraft> Aircraft)
        {
            this.aircraft = Aircraft;
        }

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
        public void Start()
        {
            if (running)
                return;
            Enabled = true;
            client = new EventDrivenTCPClient(Host, Port, true);
            client.DataReceived += Client_DataReceived;
            client.Connect();
            running = true;
        }
        public void Stop()
        {
            if (!running)
                return;
            Enabled = false;
            client.Disconnect();
            while (client.ConnectionState != EventDrivenTCPClient.ConnectionStatus.DisconnectedByUser) { }
            //client.DataReceived -= Client_DataReceived;

            running = false;
        }
        public void Restart()
        {
            Stop();
            Start();
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
                            Aircraft plane;
                            lock (aircraft)
                            {
                                plane = (from x in aircraft where x.ModeSCode == icaoID select x).FirstOrDefault();
                                if (plane == null)
                                {
                                    plane = new Aircraft(icaoID);
                                    aircraft.Add(plane);
                                    Debug.WriteLine("Added airplane " + sbs_data[4] + " from " + Host);
                                }
                            }
                            //DateTime messageTime = DateTime.Parse(sbs_data[6] + " " + sbs_data[7]);
                            DateTime messageTime = DateTime.UtcNow;
                            if (plane.LastMessageTime < messageTime)
                                plane.LastMessageTime = messageTime;
                            plane.ModeSCode = icaoID;
                            //Debug.WriteLine(message);
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
                                    if (sbs_data[14] != "" && messageTime > plane.LastPositionTime)
                                        plane.Latitude = Double.Parse(sbs_data[14]);
                                    if (sbs_data[15] != "" && messageTime > plane.LastPositionTime)
                                    {
                                        plane.Longitude = Double.Parse(sbs_data[15]);
                                        plane.TargetChar = '/';
                                        plane.LastPositionTime = messageTime;
                                        plane.LocationReceivedBy = this;
                                    }
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "3":
                                    if (sbs_data[11] != "")
                                        plane.Altitude = Int32.Parse(sbs_data[11]);
                                    if (sbs_data[14] != "" && messageTime > plane.LastPositionTime)
                                        plane.Latitude = Double.Parse(sbs_data[14]);
                                    if (sbs_data[15] != "" && messageTime > plane.LastPositionTime)
                                    {
                                        plane.Longitude = Double.Parse(sbs_data[15]);
                                        plane.TargetChar = '/';
                                        plane.LastPositionTime = messageTime;
                                        plane.LocationReceivedBy = this;
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
    public class BeastReceiver : IReceiver
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 30005;
        public GeoPoint Location { get; set; } = new GeoPoint(0, 0);
        private List<Aircraft> aircraft;
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

        public void Start()
        {
            if (running)
                return;
            Enabled = true;
            client = new EventDrivenTCPClient(Host, Port, true);
            client.Connect();
            client.DataReceived += Client_DataReceived;
            running = true;
        }
        public void Stop()
        {
            if (!running)
                return;
            Enabled = false;
            client.DataReceived -= Client_DataReceived;
            client.Disconnect();
            running = false;
        }
        public void Restart()
        {
            Stop();
            Start();
        }
        bool running;

        public override string ToString()
        {
            return Name;
        }
   
        public void SetAircraftList(List<Aircraft> Aircraft)
        {
            this.aircraft = Aircraft;
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