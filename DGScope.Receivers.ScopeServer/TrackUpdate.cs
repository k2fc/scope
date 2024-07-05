using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Reflection;
using ProtoBuf;

namespace DGScope.Library
{
    [ProtoContract]
    public class TrackUpdate : Update
    {
        [ProtoMember(3, IsRequired = false)]
        public Altitude Altitude { get; set; }
        [ProtoMember(4, IsRequired = false)]
        public int? GroundSpeed { get; set; }
        [ProtoMember(5, IsRequired = false)]
        public int? GroundTrack { get; set; }
        [ProtoMember(6, IsRequired = false)]
        public bool? Ident { get; set; }
        [ProtoMember(7, IsRequired = false)]
        public bool? IsOnGround { get; set; }
        [ProtoMember(8, IsRequired = false)]
        public string Squawk { get; set; }
        [ProtoMember(9, IsRequired = false)]
        public GeoPoint Location { get; set; }
        [ProtoMember(10, IsRequired = false)]
        public string Callsign { get; set; }
        [ProtoMember(11, IsRequired = false)]
        public int? VerticalRate { get; set; }
        [ProtoMember(12, IsRequired = false)]
        public int? ModeSCode { get; set; }

        public override UpdateType UpdateType => UpdateType.Track;

        public TrackUpdate(Track track, DateTime timestamp)
        {
            Base = track;
            Altitude = track.Altitude.Clone();
            TimeStamp = timestamp;
        }
        public TrackUpdate(Track track)
        {
            Base = track;
            Altitude = track.Altitude.Clone();
        }
        public TrackUpdate() { }
        public TrackUpdate(TrackUpdate trackUpdate, Track track)
        {
            SetAllProperties(trackUpdate);
            Base = track;
            /*
            TimeStamp = trackUpdate.TimeStamp;
            Altitude = trackUpdate.Altitude;
            GroundTrack = trackUpdate.GroundTrack;
            GroundSpeed = trackUpdate.GroundSpeed;
            Ident = trackUpdate.Ident;
            IsOnGround = trackUpdate.IsOnGround;
            Squawk = trackUpdate.Squawk;
            Location = trackUpdate.Location;
            Callsign = trackUpdate.Callsign;
            VerticalRate = trackUpdate.VerticalRate;
            ModeSCode = trackUpdate.ModeSCode;
            */
        }

        public string SerializeToJson()
        {
            return SerializeToJson(this);
        }

        public static string SerializeToJson(TrackUpdate trackUpdate)
        {
            return JsonConvert.SerializeObject(trackUpdate, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static TrackUpdate DeserializeFromJson(string json, Track track)
        {
            var update = JsonConvert.DeserializeObject<TrackUpdate>(json);
            return new TrackUpdate(update, track);
        }

        public Update DeserializeFromJson(string json)
        {
            return DeserializeFromJson(json, Base as Track);
        }
        public new void RemoveUnchanged()
        {
            base.RemoveUnchanged();
            //var track = Base as Track;
            //    ModeSCode = track.ModeSCode;
        }
    }
}
