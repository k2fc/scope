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
        public int Altitude { get; set; }
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
            }
        }
        public PointF LocationF { get; set; }
        public IReceiver LocationReceivedBy { get; set; }
        public int GroundSpeed { get; set; }
        public int Track { get; set; }
        public int VerticalRate { get; set; }
        public bool Ident { get; set; }
        public bool IsOnGround { get; set; }
        public bool Emergency { get; set; }
        public bool Alert { get; set; }
        public DateTime LastMessageTime { get; set; }
        public DateTime LastPositionTime { get; set; }
        public char TargetChar { get; set; }
        public Color TargetColor { get { return TargetReturn.ForeColor; } set { TargetReturn.ForeColor = value; } }

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

            double alt = Altitude / 6076.12;


            double dist = Math.Sqrt((R * c) * (R * c) + (alt * alt)); // in nautical miles
            return dist;
        }

        public PrimaryReturn TargetReturn = new PrimaryReturn() { BackColor = Color.Transparent, ForeColor = Color.Lime };
        public List<PrimaryReturn> ReturnTrails = new List<PrimaryReturn>();
        public ConnectingLine ConnectingLine = new ConnectingLine() { BackColor = Color.Transparent, ForeColor = Color.Lime };

        public TransparentLabel DataBlock = new TransparentLabel()
        {
            ForeColor = Color.Lime,
            //BackColor = Color.Transparent,
            TextAlign = ContentAlignment.TopLeft,
            AutoSize = true,
            Font = new Font("Consolas", 10)
        };

        public void Dispose()
        {
            DataBlock.Dispose();
            TargetReturn.Dispose();
        }
        public void RedrawTarget(int top, int left)
        {

            TargetReturn.Top = top - TargetReturn.Height / 2;
            TargetReturn.Left = left - TargetReturn.Width / 2;
            DataBlock.Top = TargetReturn.Top - 50;
            DataBlock.Left = TargetReturn.Right + 50;
            string vrchar = " ";
            if (VerticalRate > 100)
                vrchar = "↑";
            else if (VerticalRate < -100)
                vrchar = "↓";
            var oldtext = DataBlock.Text;
            if (Callsign != null)
                DataBlock.Text = Callsign + "\r\n" + (Altitude / 100).ToString("D3") + vrchar + " " + (GroundSpeed / 10).ToString("D2");
            else
                DataBlock.Text = (Altitude / 100).ToString("D3") + vrchar + " " + (GroundSpeed / 10).ToString("D2");
            DataBlock.Redraw = DataBlock.Text != oldtext;
            TargetReturn.Text = TargetChar.ToString();
            TargetReturn.Refresh();
            if (Emergency)
                DataBlock.ForeColor = Color.Red;
            else
                DataBlock.ForeColor = Color.Lime;
        }

        public void RedrawTarget(PointF LocationF)
        {
            this.LocationF = LocationF;
            TargetReturn.LocationF = LocationF;
            TargetReturn.Angle = Location.BearingTo(LocationReceivedBy.Location);
            string vrchar = " ";
            if (VerticalRate > 100)
                vrchar = "↑";
            else if (VerticalRate < -100)
                vrchar = "↓";
            var oldtext = DataBlock.Text;
            if (Callsign != null)
                DataBlock.Text = Callsign + "\r\n" + (Altitude / 100).ToString("D3") + vrchar + " " + (GroundSpeed / 10).ToString("D2");
            else
                DataBlock.Text = (Altitude / 100).ToString("D3") + vrchar + " " + (GroundSpeed / 10).ToString("D2");
            DataBlock.Redraw = DataBlock.Text != oldtext;
            TargetReturn.Text = TargetChar.ToString();
            TargetReturn.Refresh();
            if (Emergency)
                DataBlock.ForeColor = Color.Red;
            else
                DataBlock.ForeColor = Color.Lime;
        }



        public override string ToString()
        {
            return Callsign;
        }
    }

}
