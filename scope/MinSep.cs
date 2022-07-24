using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class MinSep
    {
        private const double DESIREDPRECISION = .01;
        private const int START_SECONDS_STEP = 30;

        
        public Aircraft Plane1 { get; private set; }
        public Aircraft Plane2 { get; private set; }
        public double? MinSepDistance { get; private set; } = 0;
        public GeoPoint Point1 { get; private set; }
        public GeoPoint Point2 { get; private set; }
        public Line Line1 { get; private set; } = new Line();
        public Line Line2 { get; private set; } = new Line();
        public Line SepLine { get; private set; } = new Line();
        public TransparentLabel Label { get; private set; } = new TransparentLabel() { AutoSize = true };
        public MinSep(Aircraft Plane1, Aircraft Plane2)
        {
            this.Plane1 = Plane1;
            this.Plane2 = Plane2;
        }
        bool workingonit = false;
        public async Task<bool> CalculateMinSep()
        {
            if (workingonit)
                return false;
            workingonit = true;
            if (Plane1 == null || Plane2 == null)
            {
                workingonit = false;
                return false;
            }
            if (Plane1.SweptLocation == null || Plane2.SweptLocation == null)
            {
                workingonit = false;
                return false;
            }
            double secondsStep = START_SECONDS_STEP;
            double minsep = Plane1.SweptLocation.DistanceTo(Plane2.SweptLocation);
            double testdistance = Plane1.SweptLocation.FromPoint(Plane1.GroundSpeed / 3600d, Plane1.ExtrapolateTrack()).DistanceTo(Plane2.SweptLocation.FromPoint(Plane2.GroundSpeed / 3600d, Plane2.ExtrapolateTrack()));
            if (testdistance >= minsep)
            {
                // Planes are moving away from each other
                // NO XING
                Point1 = Plane1.SweptLocation;
                Point2 = Plane2.SweptLocation;
                SepLine.End1 = Point1;
                SepLine.End2 = Point2;
                Line1 = new Line();
                Line2 = new Line();
                MinSepDistance = null;
                workingonit = false;
                return false;
            }
            
            double lastdistance = 0;
            double hours = 0;
            GeoPoint minPoint1 = null, minPoint2 = null;
            while (Math.Abs(testdistance - minsep) > DESIREDPRECISION || Math.Abs(testdistance - lastdistance) > DESIREDPRECISION)
            {
                hours += 1 / (3600 / secondsStep);
                GeoPoint point1 = Plane1.SweptLocation.FromPoint(Plane1.GroundSpeed * hours, Plane1.ExtrapolateTrack());
                GeoPoint point2 = Plane2.SweptLocation.FromPoint(Plane2.GroundSpeed * hours, Plane2.ExtrapolateTrack());
                lastdistance = testdistance;
                testdistance = point1.DistanceTo(point2);
                if (testdistance < minsep)
                {
                    minsep = testdistance;
                    minPoint1 = point1;
                    minPoint2 = point2;
                }
                else if (testdistance > lastdistance) // going the wrong way
                {
                    secondsStep = -(secondsStep / 2d); // reverse at half speed
                }

            }
            MinSepDistance = minsep;
            Point1 = minPoint1;
            Point2 = minPoint2;
            Line1.End1 = Plane1.SweptLocation;
            Line1.End2 = Point1;
            Line2.End1 = Plane2.SweptLocation;
            Line2.End2 = Point2;
            SepLine.End1 = Point1;
            SepLine.End2 = Point2;
            workingonit = false;
            return true;
        }
    }
}

