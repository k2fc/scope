using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DGScope.Library;
using DGScope.Receivers;
using Newtonsoft.Json;

namespace DGScope.Receivers.ScopeServer
{
    public class ScopeServerClient : Receiver
    {
        private List<Track> tracks = new List<Track>();
        private List<FlightPlan> flightPlans = new List<FlightPlan>();
        private List<WeatherRadar> weatherRadars = new List<WeatherRadar>();
        private NexradDisplay weatherDisplay;
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
            aircraft.CollectionChanged += Aircraft_CollectionChanged;
        }

        private void Aircraft_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (Aircraft plane in e.NewItems)
                    {
                        plane.Update += Plane_Update;
                        plane.FpDeleted += Plane_FpDeleted; 
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (Aircraft plane in e.OldItems)
                    {
                        plane.Update -= Plane_Update;
                        plane.FpDeleted -= Plane_FpDeleted; 
                        
                    }
                    break;
            }
        }

        private void Plane_FpDeleted(object sender, EventArgs e)
        {
            DeletionUpdate del = new DeletionUpdate() { Guid = (sender as Aircraft).FlightPlanGuid };
            Send(del);
        }
        public override void SetWeatherRadarDisplay(NexradDisplay weatherRadar)
        {
            weatherDisplay = weatherRadar;
        }
        private byte[][] UnpackRLE(byte[] data, int rows, int cols)
        {
            if (data == null)
                return null;
            byte[][] output = new byte[rows][];
            for (int y = 0;  y < rows; y++)
            {
                output[y] = new byte[cols];
            }
            int gridsize = rows > cols ? rows : cols;
            byte[] unpacked = new byte[gridsize * gridsize];
            int unpackcount = 0;
            for (int i = 0; i < data.Length; i+=3)
            {
                var value = data[i];
                var countbytes = new byte[2] { data[i + 1], data[i + 2] };
                var count = BinaryPrimitives.ReadUInt16BigEndian(countbytes);
                for (int j = 0; j < count; j++)
                {
                    unpacked[unpackcount] = value;
                    unpackcount++;
                }
            }
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    var gridnum = (y * gridsize) + x;
                    output[y][x] = unpacked[gridnum];
                    if (unpacked[gridnum] != 0 && unpacked[gridnum] != 15)
                    {
                        ;
                    }
                }
            }
            return output;
        }
        private void Plane_Update(object sender, EventArgs e)
        {
            Aircraft plane = sender as Aircraft;
            FlightPlan flightPlan;
            lock (flightPlans)
                flightPlan = flightPlans.Where(x => x.Guid == plane.FlightPlanGuid).FirstOrDefault();
            if (flightPlan == null)
                return;
            FlightPlanUpdate upd = flightPlan.GetCompleteUpdate() as FlightPlanUpdate;
            upd.TimeStamp = RadarWindow.CurrentTime;
            upd.Callsign = plane.FlightPlanCallsign;
            upd.AircraftType = plane.Type;
            upd.PendingHandoff = plane.PendingHandoff;
            upd.Owner = plane.PositionInd;
            upd.AssociatedTrackGuid = plane.TrackGuid;
            upd.Scratchpad1 = plane.Scratchpad;
            upd.Scratchpad2 = plane.Scratchpad2;
            Send(upd);
        }

        private bool streamended = true;
        private static async Task<string> ReadString(ClientWebSocket ws)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[49316]);

            WebSocketReceiveResult result = null;
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);
                ms.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                    return reader.ReadToEnd();
            }
        }

        private async Task ProcessLine(string line)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            JsonUpdate obj = JsonConvert.DeserializeObject<JsonUpdate>(line, settings);
            Update update;
            switch (obj.UpdateType)
            {
                case 0:
                    update = JsonConvert.DeserializeObject<TrackUpdate>(line, settings);
                    ProcessUpdate(update);
                    break;
                case 1:
                    update = JsonConvert.DeserializeObject<FlightPlanUpdate>(line, settings);
                    ProcessUpdate(update);
                    break;
                case 2:
                    update = JsonConvert.DeserializeObject<DeletionUpdate>(line, settings);
                    ProcessUpdate(update);
                    break;
                case 3:
                    update = JsonConvert.DeserializeObject<WeatherRadarUpdate>(line, settings);
                    ProcessUpdate(update);
                    break;
            }
        }
        private async Task<bool> Receive()
        {
            /// Try some websocket
            Uri uri;
            if (Url.EndsWith("/updates"))
                uri = new Uri(Url);
            else if (Url.EndsWith("/"))
                uri = new Uri(Url + "updates");
            else
                uri = new Uri(Url + "/updates");
            NetworkCredential credentials = new NetworkCredential(Username, Password);
            
            var scheme = uri.GetLeftPart(UriPartial.Scheme);
            switch (scheme.ToLower())
            {
                case "wss://":
                case "ws://":
                    using (var client = new ClientWebSocket())
                    {
                        var cts = new CancellationTokenSource();
                        client.Options.Credentials = credentials;
                        client.Options.KeepAliveInterval = TimeSpan.FromMinutes(30);
                        client.ConnectAsync(uri, cts.Token);
                        while (client.State == WebSocketState.Connecting)
                        {
                            Thread.Sleep(1000);
                        }
                        Debug.WriteLine("connected!");
                        while (client.State == WebSocketState.Open)
                        {
                            Debug.WriteLine("reading a line");
                            
                            var line = await ReadString(client);
                            ProcessLine(line);
                        }
                    }
                    break;
                case "http://":
                case "https://":
                    using (var client = new WebClient())
                    {
                        client.Credentials = new NetworkCredential(Username, Password);
                        client.BaseAddress = Url;
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
                                            ProcessLine(line);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                            streamended = true;
                                            break;
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
                            client.OpenReadAsync(uri);
                            while (!streamended)
                            {
                                System.Threading.Thread.Sleep(1000);
                            }
                        }
                    }
                    break;
            }
            running = false;

            if (stop)
                return true;
            return false;
                
        }
        public async Task ProcessUpdate(Update update)
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
                case UpdateType.Deletion:
                    lock (flightPlans)
                    {
                        flightPlan = flightPlans.Where(x => x.Guid == updateGuid).FirstOrDefault();
                        if (flightPlan != null)
                        {
                            update = new FlightPlanUpdate();
                            flightPlans.Remove(flightPlan);
                            if (flightPlan.AssociatedTrack != null)
                            {
                                plane = GetPlane(flightPlan.AssociatedTrack.Guid, false);
                                track = flightPlan.AssociatedTrack;
                            }
                            flightPlan = new FlightPlan(Guid.Empty);
                            if (plane != null)
                            {
                                plane.Owned = false;
                                plane.FlightPlanCallsign = string.Empty;
                            }
                        }
                    }
                    lock (tracks)
                    {
                        track = tracks.Where(x => x.Guid == updateGuid).FirstOrDefault();
                        if (track != null)
                        {
                            flightPlans.ToList().ForEach(fp =>
                            {
                                if (fp.AssociatedTrack == track)
                                    fp.AssociateTrack(null);
                            });
                            tracks.Remove(track);
                            plane = GetPlane(updateGuid, false);
                        }
                    }
                    lock (aircraft)
                    {
                        if (plane != null && track != null)
                            aircraft.Remove(plane);
                    }
                    break;
                case UpdateType.WeatherRadar:
                    var wxradarupdate = update as WeatherRadarUpdate;
                    if (update != null)
                    {
                        var radar = GetWeatherRadar(wxradarupdate.RadarID, true);
                        radar?.Update(update);
                        var values = UnpackRLE(radar.Levels, radar.Rows, radar.Columns);
                        ScopeServerWxRadarReport report = new ScopeServerWxRadarReport()
                        {
                            BoxSize = radar.BoxSize,
                            GridSize = new System.Numerics.Vector2(radar.Columns, radar.Rows),
                            ReferencePoint = radar.ReferencePoint,
                            OriginOffset = radar.OffsetToOrigin,
                            Values = values,
                            Rotation = radar.Rotation
                        };
                        weatherDisplay.AddWeatherRadarReport(radar.RadarID, report);
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
                if (!string.IsNullOrEmpty(flightPlan.Callsign))
                    plane.FlightPlanCallsign = flightPlan.Callsign;
                plane.Destination = flightPlan.Destination;
                plane.FlightRules = flightPlan.FlightRules;
                plane.Category = flightPlan.WakeCategory;
                plane.PositionInd = flightPlan.Owner;
                plane.PendingHandoff = flightPlan.PendingHandoff;
                plane.RequestedAltitude = flightPlan.RequestedAltitude;
                plane.Scratchpad = flightPlan.Scratchpad1;
                plane.Scratchpad2 = flightPlan.Scratchpad2;
                plane.AssignedSquawk = flightPlan.AssignedSquawk;
                plane.OwnerLeaderDirection = RadarWindow.ParseLDR(flightPlan.LDRDirection.ToString());
                plane.FlightPlanGuid = flightPlan.Guid;
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
                if (!string.IsNullOrEmpty(track.Callsign))
                    plane.Callsign = track.Callsign;
                plane.GroundSpeed = track.GroundSpeed;
                if (update.GetType() == typeof(TrackUpdate))
                {
                    var tu = update as TrackUpdate;
                    if (tu.GroundTrack != null)
                        plane.SetTrack((double)tu.GroundTrack, tu.TimeStamp);
                    if (tu.Location != null)
                        plane.SetLocation(tu.Location, tu.TimeStamp);
                }
                plane.Ident = track.Ident;
                plane.IsOnGround = track.IsOnGround;
                plane.ModeSCode = track.ModeSCode;
                plane.Squawk = track.Squawk;
                plane.VerticalRate = track.VerticalRate;
            }
        }
        private WeatherRadar GetWeatherRadar(string id, bool addnew = false)
        {
            lock (weatherRadars)
            {
                var radar = weatherRadars.Where(x => x.RadarID == id).FirstOrDefault();
                if (radar is null && addnew)
                {
                    radar = new WeatherRadar();
                    weatherRadars.Add(radar);
                }
                return radar; 
            }
        }
        public async Task<bool> Send(Update update)
        {
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(Username, Password);
                update.RemoveUnchanged();
                var json = update.SerializeToJson();
                var uri = new Uri(Url + "update");
                client.Headers.Add("Content-Type", "application/json");
                client.UploadStringAsync(uri, json);
            }
            return true;
        }
        public override void Stop()
        {
            stop = true;
            if (aircraft != null) 
                aircraft.CollectionChanged -= Aircraft_CollectionChanged;
        }
    }
}
