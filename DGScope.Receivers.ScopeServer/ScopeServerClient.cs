using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DGScope.Library;
using DGScope.Receivers;
using libmetar;
using Newtonsoft.Json;

namespace DGScope.Receivers.ScopeServer
{
    public class ScopeServerClient : Receiver
    {
        private Dictionary<Guid, Guid> associatedFlightPlans = new Dictionary<Guid, Guid>();
        private List<Track> tracks = new List<Track>();
        private List<FlightPlan> flightPlans = new List<FlightPlan>();
        private UpdateConverter updateConverter = new UpdateConverter();
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
                client.Credentials = new NetworkCredential(Username, Password);
                client.OpenReadCompleted += (sender, e) =>
                {
                    if (e.Error == null)
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
                                    JsonUpdate obj = JsonConvert.DeserializeObject<JsonUpdate>(line);
                                    Update update;
                                    switch (obj.UpdateType)
                                    {
                                        case 0:
                                            update = JsonConvert.DeserializeObject<TrackUpdate>(line);
                                            ProcessUpdate(update);
                                            break;
                                        case 1:
                                            update = JsonConvert.DeserializeObject<FlightPlanUpdate>(line);
                                            ProcessUpdate(update);
                                            break;
                                    }
                                    //ProcessUpdate(obj);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(e.Error.ToString());
                    }
                    streamended = true;
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
        public void ProcessUpdate(Update update)
        {
            Aircraft plane = null; ;
            Guid updateGuid = update.Guid;
            Track track = null;
            FlightPlan flightPlan = null;
            switch (update.UpdateType)
            {
                case UpdateType.Track:
                    lock (tracks)
                    {
                        track = tracks.Where(x => x.Guid == updateGuid).FirstOrDefault();
                        if (track == null)
                        {
                            track = new Track(updateGuid);
                            tracks.Add(track);
                        }
                    }
                    track.UpdateTrack(update as TrackUpdate);
                    flightPlan = flightPlans.Where(x => x.AssociatedTrack == track).FirstOrDefault();
                    plane = GetPlane(updateGuid, true);
                    break;
                case UpdateType.Flightplan:
                    lock (flightPlans)
                    {
                        flightPlan = flightPlans.Where(x => x.Guid == updateGuid).FirstOrDefault();
                        if (flightPlan == null)
                        {
                            flightPlan = new FlightPlan(updateGuid);
                            flightPlans.Add(flightPlan);
                        }
                    }
                    flightPlan.UpdateFlightPlan(update as FlightPlanUpdate);
                    var associatedTrack = (update as FlightPlanUpdate).AssociatedTrackGuid;
                    if (associatedTrack != null)
                        flightPlan.AssociateTrack(tracks.Where(x => x.Guid == associatedTrack).FirstOrDefault());
                    if (flightPlan.AssociatedTrack != null)
                    {
                        plane = GetPlane(flightPlan.AssociatedTrack.Guid, false);
                        track = flightPlan.AssociatedTrack;
                    }
                    break;
            }
            if (plane == null)
                return;
            
            if (update.TimeStamp > plane.LastMessageTime)
                plane.LastMessageTime = update.TimeStamp;
            if (flightPlan != null)
            {
                plane.Type = flightPlan.AircraftType;
                plane.FlightPlanCallsign = flightPlan.Callsign;
                plane.Destination = flightPlan.Destination;
                plane.FlightRules = flightPlan.FlightRules;
                plane.Category = flightPlan.WakeCategory;
                plane.PositionInd = flightPlan.Owner;
                plane.PendingHandoff = flightPlan.PendingHandoff;
                plane.RequestedAltitude = flightPlan.RequestedAltitude;
                plane.Scratchpad = flightPlan.Scratchpad1;
                plane.Scratchpad2 = flightPlan.Scratchpad2;
                plane.LDRDirection = RadarWindow.ParseLDR(flightPlan.LDRDirection.ToString());
            }
            if (track != null)
            {
                if (track.Altitude != null)
                {
                    if (plane.Altitude != null)
                    {
                        plane.Altitude.Value = track.Altitude.Value;
                        plane.Altitude.AltitudeType = track.Altitude.AltitudeType;
                    }
                }
                plane.Callsign = track.Callsign;
                plane.GroundSpeed = track.GroundSpeed;
                if (track.PropertyUpdatedTimes.ContainsKey(track.GetType().GetProperty("GroundTrack")))
                    plane.SetTrack(track.GroundTrack, track.PropertyUpdatedTimes[track.GetType().GetProperty("GroundTrack")]);
                plane.Ident = track.Ident;
                plane.IsOnGround = track.IsOnGround;
                if (track.PropertyUpdatedTimes.ContainsKey(track.GetType().GetProperty("Location")))
                    plane.SetLocation(track.Location, track.PropertyUpdatedTimes[track.GetType().GetProperty("Location")]);
                plane.ModeSCode = track.ModeSCode;
                plane.Squawk = track.Squawk;
                plane.VerticalRate = track.VerticalRate;
            }
        }

        public override void Stop()
        {
            stop = true;
        }
    }
}
