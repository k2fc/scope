using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class FlightPlanUpdate : Update
    {
        public string Callsign { get; set; }
        public string AircraftType { get; set; }
        public string WakeCategory { get; set; }
        public string FlightRules { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string EntryFix { get; set; }
        public string ExitFix { get; set; }
        public string Route { get; set; }
        public int? RequestedAltitude { get; set; }
        public string Scratchpad1 { get; set; }
        public string Scratchpad2 { get; set; }
        public string Runway { get; set; }
        public string Owner { get; set; }
        public string PendingHandoff { get; set; }
        public string AssignedSquawk { get; set; }
        public string EquipmentSuffix { get; set; }
        public LDRDirection? LDRDirection { get; set; }
        [JsonIgnore]
        public Track AssociatedTrack { get; set; }
        private Guid? associatedTrackGuid;
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
