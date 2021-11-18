using System;
using System.Collections.Generic;
using System.Drawing;
using DGScope.Receivers;

namespace DGScope
{
    public class Aircraft : IDisposable
    {
        public int ModeSCode { get; set; }
        public string Squawk { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Callsign { get; set; }
        public int PressureAltitude { get; set; }
        public int TrueAltitude { get; set; }
        public string PositionInd { get; set; }
        public string PendingHandoff { get; set; }
        public int Altitude
        {
            get
            {
                if (TrueAltitude <= 18000)
                    return TrueAltitude;
                return PressureAltitude;
            }
        }
        public GeoPoint Location
        {
            get
            {
                return new GeoPoint(Latitude, Longitude);
            }
            set
            {
                Latitude = value.Latitude;
                Longitude = value.Longitude;
                Drawn = false;
            }
        }
        public PointF LocationF { get; set; }
        public Receiver LocationReceivedBy { get; set; }
        public int GroundSpeed { get; set; }
        public int Track { get; set; }
        public int VerticalRate { get; set; }
        public bool Ident { get => ident;
            set
            {
                ident = value;
                //DataBlock.Flashing = value;
                //DataBlock2.Flashing = value;
            }
        }
        public bool IsOnGround { get; set; }
        public bool Emergency { get; set; }
        public bool Alert { get; set; }
        public DateTime LastMessageTime { get; set; }
        public DateTime LastPositionTime { get; set; }
        public Color TargetColor { get { return TargetReturn.ForeColor; } set { TargetReturn.ForeColor = value; } }
        public Font Font { get { return DataBlock.Font; } set { DataBlock.Font = value; } }
        public Line PTL { get; set; } = new Line();
        public string? Destination { get; set; }
        public string? Scratchpad { get; set; }
        public string? Type { get; set; }
        public string? Scratchpad2 { get; set; }
        public string? FlightRules { get; set; }
        public string? Runway { get; set; }
        public string? Category { get; set; }
        public bool Drawn { get; set; } = false;
        public bool Owned { get; set; } = false;
        public bool Marked { get; set; } = false;
        public bool QuickLook { get; set; } = false;

        public RadarWindow.LeaderDirection? LDRDirection = null;
        public bool ShowCallsignWithNoSquawk { get; set; } = false;
        public bool FDB { 
            get
            {
                return _fdb;
            }
            set
            {
                DataBlock.Redraw = true;
                if (QuickLook)
                    QuickLook = false;
                _fdb = value;
            }
        }

        bool ident;
        bool _fdb = false;
        private bool fdb()
        {
            if (Owned)
            {
                _fdb = true;
                return true;
            }
            if (Emergency || ShowCallsignWithNoSquawk || QuickLook)
            {
                return true;
            }
            else
            {
                return _fdb;
            }
        }
        public Aircraft(int icaoID)
        {
            ModeSCode = icaoID;
        }
        public Aircraft() { }
        public double Bearing(GeoPoint FromPoint)
        {
            double λ2 = Longitude * (Math.PI / 180);
            double λ1 = FromPoint.Longitude * (Math.PI / 180);
            double φ2 = Latitude * (Math.PI / 180);
            double φ1 = FromPoint.Latitude * (Math.PI / 180);

            double y = Math.Sin(λ2 - λ1) * Math.Cos(φ2);
            double x = Math.Cos(φ1) * Math.Sin(φ2) -
                      Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(λ2 - λ1);
            double θ = Math.Atan2(y, x);
            //θ = (Math.PI / 2) - θ;
            return (θ * 180 / Math.PI + 360) % 360; // in degrees

        }

        public double Distance(GeoPoint FromPoint)
        {
            double R = 3443.92; // nautical miles
            double φ1 = Latitude * Math.PI / 180; // φ, λ in radians
            double φ2 = FromPoint.Latitude * Math.PI / 180;
            double Δφ = (FromPoint.Latitude - Latitude) * Math.PI / 180;
            double Δλ = (FromPoint.Longitude - Longitude) * Math.PI / 180;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                      Math.Cos(φ1) * Math.Cos(φ2) *
                      Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double alt = TrueAltitude / 6076.12;


            double dist = Math.Sqrt((R * c) * (R * c) + (alt * alt)); // in nautical miles
            return dist;
        }

        public PrimaryReturn TargetReturn = new PrimaryReturn() { BackColor = Color.Transparent, ForeColor = Color.Lime };
        public List<PrimaryReturn> ReturnTrails = new List<PrimaryReturn>();
        public ConnectingLineF ConnectingLine = new ConnectingLineF() { };

        public TransparentLabel DataBlock = new TransparentLabel()
        {
            //ForeColor = Color.Lime,
            //BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
            AutoSize = true
        };
        public TransparentLabel DataBlock2 = new TransparentLabel()
        {
            //ForeColor = Color.Lime,
            //BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
            AutoSize = true
        };

        public TransparentLabel PositionIndicator = new TransparentLabel()
        {
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "*",
            AutoSize = true
        };

        public void Dispose()
        {
            DataBlock.Dispose();
            TargetReturn.Dispose();
        }

        private int dbAlt, dbSpeed = 0;
        public void RedrawDataBlock(bool updatepos = false)
        {
            if (updatepos || dbAlt == 0 || dbSpeed == 0)
            {
                dbAlt = Altitude;
                dbSpeed = GroundSpeed;
            }
            string vrchar = " ";
            if (PendingHandoff != null)
                if (PendingHandoff != PositionInd)
                    vrchar = PendingHandoff.Substring(PendingHandoff.Length - 1);
            var oldtext = DataBlock.Text;
            var oldtext2 = DataBlock.Text;

            string field1 = "";
            if (Scratchpad != null)
            {
                field1 = Scratchpad;
            }
            else if (Runway != null)
            {
                if (Runway != "NNNN")
                    field1 = Runway;
            }
            if (field1.Trim() == "" && Destination != null)
            {
                if (Destination != "unassigned")
                    field1 = Destination;
            }

            if (field1.Trim() == "")
            {
                field1 = (dbAlt / 100).ToString("D3");
            }

            field1 = field1.Trim();
            string field2 = "";
            if (Type == null && Scratchpad2 == null)
                field2 = (dbSpeed / 10).ToString("D2");
            else if (Scratchpad2 != null)
                field2 = Scratchpad2;
            else
                field2 = Type;
            field2 = field2.Trim();



            DataBlock.Text = "";
            if (Squawk == "7700")
                DataBlock.Text += "EM" + "\r\n";
            else if (Squawk == "7600")
                DataBlock.Text += "RF" + "\r\n";
            if (Callsign != null && fdb() && ((Squawk != "1200" && Squawk != null) || ShowCallsignWithNoSquawk))
                DataBlock.Text += Callsign + "\r\n";
            else if (Squawk == "1200" && fdb())
                DataBlock.Text = "1200\r\n";
            DataBlock2.Text = DataBlock.Text;
            if (!fdb())
            {
                DataBlock.Text = (dbAlt / 100).ToString("D3");
                DataBlock2.Text = field1;
            }
            else
            {
                DataBlock.Text += (dbAlt / 100).ToString("D3") + vrchar + (dbSpeed / 10).ToString("D2");
                DataBlock2.Text += field1 + vrchar + field2;
            }
            if (Squawk == "1200" || (FlightRules != "IFR" && FlightRules != null))
            {
                if (DataBlock2.Text == DataBlock.Text)
                    DataBlock2.Text += "V ";
                DataBlock.Text += "V";
            }
            if (fdb())
                DataBlock.Text += " " + Category;
            if (Ident)
            {
                DataBlock2.Text += "ID";
                DataBlock.Text += "ID";
            }
            if (!DataBlock2.Redraw)
                DataBlock2.Redraw = DataBlock2.Text != oldtext2;
            if (!DataBlock.Redraw)
                DataBlock.Redraw = DataBlock.Text != oldtext;
            if (PositionInd != null)
                PositionIndicator.Text = PositionInd.Substring(PositionInd.Length - 1);
            else
                PositionIndicator.Text = "*";
        }
        
        public void RedrawTarget(PointF LocationF)
        {
            this.LocationF = LocationF;
            TargetReturn.Angle = Location.BearingTo(LocationReceivedBy.Location);
            TargetReturn.LocationF = LocationF;
            PositionIndicator.CenterOnPoint(LocationF);
            RedrawDataBlock(true);
            TargetReturn.Refresh();
        }



        public override string ToString()
        {
            return Callsign;
        }
    }

}
