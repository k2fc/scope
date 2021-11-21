using System;
using System.Drawing;

namespace DGScope
{
    public class RangeBearingLine
    {
        private PointF _start;
        public PointF Start
        {
            get { return _start; }
            set
            {
                _start = value;

            }
        }
        private PointF _end;
        public PointF End
        {
            get { return _end; }
            set
            {
                _end = value;
            }
        }

        public GeoPoint? StartGeo { get; set; }
        public GeoPoint? EndGeo { get; set; }

        public Aircraft? StartPlane { get; set; } = null;
        public Aircraft? EndPlane { get; set; } = null;
        public PointF LocationF { get { return new PointF(Start.X > End.X ? End.X : Start.X, Start.Y > End.Y ? End.Y : Start.Y); } set { } }
        public SizeF SizeF { get { return new SizeF(Math.Abs(Start.X - End.X), Math.Abs(Start.Y - End.Y)); } set { } }

        public RectangleF BoundsF => new RectangleF(LocationF, SizeF);
        public Line Line { get; set; } = new Line();
        public TransparentLabel Label = new TransparentLabel() { AutoSize = true };

    }

}
