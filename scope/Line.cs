namespace DGScope
{
    public class Line
    {
        public GeoPoint End1 { get; set; }
        public GeoPoint End2 { get; set; }
        public double Direction { get => End1.BearingTo(End2); }
        public double Length { get => End1.DistanceTo(End2); }
        public Line(GeoPoint End1, GeoPoint End2) { this.End1 = End1; this.End2 = End2; }
        public Line() { }
        public static Line Parse(string line)
        {
            line = line.Trim();
            var lineparts = line.Split(' ');
            string point1 = string.Concat(lineparts[0], " ", lineparts[1]);
            string point2 = string.Concat(lineparts[2], " ", lineparts[3]);
            return new Line(GeoPoint.Parse(point1), GeoPoint.Parse(point2));
        }
    }
}
