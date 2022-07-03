using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public class FlightPlan :IUpdatable
    {
        public string Callsign { get; private set; }
        public string AircraftType { get; private set; }
        public string WakeCategory { get; private set; }
        public string FlightRules { get; private set; }
        public string Origin { get; private set; }
        public string Destination { get; private set; }
        public string EntryFix { get; private set; }
        public string ExitFix { get; private set; }
        public string Route { get; private set; }
        public int RequestedAltitude { get; private set; }
        public string Scratchpad1 { get; private set; }
        public string Scratchpad2 { get; private set; }
        public string Runway { get; private set; }
        public string Owner { get; private set; }
        public string PendingHandoff { get; private set; }
        public string AssignedSquawk { get; private set; }
        public string EquipmentSuffix { get; private set; }

        public Guid Guid { get; set; } = Guid.NewGuid();
        public DateTime LastMessageTime { get; private set; } = DateTime.MinValue;
        public LDRDirection? LDRDirection { get; private set; }
        public Track AssociatedTrack { get; private set; }
        public Dictionary<PropertyInfo, DateTime> PropertyUpdatedTimes { get; } = new Dictionary<PropertyInfo, DateTime>();
        public FlightPlan(string Callsign) 
        {
            this.Callsign = Callsign;
            Created?.Invoke(this, new FlightPlanUpdatedEventArgs(GetCompleteUpdate()));
        }
        public FlightPlan(Guid guid)
        {
            this.Guid = guid;
            Created?.Invoke(this, new FlightPlanUpdatedEventArgs(GetCompleteUpdate()));
        }

        public void AssociateTrack(Track track)
        {
            AssociatedTrack = track;
        }

        public void UpdateFlightPlan(FlightPlanUpdate update)
        {
            update.RemoveUnchanged();
            

            bool changed = false;
            foreach (var updateProperty in update.GetType().GetProperties())
            {
                PropertyInfo thisProperty = GetType().GetProperty(updateProperty.Name);
                object updateValue = updateProperty.GetValue(update);
                if (updateValue == null || thisProperty == null)
                    continue;
                object thisValue = thisProperty.GetValue(this);
                lock (PropertyUpdatedTimes)
                {
                    if (!PropertyUpdatedTimes.TryGetValue(thisProperty, out DateTime lastUpdatedTime))
                        PropertyUpdatedTimes.Add(thisProperty, update.TimeStamp);
                    if (update.TimeStamp > lastUpdatedTime && thisProperty.CanWrite && !Equals(thisValue, updateValue))
                    {
                        thisProperty.SetValue(this, updateValue);
                        PropertyUpdatedTimes[thisProperty] = update.TimeStamp;
                        changed = true;
                    }
                }
            }
            if (changed)
            {
                if (update.TimeStamp > LastMessageTime)
                    LastMessageTime = update.TimeStamp;
                Updated?.Invoke(this, new FlightPlanUpdatedEventArgs(update));
            }
            return;
            update.RemoveUnchanged();
            if (update.TimeStamp < LastMessageTime)
                return;
            LastMessageTime = update.TimeStamp;
            if (update.Callsign != null)
            {
                changed = true;
                Callsign = update.Callsign;
            }
            if (update.AircraftType != null)
            {
                changed = true;
                AircraftType = update.AircraftType;
            }
            if (update.WakeCategory != null)
            {
                changed = true;
                WakeCategory = update.WakeCategory;
            }
            if (update.FlightRules != null)
            {
                changed = true;
                FlightRules = update.FlightRules;
            }
            if (update.Origin != null)
            {
                changed = true;
                Origin = update.Origin;
            }
            if (update.Destination != null)
            {
                changed = true;
                Destination = update.Destination;
            }
            if (update.EntryFix != null)
            {
                changed = true;
                EntryFix = update.EntryFix;
            }
            if (update.ExitFix != null)
            {
                changed = true;
                EntryFix = update.ExitFix;
            }
            if (update.Route != null)
            {
                changed = true;
                Route = update.Route;
            }
            if (update.RequestedAltitude != null)
            {
                changed = true;
                RequestedAltitude = (int)update.RequestedAltitude;
            }
            if (update.Scratchpad1 != null)
            {
                changed = true;
                Scratchpad1 = update.Scratchpad1;
            }
            if (update.Scratchpad2 != null)
            {
                changed = true;
                Scratchpad2 = update.Scratchpad2;
            }
            if (update.Runway != null)
            {
                changed = true;
                Runway = update.Runway;
            }
            if (update.Owner != null)
            {
                changed = true;
                Owner = update.Owner;
            }
            if (update.PendingHandoff != null)
            {
                changed = true;
                PendingHandoff = update.PendingHandoff;
            }
            if (update.AssignedSquawk != null)
            {
                changed = true;
                AssignedSquawk = update.AssignedSquawk;
            }
            if (update.LDRDirection != null)
            {
                changed = true;
                LDRDirection = update.LDRDirection;
            }
            if (update.EquipmentSuffix != null)
            {
                changed = true;
                EquipmentSuffix = update.EquipmentSuffix;
            }
            AssociatedTrack = update.AssociatedTrack;
            if (changed)
                Updated?.Invoke(this, new FlightPlanUpdatedEventArgs(update));
        }
        public Update GetCompleteUpdate()
        {
            var newUpdate = new FlightPlanUpdate(this);
            newUpdate.SetAllProperties();
            return newUpdate;
        }
        public override string ToString()
        {
            return Callsign;
        }
        public event EventHandler<UpdateEventArgs> Updated;
        public event EventHandler<UpdateEventArgs> Created;
    }

    public class FlightPlanUpdatedEventArgs : UpdateEventArgs
    {
        public FlightPlan FlightPlan { get; private set; }
        public FlightPlanUpdatedEventArgs(FlightPlan flightPlan)
        {
            FlightPlan = flightPlan;
            Update = flightPlan.GetCompleteUpdate();
        }
        public FlightPlanUpdatedEventArgs(Update update)
        {
            FlightPlan = update.Base as FlightPlan;
            Update = update;
        }
    }
}
