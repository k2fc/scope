using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class ATPAVolume : Polygon
    {
        public GeoPoint MergePoint { get; set; }
        public double MinimumSeparation { get; set; } = 3;
        public string Name { get; set; }
        public bool Active { get; set; }
        public int MaxAltitude { get; set; }
        public int MinAltitude { get; set; }
        public string? Runway { get; set; }
        public string? Destination { get; set; }
        public int? LDRDirection { get; set; }

        private List<Aircraft> order;
        private object orderlock = new object();
        public bool IsInside (Aircraft aircraft)
        {
            if (aircraft == null)
                return false;
            if (aircraft.SweptLocation == null)
                return false;
            if (!aircraft.SweptLocation.IsInsidePolygon(this))
                return false;
            if (aircraft.TrueAltitude > MaxAltitude || aircraft.TrueAltitude < MinAltitude)
                return false;
            //if (Runway != null && Runway != aircraft.Runway)
            //    return false;
            if (Destination != null && Destination != aircraft.Destination)
                return false;
            if (LDRDirection != null && LDRDirection != (int?)aircraft.LDRDirection)
                return false;
            double currentdistance = aircraft.SweptLocation.DistanceTo(MergePoint);
            double testdistance = aircraft.SweptLocation.FromPoint(aircraft.GroundSpeed / 3600d, aircraft.ExtrapolateTrack()).DistanceTo(MergePoint);
            if (testdistance >= currentdistance) // Plane is moving away from merge point
                return false;
            return true;
        }

        public void CalculateATPA(List<Aircraft> aircraft, SeparationTable separationtable)
        {
            lock (orderlock) 
            {
                order = (aircraft.ToList().Where(x => IsInside(x)).OrderBy(x => x.SweptLocation.DistanceTo(MergePoint))).ToList();
                if (order.Count > 1)
                {
                    order[0].ATPAFollowing = null;
                    order[0].ATPAMileageNow = null;
                    order[0].ATPAMileage24 = null;
                    order[0].ATPAMileage45 = null;
                    order[0].ATPARequiredMileage = null;
                    order[0].ATPAStatus = null;
                    order[0].ATPACone = null;
                    for (int i = 1; i < order.Count; i++)
                    {
                        var leader = order[i - 1];
                        var follower = order[i];
                        follower.ATPAFollowing = leader;
                        follower.ATPAMileageNow = follower.SweptLocation.DistanceTo(leader.SweptLocation);
                        follower.ATPAMileage24 = follower.SweptLocation.FromPoint(follower.GroundSpeed * 24 / 3600d, follower.ExtrapolateTrack())
                            .DistanceTo(leader.SweptLocation.FromPoint(leader.GroundSpeed * 24 / 3600d, leader.ExtrapolateTrack()));
                        follower.ATPAMileage45 = follower.SweptLocation.FromPoint(follower.GroundSpeed * 45 / 3600d, follower.ExtrapolateTrack())
                            .DistanceTo(leader.SweptLocation.FromPoint(leader.GroundSpeed * 45 / 3600d, leader.ExtrapolateTrack()));
                        if (separationtable.TryGetValue(follower.Category, out SerializableDictionary<string, double> leaderTable))
                        {
                            if (leaderTable.TryGetValue(leader.Category, out double miles))
                                follower.ATPARequiredMileage = miles;
                            else
                                follower.ATPARequiredMileage = MinimumSeparation;
                        }
                        else
                        {
                            follower.ATPARequiredMileage = MinimumSeparation;
                        }
                        if (follower.ATPAMileageNow < follower.ATPARequiredMileage || follower.ATPAMileage24 < follower.ATPARequiredMileage)
                            follower.ATPAStatus = ATPAStatus.Alert;
                        else if (follower.ATPAMileage45 < follower.ATPARequiredMileage)
                            follower.ATPAStatus = ATPAStatus.Caution;
                        else
                            follower.ATPAStatus = ATPAStatus.Monitor;
                        follower.ATPATrackToLeader = follower.SweptLocation.BearingTo(leader.SweptLocation);
                    }
                }
                else if (order.Count == 1)
                {
                    order[0].ATPAFollowing = null;
                    order[0].ATPAMileageNow = null;
                    order[0].ATPAMileage24 = null;
                    order[0].ATPAMileage45 = null;
                    order[0].ATPARequiredMileage = null;
                    order[0].ATPAStatus = null;
                }
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }

    
}
