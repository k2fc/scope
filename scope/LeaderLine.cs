using System;
using System.Drawing;

namespace DGScope
{
    public class ConnectingLineF : IScreenObject
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }
        public PointF LocationF { get { return new PointF(Start.X > End.X ? End.X : Start.X, Start.Y > End.Y ? End.Y : Start.Y); } set { } }
        public PointF NewLocation { get; set; }
        public SizeF SizeF { get { return new SizeF(Math.Abs(Start.X - End.X), Math.Abs(Start.Y - End.Y)); } set { } }

        public RectangleF BoundsF => new RectangleF(LocationF, SizeF);

        public bool IntersectsWith(RectangleF Rectangle)
        {
            return LineIntersectsRect(Start, End, Rectangle);
        }
        public bool IntersectsWith(ConnectingLineF Line)
        {
            return LineIntersectsLine(Line.Start, Line.End, Start, End);
        }
        private static bool LineIntersectsRect(PointF p1, PointF p2, RectangleF r)
        {
            return LineIntersectsLine(p1, p2, new PointF(r.Left, r.Top), new PointF(r.Right, r.Top)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.Right, r.Y), new PointF(r.Right, r.Bottom)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.Right, r.Bottom), new PointF(r.Left, r.Bottom)) ||
                   LineIntersectsLine(p1, p2, new PointF(r.Left, r.Bottom), new PointF(r.Left, r.Top)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }

        private static bool LineIntersectsLine(PointF l1p1, PointF l1p2, PointF l2p1, PointF l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }
        public Aircraft ParentAircraft { get; set; }
    }

}
