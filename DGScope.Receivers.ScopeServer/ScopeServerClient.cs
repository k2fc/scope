using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DGScope.Receivers;
using Newtonsoft.Json;

namespace DGScope.Receivers.ScopeServer
{
    public class ScopeServerClient : Receiver
    {
        private Dictionary<Guid, Guid> associatedFlightPlans = new Dictionary<Guid, Guid>();
        private bool stop = true;
        private bool running = false;
        public string Url { get; set; }
        public string Username { get; set; }
        [PasswordPropertyText(true)]
        public string Password { get; set; }
        public override void Start()
        {
            if (running)
                return;
            running = true;
            stop = false;
            Task.Run(Receive);
        }

        private bool streamended = true;
        private async Task<bool> Receive()
        {
            using (var client = new WebClient())
            {
                client.OpenReadCompleted += (sender, e) =>
                {
                    using (var reader = new StreamReader(e.Result))
                    {
                        while (!stop)
                        {
                            try
                            {
                                var line = reader.ReadLine();
                                if (line == null)
                                    continue;
                                JsonUpdate obj = JsonConvert.DeserializeObject(line, typeof(JsonUpdate)) as JsonUpdate;
                                ProcessUpdate(obj);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        streamended = true;
                    }
                };
                
                while (!stop)
                {
                    streamended = false;
                    client.OpenReadAsync(new Uri(Url));
                    while (!streamended)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
                running = false;

            if (stop)
                return true;
            return false;
                
        }
        public void ProcessUpdate(JsonUpdate update)
        {
            Aircraft plane = null; ;
            Guid updateGuid;
            switch (update.UpdateType)
            {
                case 0:
                    updateGuid = update.Guid;
                    break;
                case 1 when update.AssociatedTrackGuid != null:
                    updateGuid = (Guid)update.AssociatedTrackGuid;
                    if (!associatedFlightPlans.ContainsKey(update.Guid))
                        associatedFlightPlans.Add(update.Guid, (Guid)update.AssociatedTrackGuid);
                    break;
                default:
                    if (!associatedFlightPlans.TryGetValue(update.Guid, out updateGuid))
                        return;
                    break;
            }
            plane = GetPlane(updateGuid, true);
            if (plane == null)
                return;
            //switch (update.UpdateType)
            //{
            //    case 0 when update.TimeStamp < plane.LastPositionTime:
            //        return;
            //    case 1 when update.TimeStamp < plane.LastMessageTime:
            //        return;
            //}
            plane.LastMessageTime = update.TimeStamp;
            if (update.AircraftType != null)
                plane.Type = update.AircraftType;
            if (update.Altitude != null)
            {
                plane.Altitude.AltitudeType = (AltitudeType)update.Altitude.AltitudeType;
                plane.Altitude.Value = update.Altitude.Value;
            }
            if (update.Callsign != null)
                plane.Callsign = update.Callsign;
            if (update.Destination != null)
                plane.Destination = update.Destination;
            if (update.FlightRules != null)
                plane.FlightRules = update.FlightRules;
            if (update.WakeCategory != null)
                plane.Category = update.WakeCategory;
            if (update.GroundSpeed != null)
                plane.GroundSpeed = (int)update.GroundSpeed;
            if (update.GroundTrack != null)
                plane.SetTrack((int)update.GroundTrack, update.TimeStamp);
            if (update.Ident != null)
                plane.Ident = (bool)update.Ident;
            if (update.IsOnGround != null)
                plane.IsOnGround = (bool)update.IsOnGround;
            if (update.LDRDirection != null)
                plane.LDRDirection = LDRDirection.ParseLDR(update.LDRDirection);
            if (update.Location != null)
                plane.SetLocation(update.Location.Latitude, update.Location.Longitude, update.TimeStamp);
            if (update.ModeSCode != null)
                plane.ModeSCode = (int)update.ModeSCode;
            if (update.Owner != null)
                plane.PositionInd = update.Owner;
            if (update.PendingHandoff != null)
                plane.PendingHandoff = update.PendingHandoff;
            if (update.RequestedAltitude != null)
                plane.RequestedAltitude = (int)update.RequestedAltitude;
            if (!string.IsNullOrEmpty(update.Scratchpad1))
                plane.Scratchpad = update.Scratchpad1;
            else if (update.Scratchpad1 == string.Empty)
                plane.Scratchpad = null;
            if (!string.IsNullOrEmpty(update.Scratchpad2))
                plane.Scratchpad2 = update.Scratchpad2;
            else if (update.Scratchpad2 == string.Empty)
                plane.Scratchpad2 = null;
            if (update.Squawk != null)
                plane.Squawk = update.Squawk;
            if (update.VerticalRate != null)
                plane.VerticalRate = (int)update.VerticalRate;
                


        }

        public override void Stop()
        {
            stop = true;
        }
    }
}
