﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class ATPAVolume
    {
        public string VolumeId { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public GeoPoint RunwayThreshold { get; set; } = new GeoPoint();
        public int TrueHeading { get; set; }
        public int MaxHeadingDeviation { get; set; }
        public int Ceiling { get; set; }
        public int Floor { get; set; }
        public double Length { get; set; }
        public int WidthLeft { get; set; }
        public int WidthRight { get; set; }
        public bool TwoPointFiveEnabled { get; set; }
        public bool TwoPointFiveActive { get; set; }
        //public List<ScratchpadFilter> ScratchpadFilters { get; set; } = new List<ScratchpadFilter>();
        public List<int> LeaderFilters { get; set; } = new List<int>();
        public string Destination { get; set; }
        public double TwoPointFiveDistance { get; set; }


        private List<Aircraft> order;
        private object orderlock = new object();

        private int minHeading => TrueHeading - MaxHeadingDeviation;
        private int maxHeading => TrueHeading + MaxHeadingDeviation;
        private int minBearing => TrueHeading - 90;
        private int maxBearing => TrueHeading + 90;
        
        public bool IsInside (Aircraft aircraft, Radar radar)
        {
            if (aircraft == null)
                return false;
            if (aircraft.TrueAltitude > Ceiling || aircraft.TrueAltitude < Floor)
                return false;
            if (Destination != null && Destination != aircraft.Destination)
                return false;
            if (LeaderFilters.Count > 0 && !LeaderFilters.Contains((int)aircraft.LDRDirection))
                return false;
            var acloc = aircraft.SweptLocation(radar);
            if (acloc == null)
                return false;
            var acdistancetothreshold = acloc.DistanceTo(RunwayThreshold);
            if (acdistancetothreshold > Length) 
                return false;
            var acbearingtothreshold = (acloc.BearingTo(RunwayThreshold) + 360) % 360;
            if (!BearingIsBetween(acbearingtothreshold, minBearing, maxBearing))
                return false;
            if (!BearingIsBetween(aircraft.SweptTrack(radar), minHeading, maxHeading))
                return false;
            var angletothreshold = acbearingtothreshold - TrueHeading;
            var disttocenterline = acdistancetothreshold * Math.Sin(Math.Abs(angletothreshold) * (Math.PI / 180));
            if (angletothreshold > 0 && disttocenterline * 6076 > WidthLeft) // left
                return false;
            if (angletothreshold < 0 && disttocenterline * 6076 > WidthRight) // right
                return false;
            return true;
        }
        private bool BearingIsBetween(double bearing, double az1, double az2)
        {
            if (az2 == az1)
            {
                return bearing == az1;
            }
            if (az2 > az1)
            {
                return bearing >= az1 && bearing <= az2;
            }
            else
            {
                return (bearing >= az1 && bearing <= 360) || bearing <= az2;
            }
        }

        public void ResetAircraftATPAValues(Aircraft aircraft, bool resetAcATPARef = true)
        {
            if (resetAcATPARef)
                aircraft.ATPAVolume = null;
            aircraft.ATPAFollowing = null;
            aircraft.ATPAMileageNow = null;
            aircraft.ATPAMileage24 = null;
            aircraft.ATPAMileage45 = null;
            aircraft.ATPARequiredMileage = null;
            aircraft.ATPAStatus = null;
            aircraft.ATPACone = null;
        }

        public void CalculateATPA(List<Aircraft> aircraft, SeparationTable separationtable, Radar radar)
        {
            lock (orderlock) 
            {
                if (!Active)
                {
                    aircraft.Where(x => x.ATPAVolume == this).ToList().ForEach(x => ResetAircraftATPAValues(x));
                    return;
                }
                order = (aircraft.ToList().Where(x => IsInside(x, radar)).OrderBy(x => x.SweptLocation(radar).DistanceTo(RunwayThreshold))).ToList();
                aircraft.ToList().Where(x => x.ATPAVolume == this && !order.Contains(x)).ToList().ForEach(x =>
                {
                    ResetAircraftATPAValues(x);
                });
                if (order.Count > 1)
                {
                    order[0].ATPAVolume = this;
                    ResetAircraftATPAValues(order[0], false);
                    for (int i = 1; i < order.Count; i++)
                    {
                        var leader = order[i - 1];
                        var follower = order[i];
                        follower.ATPAVolume = this;
                        follower.ATPAFollowing = leader;
                        follower.ATPAMileageNow = follower.SweptLocation(radar).DistanceTo(leader.SweptLocation(radar));
                        follower.ATPAMileage24 = follower.SweptLocation(radar).FromPoint(follower.GroundSpeed * 24 / 3600d, follower.ExtrapolateTrack())
                            .DistanceTo(leader.SweptLocation(radar).FromPoint(leader.GroundSpeed * 24 / 3600d, leader.ExtrapolateTrack()));
                        follower.ATPAMileage45 = follower.SweptLocation(radar).FromPoint(follower.GroundSpeed * 45 / 3600d, follower.ExtrapolateTrack())
                            .DistanceTo(leader.SweptLocation(radar).FromPoint(leader.GroundSpeed * 45 / 3600d, leader.ExtrapolateTrack()));
                        double minsep = 3;
                        if (TwoPointFiveEnabled && TwoPointFiveActive && follower.SweptLocation(radar).DistanceTo(RunwayThreshold) <= TwoPointFiveDistance)
                            minsep = 2.5;
                        if (follower.Category != null && separationtable.TryGetValue(follower.Category, out SerializableDictionary<string, double> leaderTable))
                        {
                            if (leader.Category != null && leaderTable != null && leaderTable.TryGetValue(leader.Category, out double miles))
                                follower.ATPARequiredMileage = miles;
                            else
                                follower.ATPARequiredMileage = minsep;
                        }
                        else
                        {
                            follower.ATPARequiredMileage = minsep;
                        }
                        if (follower.ATPAMileageNow < follower.ATPARequiredMileage || follower.ATPAMileage24 < follower.ATPARequiredMileage)
                            follower.ATPAStatus = ATPAStatus.Alert;
                        else if (follower.ATPAMileage45 < follower.ATPARequiredMileage)
                            follower.ATPAStatus = ATPAStatus.Caution;
                        else
                            follower.ATPAStatus = ATPAStatus.Monitor;
                        follower.ATPATrackToLeader = follower.SweptLocation(radar).BearingTo(leader.SweptLocation(radar));
                    }
                }
                else if (order.Count == 1)
                {
                    order[0].ATPAVolume = this;
                    ResetAircraftATPAValues(order[0], false);
                }
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }

    
}
