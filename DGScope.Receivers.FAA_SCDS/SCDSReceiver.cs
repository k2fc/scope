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
        [PasswordPropertyText(true)]
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
            session = new Session(connection);
            receiver = new ReceiverLink(session, "amqpConsumer", Queue);

            if (!Forever)
                timeout = TimeSpan.FromSeconds(ClientTimeout);
            while (!stop)
            {
                var message = receiver.Receive(timeout);
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
                        if (stop)
                            return false;
                        Aircraft plane = null; 
                        if (record.track != null)
                        {
                            int address = 0;
                            var addressString = record.track.acAddress;
                            if (addressString != "" )
                                address = Convert.ToInt32(record.track.acAddress, 16);
                            if (address != 0)
                                plane = GetPlane(address);
                        }
                        if (plane == null && record.flightPlan.assignedBeaconCode != 0)
                        {
                            plane = GetPlaneBySquawk(record.flightPlan.assignedBeaconCode.ToString("0000"));
                        }
                        if (plane == null)
                            continue;
                        lock (plane)
                        {
                            plane.Type = record.flightPlan.acType;
                            plane.LDRDirection = RadarWindow.ParseLDR(record.flightPlan.lld);
                            plane.Scratchpad = record.flightPlan.scratchPad1;
                            plane.Runway = record.flightPlan.runway;
                            plane.Scratchpad2 = record.flightPlan.scratchPad2;
                            plane.RequestedAltitude = record.flightPlan.requestedAltitude;
                            plane.Category = record.flightPlan.category;
                            if (record.flightPlan.exitFix != null)
                                plane.Destination = record.flightPlan.exitFix;
                            else
                                plane.Destination = record.flightPlan.airport;
                            plane.FlightRules = record.flightPlan.flightRules;
                            plane.LastMessageTime = record.track.mrtTime;
                            if (record.track != null)
                            {
                                if (record.track.reportedBeaconCode > 0)
                                    plane.Squawk = record.track.reportedBeaconCode.ToString("0000");

                                if (record.track.mrtTime > plane.LastPositionTime)
                                {
                                    plane.SetLocation((double)record.track.lat, (double)record.track.lon, record.track.mrtTime);
                                    double vx = record.track.vx;
                                    double vy = record.track.vy;
                                    var speed = Math.Sqrt((vx * vx) + (vy * vy));
                                    double angle = Math.Atan2(vy, vx) * (180 / Math.PI);
                                    double track = 0;
                                    if (!double.IsNaN(angle))
                                        track = (450 - angle) % 360;
                                    plane.SetTrack(track, record.track.mrtTime);
                                    plane.GroundSpeed = (int)speed;
                                    plane.Squawk = record.track.reportedBeaconCode.ToString();
                                    plane.Altitude.TrueAltitude = record.track.reportedAltitude;

                                }
                            }
                            plane.FlightPlanCallsign = record.flightPlan.acid.Trim();

                            if (plane.Callsign == null)
                                plane.Callsign = record.flightPlan.acid.Trim();
                            /*if (record.flightPlan.status == "drop" || record.flightPlan.delete != 0)
                            {
                                plane.DropTrack();
                            }*/
                            else
                            {
                                switch (record.flightPlan.ocr)
                                {
                                    case "intrafacility handoff":
                                        if (plane.QuickLook)
                                            plane.QuickLook = false;
                                        plane.PositionInd = record.flightPlan.cps;
                                        break;
                                    case "normal handoff":
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
                                        plane.PositionInd = record.flightPlan.cps;
                                        break;
                                }
                            }
                            
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(message);
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

        bool stop = false;

        public override void Stop()
        {
            stop = true;
            if (receiver != null)
                receiver.Close();
            if (session != null)
                session.Close();
            if (connection != null)
                connection.Close();
        }
        public SCDSReceiver() { }
    }
}
