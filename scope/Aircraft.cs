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
        private double lastlat, lastlon;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Callsign { get; set; }
        public int PressureAltitude => Altitude.PressureAltitude;
        public int TrueAltitude => Altitude.TrueAltitude;
        private string positionind;
        public string PositionInd 
        {
            get => positionind;
            set
            {
                if (value != positionind && pendinghandoff != null && pendinghandoff != value)
                {
                    HandedOff?.Invoke(this, new HandoffEventArgs(this, value, pendinghandoff));
                    pendinghandoff = null;
                }
                positionind = value;
            }
        }
        private string pendinghandoff;
        public string PendingHandoff {
            get => pendinghandoff;
            set
            {
                if (value != pendinghandoff)
                {
                    HandoffInitiated?.Invoke(this, new HandoffEventArgs(this, value, PositionInd));
                    pendinghandoff = value;
                }
            }
        }
        public TPA TPA { get; set; }
        public Altitude Altitude
        {
            get; set;
        } = new Altitude();
        public GeoPoint Location
        {
            get
            {
                if (lastlat != Latitude || lastlon != Longitude)
                {
                    LocationUpdated?.Invoke(this, new UpdatePositionEventArgs(this, new GeoPoint(Latitude, Longitude)));
                    lastlat = Latitude;
                    lastlon = Longitude;
                }
                return new GeoPoint(Latitude, Longitude);
            }
            set
            {
                if (value.Latitude != Latitude || value.Longitude != Longitude)
                {
                    LocationUpdated?.Invoke(this, new UpdatePositionEventArgs(this, new GeoPoint(value.Latitude, value.Longitude)));
                }
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
        public int RequestedAltitude { get; set; } = 0;
        public string? Category { get; set; }
        public string FlightPlanCallsign { get; set; }
        public DateTime LastHistoryDrawn { get; set; } = DateTime.MinValue;
        public bool Drawn { get; set; } = false;
        bool owned = false;
        public bool Owned 
        {
            get => owned;
            set
            {
                if (value != owned)
                    OwnershipChange?.Invoke(this, new AircraftEventArgs(this));
                owned = value;
            } 
        }
        public bool Marked { get; set; } = false;
        public bool QuickLook { get; set; } = false;

        public RadarWindow.LeaderDirection? LDRDirection = null;
        public bool ShowCallsignWithNoSquawk { get; set; } = false;
        public bool FDB { 
            get
            {
                if (Owned) _fdb = true;
                return _fdb;
            }
            set
            {
                var oldvalue = _fdb;
                _fdb = value;
                if (oldvalue != value)
                    RedrawDataBlock(false);
                if (QuickLook)
                    QuickLook = false;
            }
        }

        bool ident;
        bool _fdb = false;
        private bool fdb()
        {
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
            Created?.Invoke(this, new EventArgs());
        }
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
            AutoSize = true
        };
        public TransparentLabel DataBlock2 = new TransparentLabel()
        {
            //ForeColor = Color.Lime,
            //BackColor = Color.Transparent,
            AutoSize = true
        };
        public TransparentLabel DataBlock3 = new TransparentLabel()
        {
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

        public void RedrawDataBlock(bool updatepos = false, RadarWindow.LeaderDirection? leaderDirection = null)
        {
            if (leaderDirection == null)
                leaderDirection = LDRDirection;
            string oldtext = DataBlock.Text;
            string oldtext2 = DataBlock2.Text;
            string oldtext3 = DataBlock3.Text;
            DataBlock.Text = "";
            DataBlock2.Text = "";
            DataBlock3.Text = "";
            if (updatepos || dbAlt == 0 || dbSpeed == 0)
            {
                dbAlt = TrueAltitude;
                dbSpeed = GroundSpeed;
            }
            string vfrchar = " ";
            string catchar = " ";
            string handoffchar = " ";
            if (PendingHandoff != null)
                handoffchar = PendingHandoff.Substring(PendingHandoff.Length - 1);

            if (Squawk == "1200" || (FlightRules != "IFR" && FlightRules != null))
            {
                vfrchar = "V";
            }

            if (Category != null)
            {
                catchar = Category;
            }
            string destination = "   ";
            string type = "    ";
            string yscratch = "   ";
            string hscratch = "    ";

            if (Destination == null)
            {
                destination = (dbAlt / 100).ToString("D3");
            }
            else if (Destination.Trim() != "" && Destination != "unassigned")
            {
                destination = Destination.PadRight(3);
            }
            else
            {
                destination = (dbAlt / 100).ToString("D3");
            }
            
            if (Scratchpad == null)
            {
                yscratch = destination;
            }
            else if (Scratchpad.Trim() != "")
            {
                yscratch = Scratchpad.PadRight(3);
            }
            else
            {
                yscratch = destination;
            }

            if (Type == null)
            {
                type = (dbSpeed / 10).ToString("D2") + vfrchar + catchar;
            }
            else if (Type.Trim() != "")
            {
                type = Type.PadRight(4);
            }
            else
            {
                type = (dbSpeed / 10).ToString("D2") + vfrchar + catchar;
            }

            if (Scratchpad2 == null && RequestedAltitude == 0)
            {
                hscratch = type;
            }
            else if (Scratchpad2 != null)
            {
                hscratch = Scratchpad2.PadRight(4);
            }
            else if (RequestedAltitude > 0)
            {
                hscratch = "R" + (RequestedAltitude / 100).ToString("D3");
            }
            else
            {
                hscratch = type;
            }

            string fdb1line2 = (dbAlt / 100).ToString("D3") + handoffchar + (dbSpeed / 10).ToString("D2") + vfrchar + catchar + " ";
            string fdb2line2 = destination + handoffchar + type + " ";
            string fdb3line2 = yscratch + handoffchar + hscratch + " ";


            if (FDB || ShowCallsignWithNoSquawk)
            {
                if (Callsign == null)
                {
                    DataBlock.Text = "         ";
                    DataBlock2.Text = "         ";
                    DataBlock3.Text = "         ";
                }
                else if ((Callsign.Trim() != "" && Squawk != "1200" && Squawk != null && Squawk != "0000") || ShowCallsignWithNoSquawk)
                {
                    if (leaderDirection == RadarWindow.LeaderDirection.W ||
                leaderDirection == RadarWindow.LeaderDirection.NW ||
                leaderDirection == RadarWindow.LeaderDirection.SW)
                    {
                        DataBlock.Text = Callsign.PadLeft(9);
                        DataBlock2.Text = Callsign.PadLeft(9); 
                        DataBlock3.Text = Callsign.PadLeft(9); 
                    }
                    else
                    {
                        DataBlock.Text = Callsign.PadRight(9);
                        DataBlock2.Text = Callsign.PadRight(9);
                        DataBlock3.Text = Callsign.PadRight(9);
                    }
                }
                else if (Squawk == "1200" && (PositionInd == null|| PositionInd == "*"))
                {
                    if (leaderDirection == RadarWindow.LeaderDirection.W ||
                leaderDirection == RadarWindow.LeaderDirection.NW ||
                leaderDirection == RadarWindow.LeaderDirection.SW)
                    {
                        DataBlock.Text = "     1200";
                        DataBlock2.Text = "     1200";
                        DataBlock3.Text = "     1200";
                    }
                    else
                    {
                        DataBlock.Text = "1200     ";
                        DataBlock2.Text = "1200     ";
                        DataBlock3.Text = "1200     ";
                    }
                }
                else if (Squawk == "1200" && FlightPlanCallsign != null)
                {
                    if (leaderDirection == RadarWindow.LeaderDirection.W ||
                leaderDirection == RadarWindow.LeaderDirection.NW ||
                leaderDirection == RadarWindow.LeaderDirection.SW)
                    {
                        DataBlock.Text = FlightPlanCallsign.PadLeft(9);
                        DataBlock2.Text = FlightPlanCallsign.PadLeft(9);
                        DataBlock3.Text = FlightPlanCallsign.PadLeft(9);
                    }
                    else
                    {
                        DataBlock.Text = FlightPlanCallsign.PadRight(9);
                        DataBlock2.Text = FlightPlanCallsign.PadRight(9);
                        DataBlock3.Text = FlightPlanCallsign.PadRight(9);
                    }
                }
                
                DataBlock.Text += "\r\n";
                DataBlock2.Text += "\r\n";
                DataBlock3.Text += "\r\n";

                if (FDB)
                {
                    DataBlock.Text += fdb1line2;
                    DataBlock2.Text += fdb2line2;
                    DataBlock3.Text += fdb3line2;
                }
                else
                {
                    DataBlock.Text += (dbAlt / 100).ToString("D3") + vfrchar + catchar + "\r\n     ";
                    DataBlock2.Text += destination.PadRight(5) + "\r\n     ";
                    DataBlock3.Text += yscratch.PadRight(5) + "\r\n     ";
                }
            }
            else
            {
                //This is an LDB
                DataBlock.Text = (dbAlt / 100).ToString("D3") + handoffchar + vfrchar + catchar + "\r\n     ";
                DataBlock2.Text = yscratch.PadRight(3) + handoffchar + vfrchar + catchar + "\r\n     ";
                DataBlock3.Text = yscratch.PadRight(3) + handoffchar + vfrchar + catchar + "\r\n     ";
            }

            if (!DataBlock.Redraw)
                DataBlock.Redraw = DataBlock2.Text != oldtext;
            if (!DataBlock2.Redraw)
                DataBlock2.Redraw = DataBlock.Text != oldtext2;
            if (!DataBlock3.Redraw)
                DataBlock3.Redraw = DataBlock.Text != oldtext3;
            if (PositionInd != null)
                PositionIndicator.Text = PositionInd.Substring(PositionInd.Length - 1);
            else
                PositionIndicator.Text = "*";
        }
        public void OldRedrawDataBlock(bool updatepos = false)
        {
            if (updatepos || dbAlt == 0 || dbSpeed == 0)
            {
                dbAlt = TrueAltitude;
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
            if (Callsign != null && fdb() && ((Squawk != "1200" && Squawk != null) || ShowCallsignWithNoSquawk || !(PositionInd == null || PositionInd == "*")))
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
        public void DropTrack()
        {
            Owned = false;
            PositionInd = "*";
            PendingHandoff = null;
            Scratchpad = null;
            Scratchpad2 = null;
            Runway = null;
            Destination = null;
            Dropped?.Invoke(this, new EventArgs());
        }
        public void RedrawTarget(PointF LocationF)
        {
            this.LocationF = LocationF;
            if (LocationF.X != 0 || LocationF.Y != 0)
            {
                TargetReturn.Angle = Location.BearingTo(LocationReceivedBy.Location);
                TargetReturn.LocationF = LocationF;
                PositionIndicator.CenterOnPoint(LocationF);
                RedrawDataBlock(true);
                TargetReturn.Refresh();
                SweptLocation = Location;
                LocationUpdated?.Invoke(this, new UpdatePositionEventArgs(this, Location));
            }
        }

        public GeoPoint SweptLocation;

        public override string ToString()
        {
            return Callsign;
        }

        public event EventHandler<UpdatePositionEventArgs> LocationUpdated;
        public event EventHandler<HandoffEventArgs> HandoffInitiated;
        public event EventHandler<HandoffEventArgs> HandedOff;
        public event EventHandler Created;
        public event EventHandler<AircraftEventArgs> OwnershipChange;
        //public event EventHandler Tracked;
        public event EventHandler Dropped;
        //public event EventHandler Idented;

    }

    public class HandoffEventArgs : AircraftEventArgs
    {
        public string? PositionFrom { get; private set; }
        public string PositionTo { get; private set; }
        public HandoffEventArgs(Aircraft Aircraft, string to, string from = null) :base(Aircraft)
        {
            PositionFrom = from;
            PositionTo = to;
        }
    }
    public class UpdatePositionEventArgs : AircraftEventArgs
    {
        public GeoPoint Location { get; private set; }
        //public bool Intrafacility { get; private set; }
        public UpdatePositionEventArgs(Aircraft Aircraft, GeoPoint Location, bool intrafacility = false) : base(Aircraft)
        {
            this.Location = Location;
            //Intrafacility = intrafacility;
        }
    }

    
    public class AircraftEventArgs : EventArgs
    {
        public Aircraft Aircraft { get; set; }
        public DateTime Time { get; private set; }
        public AircraftEventArgs(Aircraft Aircraft)
        {
            this.Aircraft = Aircraft;
            Time = DateTime.Now;
        }
    }
}
