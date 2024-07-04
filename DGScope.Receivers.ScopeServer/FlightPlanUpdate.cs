using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class FlightPlanUpdate : Update
    {
        [ProtoMember(3, IsRequired = false)]
        public string Callsign { get; set; }
        [ProtoMember(4, IsRequired = false)]
        public string AircraftType { get; set; }
        [ProtoMember(5, IsRequired = false)]
        public string WakeCategory { get; set; }
        [ProtoMember(6, IsRequired = false)]
        public string FlightRules { get; set; }
        [ProtoMember(7, IsRequired = false)]
        public string Origin { get; set; }
        [ProtoMember(8, IsRequired = false)]
        public string Destination { get; set; }
        [ProtoMember(9, IsRequired = false)]
        public string EntryFix { get; set; }
        [ProtoMember(10, IsRequired = false)]
        public string ExitFix { get; set; }
        [ProtoMember(11, IsRequired = false)]
        public string Route { get; set; }
        [ProtoMember(12, IsRequired = false)]
        public int? RequestedAltitude { get; set; }
        [ProtoMember(13, IsRequired = false)]
        public string Scratchpad1 { get; set; }
        [ProtoMember(14, IsRequired = false)]
        public string Scratchpad2 { get; set; }
        [ProtoMember(15, IsRequired = false)]
        public string Runway { get; set; }
        [ProtoMember(16, IsRequired = false)]
        public string Owner { get; set; }
        [ProtoMember(17, IsRequired = false)]
        public string PendingHandoff { get; set; }
        [ProtoMember(18, IsRequired = false)]
        public string AssignedSquawk { get; set; }
        [ProtoMember(19, IsRequired = false)]
        public string EquipmentSuffix { get; set; }
        [ProtoMember(20, IsRequired = false)]
        public LDRDirection? LDRDirection { get; set; }
        [JsonIgnore]
        public Track AssociatedTrack { get; set; }
        private Guid? associatedTrackGuid;
        [ProtoMember(21, IsRequired = false)]
        public Guid? AssociatedTrackGuid
        {
            get
            {
                if (AssociatedTrack != null)
                {
                    return AssociatedTrack.Guid;
                    associatedTrackGuid = AssociatedTrack.Guid;
                }
                return associatedTrackGuid;
            }
            set
            {
                associatedTrackGuid = value;
            }
        }

        public override UpdateType UpdateType => UpdateType.Flightplan;
        public FlightPlanUpdate(FlightPlan flightPlan, DateTime timestamp)
        {
            Base = flightPlan;
            TimeStamp = timestamp;
        }
        public FlightPlanUpdate(FlightPlan flightPlan)
        {
            Base = flightPlan;
            SetAllProperties();
        }
        public FlightPlanUpdate() { }
        public FlightPlanUpdate(FlightPlanUpdate update, FlightPlan flightPlan)
        {
            SetAllProperties(update);
            Base = flightPlan;
            /*
            TimeStamp = update.TimeStamp;
            Callsign = update.Callsign;
            AircraftType = update.AircraftType;
            WakeCategory = update.WakeCategory;
            FlightRules = update.FlightRules;
            Origin = update.Origin;
            Destination = update.Destination;
            EntryFix = update.EntryFix;
            ExitFix = update.ExitFix;
            Route = update.Route;
            RequestedAltitude = update.RequestedAltitude;
            Scratchpad1 = update.Scratchpad1;
            Scratchpad2 = update.Scratchpad2;
            Runway = update.Runway;
            Owner = update.Owner;
            PendingHandoff = update.PendingHandoff;
            AssignedSquawk = update.AssignedSquawk;
            LDRDirection = update.LDRDirection;
            EquipmentSuffix = update.EquipmentSuffix;
            AssociatedTrack = update.AssociatedTrack;
            */
        }

    }
}
