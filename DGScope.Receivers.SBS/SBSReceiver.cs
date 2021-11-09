using System;
using System.Diagnostics;

namespace DGScope.Receivers.SBS
{
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
                                        plane.PressureAltitude = Int32.Parse(sbs_data[11]);
                                    if (sbs_data[12] != "")
                                        plane.GroundSpeed = (int)Double.Parse(sbs_data[12]);
                                    if (sbs_data[13] != "")
                                        plane.Track = (int)Double.Parse(sbs_data[13]);
                                    if (sbs_data[14] != "" && sbs_data[15] != "" && messageTime > plane.LastPositionTime)
                                    {
                                        var latitude = Double.Parse(sbs_data[14]);
                                        var longitude = Double.Parse(sbs_data[15]);
                                        var temploc = new GeoPoint(latitude, longitude);
                                        if (InRange(temploc, plane.PressureAltitude))
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
                                        plane.PressureAltitude = Int32.Parse(sbs_data[11]);
                                    if (sbs_data[14] != "" && sbs_data[15] != "" && messageTime > plane.LastPositionTime)
                                    {
                                        var latitude = Double.Parse(sbs_data[14]);
                                        var longitude = Double.Parse(sbs_data[15]);
                                        var temploc = new GeoPoint(latitude, longitude);
                                        if (InRange(temploc, plane.PressureAltitude))
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
                                        plane.PressureAltitude = Int32.Parse(sbs_data[11]);
                                    plane.Alert = sbs_data[18] == "-1";
                                    plane.Ident = sbs_data[20] == "-1";
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "6":
                                    if (sbs_data[11] != "")
                                        plane.PressureAltitude = Int32.Parse(sbs_data[11]);
                                    plane.Squawk = sbs_data[17];
                                    plane.Alert = sbs_data[18] == "-1";
                                    plane.Emergency = sbs_data[19] == "-1";
                                    plane.Ident = sbs_data[20] == "-1";
                                    plane.IsOnGround = sbs_data[21] == "-1";
                                    break;
                                case "7":
                                    if (sbs_data[11] != "")
                                        plane.PressureAltitude = Int32.Parse(sbs_data[11]);
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

}
