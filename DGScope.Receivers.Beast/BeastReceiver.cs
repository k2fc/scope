using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DGScope.Receivers.Beast
{
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
            if (icaoAddr != int.MaxValue)
            {
                Aircraft plane = GetPlane(icaoAddr);
            }

        }
    }

}
