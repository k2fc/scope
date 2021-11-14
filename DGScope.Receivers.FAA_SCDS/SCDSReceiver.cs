using Amqp;
using Amqp.Sasl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms.Design;
using System.IO;

namespace DGScope.Receivers.FAA_SCDS
{
    public class SCDSReceiver : Receiver
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Queue { get; set; }
        public bool Forever { get; set; } = true;
        public int ClientTimeout { get; set; } = 5000;
        public int InitialCredit { get; set; } = 5;
        public string CertificatePath { get; set; } = @"scds.cert";

        TimeSpan timeout = TimeSpan.MaxValue;

        Connection connection = null;
        Session session;
        ReceiverLink receiver;
        public override void Start()
        {
            Address address = new Address(Host, 5668, Username, Password, "/", "amqps");

            ConnectionFactory factory = new ConnectionFactory();
            factory.SSL.ClientCertificates.Add(new X509Certificate(CertificatePath));
            factory.SASL.Profile = SaslProfile.External;
            factory.SSL.RemoteCertificateValidationCallback = ValidateServerCertificate;
            connection = factory.CreateAsync(address).Result;

            Task.Run(ReceiveMessage);
        }

        private async Task<bool> ReceiveMessage()
        {
            Session session = new Session(connection);
            receiver = new ReceiverLink(session, "amqpConsumer", Queue);

            if (!Forever)
                timeout = TimeSpan.FromSeconds(ClientTimeout);
            string path = @"messages.xml";
            using (var sw = File.CreateText(path)) { } ;
            while (true)
            {
                var message = receiver.Receive(timeout);
                Console.WriteLine("Received message from " + Name);
                if (message == null)
                    continue;
                receiver.Accept(message);
                try
                {
                    
                    TATrackAndFlightPlan data = XmlSerializer<TATrackAndFlightPlan>.Deserialize(message.Body.ToString());
                    if (data.record == null)
                        continue;
                    foreach (var record in data.record)
                    {
                        if (record.flightPlan == null)
                            continue;
                        Console.WriteLine("Processing record for {0} from {1}", record.flightPlan.acid, Name);
                        Aircraft plane = GetPlaneBySquawk(record.flightPlan.assignedBeaconCode.ToString("0000"));
                        if (plane == null)
                            plane = GetPlane(Convert.ToInt32(record.track.acAddress, 16));
                        lock (plane)
                        {
                            plane.Type = record.flightPlan.acType;
                            plane.LDRDirection = RadarWindow.ParseLDR(record.flightPlan.lld);
                            plane.Scratchpad = record.flightPlan.scratchPad1;
                            plane.Runway = record.flightPlan.runway;
                            plane.Scratchpad2 = record.flightPlan.scratchPad2;
                            if (record.flightPlan.exitFix != null)
                                plane.Destination = record.flightPlan.exitFix;
                            else
                                plane.Destination = record.flightPlan.airport;
                            plane.FlightRules = record.flightPlan.flightRules;
                            plane.Squawk = record.track.reportedBeaconCode.ToString("0000");
                            if (record.track.mrtTime > plane.LastPositionTime)
                            {
                                plane.Location = new GeoPoint((double)record.track.lat, (double)record.track.lon);
                                plane.LastPositionTime = record.track.mrtTime;
                                plane.LocationReceivedBy = this;
                            }
                            if (plane.Callsign == null)
                                plane.Callsign = record.flightPlan.acid;
                            switch (record.flightPlan.ocr)
                            {
                                case "normal handoff":
                                case "intrafacility handoff":
                                case "manual":
                                case "no change":
                                case "consolidation":
                                case "directed handoff":
                                    plane.PositionInd = record.flightPlan.cps;
                                    break;
                                case "pending":
                                    plane.PendingHandoff = record.flightPlan.cps;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(message.Body);
                    }
                }
                
            }
            return true;
        }
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                return true;
            else
                return false;
        }



        public override void Stop()
        {
            receiver.Close();
            session.Close();
            connection.Close();
        }
        public SCDSReceiver() { }
    }
}
