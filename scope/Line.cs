namespace DGScope
{
    public class Line
    {
        public GeoPoint End1 { get; set; }
        public GeoPoint End2 { get; set; }
        public double Direction { get => End1.BearingTo(End2); }
        public double Length { get => End1.DistanceTo(End2); }
        public Line(GeoPoint End1, GeoPoint End2) { this.End1 = End1; this.End2 = End2; }
        public GeoPoint MidPoint { 
            get
            {
                if (End1 == null || End2 == null)
                    return null;
                double latitude = -((End1.Latitude - End2.Latitude) / 2) + End1.Latitude;
                double longitude = ((End1.Longitude - End2.Longitude) / 2) + End2.Longitude;
                return new GeoPoint(latitude, longitude);
            } 
        }
        public Line() { }
        public static bool TryParse(string linestring, out Line line)
        {
            linestring = linestring.Trim();
            line = new Line();
            var lineparts = linestring.Split(' ');
            if (lineparts.Length < 4)
                return false;
            string point1 = string.Concat(lineparts[0], " ", lineparts[1]);
            string point2 = string.Concat(lineparts[2], " ", lineparts[3]);
            if (GeoPoint.TryParse(point1, out GeoPoint geoPoint1))
                if (GeoPoint.TryParse(point2, out GeoPoint geoPoint2))
                {
                    line.End1 = geoPoint1;
                    line.End2 = geoPoint2;
                    return true;
                }
            return false;
        }
    }
}
