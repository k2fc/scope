using System.Collections.Generic;

namespace DGScope
{
    public class Runway : Line
    {

        public GeoPoint End1_Distance(double Distance)
        {
            return GeoPoint.FromPoint(End1, Distance, (Direction + 180) % 360);
        }
        public GeoPoint End2_Distance(double Distance)
        {
            return GeoPoint.FromPoint(End2, Distance, Direction);
        }
        public Runway(GeoPoint End1, GeoPoint End2)
        {
            base.End1 = End1;
            base.End2 = End2;
        }

        public string ID { get; set; }

        public bool DrawExtendedCenterlines { get; set; } = false;
        public List<Line> Lines
        {
            get
            {
                List<Line> lineList = new List<Line>();
                lineList.Add(new Line(this.End1, this.End2));
                //Extended Centerline
                if (DrawExtendedCenterlines)
                {
                    lineList.Add(new Line(End1_Distance(1), End1_Distance(10)));
                    lineList.Add(new Line(End1_Distance(2), GeoPoint.FromPoint(End1_Distance(2), 0.5, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(2), GeoPoint.FromPoint(End1_Distance(2), 0.5, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(3), GeoPoint.FromPoint(End1_Distance(3), 0.5, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(3), GeoPoint.FromPoint(End1_Distance(3), 0.5, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(4), GeoPoint.FromPoint(End1_Distance(4), 0.5, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(4), GeoPoint.FromPoint(End1_Distance(4), 0.5, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(5), GeoPoint.FromPoint(End1_Distance(5), 1, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(5), GeoPoint.FromPoint(End1_Distance(5), 1, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(6), GeoPoint.FromPoint(End1_Distance(6), 0.5, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(6), GeoPoint.FromPoint(End1_Distance(6), 0.5, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(7), GeoPoint.FromPoint(End1_Distance(7), 0.5, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(7), GeoPoint.FromPoint(End1_Distance(7), 0.5, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(8), GeoPoint.FromPoint(End1_Distance(8), 0.5, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(8), GeoPoint.FromPoint(End1_Distance(8), 0.5, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(9), GeoPoint.FromPoint(End1_Distance(9), 0.5, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(9), GeoPoint.FromPoint(End1_Distance(9), 0.5, Direction - 90)));
                    lineList.Add(new Line(End1_Distance(10), GeoPoint.FromPoint(End1_Distance(10), 1, Direction + 90)));
                    lineList.Add(new Line(End1_Distance(10), GeoPoint.FromPoint(End1_Distance(10), 1, Direction - 90)));

                    lineList.Add(new Line(End2_Distance(1), End2_Distance(10)));
                    lineList.Add(new Line(End2_Distance(2), GeoPoint.FromPoint(End2_Distance(2), 0.5, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(2), GeoPoint.FromPoint(End2_Distance(2), 0.5, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(3), GeoPoint.FromPoint(End2_Distance(3), 0.5, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(3), GeoPoint.FromPoint(End2_Distance(3), 0.5, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(4), GeoPoint.FromPoint(End2_Distance(4), 0.5, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(4), GeoPoint.FromPoint(End2_Distance(4), 0.5, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(5), GeoPoint.FromPoint(End2_Distance(5), 1, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(5), GeoPoint.FromPoint(End2_Distance(5), 1, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(6), GeoPoint.FromPoint(End2_Distance(6), 0.5, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(6), GeoPoint.FromPoint(End2_Distance(6), 0.5, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(7), GeoPoint.FromPoint(End2_Distance(7), 0.5, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(7), GeoPoint.FromPoint(End2_Distance(7), 0.5, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(8), GeoPoint.FromPoint(End2_Distance(8), 0.5, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(8), GeoPoint.FromPoint(End2_Distance(8), 0.5, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(9), GeoPoint.FromPoint(End2_Distance(9), 0.5, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(9), GeoPoint.FromPoint(End2_Distance(9), 0.5, Direction - 90)));
                    lineList.Add(new Line(End2_Distance(10), GeoPoint.FromPoint(End2_Distance(10), 1, Direction + 90)));
                    lineList.Add(new Line(End2_Distance(10), GeoPoint.FromPoint(End2_Distance(10), 1, Direction - 90)));
                }
                return lineList;
            }
        }
    }

}
