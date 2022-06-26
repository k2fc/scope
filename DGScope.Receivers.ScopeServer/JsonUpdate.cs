using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers.ScopeServer
{
    public class JsonUpdateRoot
    {
        public JsonUpdate Value { get; set; }
    }
    public class JsonUpdate
    {
        public Altitude Altitude { get; set; }
        public int? GroundSpeed { get; set; }
        public int? GroundTrack { get; set; }
        public bool? Ident { get; set; }
        public bool? IsOnGround { get; set; }
        public string Squawk { get; set; }
        public Location Location { get; set; }
        public int? VerticalRate { get; set; }
        public Guid Guid { get; set; }
        public int? UpdateType { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Callsign { get; set; }
        public string AircraftType { get; set; }
        public string WakeCategory { get; set; }
        public string FlightRules { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string EntryFix { get; set; }
        public int? RequestedAltitude { get; set; }
        public int? ModeSCode { get; set; }
        public string Scratchpad1 { get; set; }
        public string Scratchpad2 { get; set; }
        public string Owner { get; set; }
        public string PendingHandoff { get; set; }
        public string EquipmentSuffix { get; set; }
        public int? LDRDirection { get; set; }
        public Guid? AssociatedTrackGuid { get; set; }
        public string FacilityID { get; set; }
        public string AssignedSquawk { get; set; }
        public string ExitFix { get; set; }
    }
    public static class LDRDirection
    {
        public static RadarWindow.LeaderDirection? ParseLDR(int? LDRDirection)
        {
            switch (LDRDirection)
            {
                case 1:
                    return RadarWindow.LeaderDirection.NW;
                case 2:
                    return RadarWindow.LeaderDirection.N;
                case 3:
                    return RadarWindow.LeaderDirection.NE;
                case 6:
                    return RadarWindow.LeaderDirection.E;
                case 9:
                    return RadarWindow.LeaderDirection.SE;
                case 8:
                    return RadarWindow.LeaderDirection.S;
                case 7:
                    return RadarWindow.LeaderDirection.SW;
                case 4:
                    return RadarWindow.LeaderDirection.W;
                case 5:
                    return null;
                default:
                    throw new InvalidCastException();
            }
        }
    }
    public class Altitude
    {
        public int Value { get; set; }
        public int AltitudeType { get; set; }
    }

    public class Location
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}




