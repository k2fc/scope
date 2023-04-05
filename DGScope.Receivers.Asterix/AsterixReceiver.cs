using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DGScope.Receivers;
using System.Xml.Linq;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using NexradDecoder;

namespace DGScope.Receivers.Asterix
{
    public class AsterixReceiver : Receiver
    {
        public int Port { get; set; } = 30008;
        public string Address { get; set; } 
        public AsteriskReceiverMode Mode { get; set; }
        UdpClient udpclient;
        TcpClient tcpclient;
        public AsterixReceiver() { }
        
        public override void Start()
        {
            if (running)
                return;
            switch (Mode)
            {
                case AsteriskReceiverMode.UdpUnicast:
                    running = startUdpReceiver();
                    break;
                case AsteriskReceiverMode.UdpMulticast:
                    running = startUdpReceiver();
                    if (IPAddress.TryParse(Address, out IPAddress ip))
                        udpclient.JoinMulticastGroup(ip);
                    else
                        Stop();
                    break;
                case AsteriskReceiverMode.Tcp:
                    running = startTcpClient();
                    break;
            }
            
        }
        private bool startTcpClient()
        {
            return false;
        }
        private bool startUdpReceiver()
        {
            try
            {
                if (udpclient == null)
                    udpclient = new UdpClient(Port);
                udpclient.BeginReceive(new AsyncCallback(recv), null);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }
        public override void Stop()
        {
            running = false;
            if (udpclient != null)
                udpclient.Close();
            if (tcpclient != null)
                tcpclient.Close();
        }

        bool running;

        public override string ToString()
        {
            return Name;
        }
        
        private void recv(IAsyncResult result) 
        {
            if (!running)
                return;
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, Port);
            byte[] received = udpclient.EndReceive(result,ref ep);
            udpclient.BeginReceive(new AsyncCallback(recv), null);
            parseAsteriskPacket(received);
        }
        private async void parseAsteriskPacket(byte[] data)
        {
            ushort len = (ushort)((data[1] << 8) + data[2]);
            byte[] thisMessage = new byte[len];
            byte[] theRest = new byte[data.Length - len];
            for (int i = 0; i < data.Length; i++)
            {
                if (i >= len)
                    theRest[i - len] = data[i];
                else
                    thisMessage[i] = data[i];
            }
            if (theRest.Length > 0)
            {
                parseAsteriskPacket(theRest);
            }
            parseAsteriskMessage(thisMessage);
        }
        private async void parseAsteriskMessage(byte[] data)
        {
            byte category = data[0];
            int p = 3;
            bool[] fspec = readFspec(data, ref p);
            DateTime velocityTime = DateTime.MinValue;
            Aircraft plane;
            switch (category)
            {
                case 21: // ADS-B Message
                    double? latitude = null;
                    double? longitude = null;
                    if (!fspec[10]) // no address.  this is useless to us
                        return;
                    if (fspec[0]) // I021/010 Data Source Identification
                    {
                        p += 2;
                    }
                    if (fspec[1]) // I021/040 Target Report Descriptor
                    {
                        readFspec(data, ref p);
                    }
                    if (fspec[2]) // I021/161 Track Number
                    {
                        p += 2;
                    }
                    if (fspec[3]) // I021/015 Service Identification 
                    {
                        p += 1;
                    }
                    if (fspec[4]) // I021/071 Time of Applicability for Position 3
                    {
                        p += 3;
                    }
                    if (fspec[5]) // I021/130 Position in WGS-84 co-ordinates
                    {
                        p += 8;
                    }
                    if (fspec[6]) // I021/131 Position in WGS-84 co-ordinates, high res.
                    {
                        int lat = data[p] << 24;
                        lat += data[p + 1] << 16;
                        lat += data[p + 2] << 8;
                        lat += data[p + 3];
                        int lon = data[p + 4] << 24;
                        lon += data[p + 5] << 16;
                        lon += data[p + 6] << 8;
                        lon += data[p + 7];
                        latitude = lat * (180 / Math.Pow(2, 30));
                        longitude = lon * (180 / Math.Pow(2, 30));
                        p += 8;
                    }
                    if (fspec[7]) // I021/072 Time of Applicability for Velocity
                    {
                        p += 3;
                    }
                    if (fspec[8]) // I021/150 Air Speed
                    {
                        p += 2;
                    }
                    if (fspec[9]) // I021/151 True Air Speed
                    {
                        p += 2;
                    }
                                  // I021/080 Target Address
                    int addr = (data[p] << 16) + (data[p + 1] << 8) + data[p + 2];
                    p += 3;
                    plane = GetPlane(addr, CreateNewAircraft);
                    if (plane == null)
                        return;
                    if (fspec[11]) // I021 / 073 Time of Message Reception of Position
                    {
                        if (latitude.HasValue && longitude.HasValue)
                        {
                            
                            var time = decodeTime(data, ref p);
                            if (fspec[12]) //  I021/074 Time of Message Reception of Position–High Precision
                            {
                                decodeHighPrecisionTime(ref time, data, ref p);
                            }
                            plane.SetLocation(latitude.Value, longitude.Value, time);
                        }
                    }
                    if (fspec[13]) // I021/075 Time of Message Reception of Velocity
                    {
                        
                        velocityTime = decodeTime(data, ref p);
                        if (fspec[14]) //  I021/076 Time of Message Reception of Velocity-High Precision
                        {
                            decodeHighPrecisionTime(ref velocityTime, data, ref p);
                        }
                    }
                    if (fspec[15]) // I021/140 Geometric Height
                    {
                        double alt = ((data[p] << 8) + data[p + 1]) + 6.25;
                        plane.Altitude.TrueAltitude = (int)alt;
                        p += 2;
                    }
                    if (fspec[16]) // I021/090 Quality Indicators
                    {
                        readFspec(data, ref p);
                    }
                    if (fspec[17]) // I021/210 MOPS Version 
                    {
                        p++;
                    }
                    if (fspec[18]) // I021/070 Mode 3/A Code
                    {
                        plane.Squawk = decodeSquawk(data, ref p);
                    }
                    if (fspec[19]) // I021/230 Roll Angle
                    {
                        p += 2;
                    }
                    if (fspec[20]) // I021/145 Flight Level
                    {
                        int alt = ((data[p] << 8) + data[p + 1]) * 25;
                        p += 2;
                        plane.Altitude.PressureAltitude = alt;
                    }
                    if (fspec[21]) // I021/152 Magnetic Heading
                    {
                        p += 2;
                    }
                    if (fspec[22]) // I021/200 Target Status
                    {
                        plane.Ident = (data[p] & 0x3) == 3;
                        plane.Emergency = (data[p] & 0x1c) != 0;
                        p += 1;
                    }
                    if (fspec[23]) // ID021/155 Barometric Vertical Rate
                    {
                        if ((data[p] & 0x80) == 0)
                        {
                            short rate = (short)((short)(data[p] << 9) + (data[p + 1] << 1));
                            plane.VerticalRate = (int)(rate * 3.125);
                        }
                        p += 2;
                    }
                    if (fspec[24]) // ID021/157 Geometric Vertical Rate
                    {
                        if ((data[p] & 0x80) == 0)
                        {
                            short rate = (short)((short)(data[p] << 9) + (data[p + 1] << 1));
                            plane.VerticalRate = (int)(rate * 3.125);
                        }
                        p += 2;
                    }
                    if (fspec[25]) // ID021/160 Airborne Ground Vector
                    {
                        if ((data[p] & 0x80) == 0)
                        {
                            uint speed = (uint)((data[p] << 8) + data[p + 1]);
                            plane.GroundSpeed = (int)(speed * (Math.Pow(2, -14) * 3600));
                        }
                        p += 2;
                        uint track = (uint)((data[p] << 8) + data[p + 1]);
                        p += 2;
                        if (velocityTime != DateTime.MinValue)
                            plane.SetTrack(track * .0055, velocityTime);
                    }
                    if (fspec[26]) // ID021/165 Track Angle Rate
                    {
                        p += 2;
                    }
                    if (fspec[27]) // ID021/077 Time of Report Transmission
                    {
                        var time = decodeTime(data, ref p);
                        if (plane.LastMessageTime < time)
                            plane.LastMessageTime = time;
                    }
                    else if (plane.LastMessageTime < RadarWindow.CurrentTime)
                    {
                        plane.LastMessageTime = RadarWindow.CurrentTime;
                    }
                    if (fspec[28]) // ID021/170 Target Identification
                    {
                        ulong encoded_cs = ((ulong)data[p] << 40) + ((ulong)data[p + 1] << 32) + ((ulong)data[p + 2] << 24) + ((ulong)data[p + 3] << 16) 
                            + ((ulong)data[p + 4] << 8) + data[p + 5];
                        
                        const string ais_charset = " ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_ !\"#$%&'()*+,-./0123456789:;<=>?";
                        char[] chars = new char[8];
                        chars[0] = ais_charset[(int)((encoded_cs & 0xFC0000000000) >> 42)];
                        chars[1] = ais_charset[(int)((encoded_cs & 0x3F000000000) >> 36)];
                        chars[2] = ais_charset[(int)((encoded_cs & 0xFC0000000) >> 30)];
                        chars[3] = ais_charset[(int)((encoded_cs & 0x3F000000) >> 24)];
                        chars[4] = ais_charset[(int)((encoded_cs & 0xFC0000) >> 18)];
                        chars[5] = ais_charset[(int)((encoded_cs & 0x3F000) >> 12)];
                        chars[6] = ais_charset[(int)((encoded_cs & 0xFC0) >> 6)];
                        chars[7] = ais_charset[(int)(encoded_cs & 0x3F)];
                        plane.Callsign = new string(chars).Trim();
                    }
                    break;
                    
            }
        }
        private static string decodeSquawk(byte[] data, ref int p)
        {
            ushort code = (ushort)((data[p] << 8) + data[p + 1]);
            p += 2;
            char a = ((code & 0xe00) >> 9).ToString().ToCharArray()[0];
            char b = ((code & 0x1c0) >> 6).ToString().ToCharArray()[0];
            char c = ((code & 0x38) >> 3).ToString().ToCharArray()[0];
            char d = (code & 0x7).ToString().ToCharArray()[0];
            return new string(new char[] { a, b, c, d });
        }
        private static void decodeHighPrecisionTime(ref DateTime time, byte[] data, ref int p)
        {
            byte fsi = (byte)((data[p] & 0xC0) >> 6);
            double offset = ((data[p] & 0x3f) << 24) + (data[p + 1] << 16) + (data[p + 2] << 8) + data[p + 3];
            p += 4;
            
            offset = offset * Math.Pow(2, -30);

            var wholesecond = new DateTime(time.Ticks - (time.Ticks % TimeSpan.TicksPerSecond), time.Kind);

            switch(fsi)
            {
                case 1:
                    wholesecond = wholesecond.AddSeconds(1);
                    break;
                case 2:
                    wholesecond = wholesecond.AddSeconds(-1);
                    break;
            }
            time = wholesecond.AddSeconds(offset);
        }
        private static DateTime decodeTime(byte[] data, ref int p)
        {
            int time = (data[p] << 16) + (data[p + 1] << 8) + data[p + 2];
            p += 3;
            DateTime dt = RadarWindow.CurrentTime.Date;
            dt = dt.AddSeconds(time / 128d);
            if ((dt - RadarWindow.CurrentTime).TotalDays >= 0.5)
            {
                dt = dt.AddDays(-1);
            }
            else if (((dt - RadarWindow.CurrentTime).TotalDays <= -0.5))
            {
                dt = dt.AddDays(1);
            }

            return dt;
        }
        private static bool[] readFspec(byte[] data, ref int offset)
        {
            bool[] fs = new bool[(data.Length - offset) * 8];
            int i = offset;
            int b = 0;
            do
            {
                fs[b + 0] = (data[i] & 0b10000000) != 0;
                fs[b + 1] = (data[i] & 0b01000000) != 0;
                fs[b + 2] = (data[i] & 0b00100000) != 0;
                fs[b + 3] = (data[i] & 0b00010000) != 0;
                fs[b + 4] = (data[i] & 0b00001000) != 0;
                fs[b + 5] = (data[i] & 0b00000100) != 0;
                fs[b + 6] = (data[i] & 0b00000010) != 0;
                i++;
                b += 7;
            } while ((data[i - 1] & 0x1) != 0);
            for (int j = 8 * (i - offset); j < fs.Length; j++)
            {
                fs[j] = false;
            }
            offset = i;
            return fs;
        }
        
    }
    public enum AsteriskReceiverMode
    {
        UdpUnicast, UdpMulticast, Tcp
    }
}

