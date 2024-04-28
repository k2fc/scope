using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Receivers.Falcon
{
    internal class FalconUpdate
    {
        public DateTime Time { get; set; }
        public string Site { get; set; }
        public string ACID { get; set; }
        public string ReportedBeaconCode { get; set; }
        public Altitude Altitude { get; set; }
        public int? RequestedAltitude { get; set; }
        public int TrackID { get; set; }
        public double? Track { get; set; }
        public int? Speed { get; set; }
        public string Owner { get; set; }
        public string FlightRules { get; set; }
        public string Category { get; set; }
        public string Scratchpad1 { get; set; }
        public string Scratchpad2 { get; set; }
        public string Type { get; set; }
        public string PendingHandoff { get; set; }
        public GeoPoint Location { get; set; }
        public int? ModeSAddress { get; set; }
        public string BcastFLID { get; set; }
        public string RawLine { get; set; }
        public string Destination { get; set; }
    }
}
