using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DGScope.STARS
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [JsonObject]
    public class TargetExtentSymbols
    {
        static double acpangle = Math.PI / 2048;
        public SearchTargetParams SearchTargets { get; set; } = new SearchTargetParams();
        public FusedTrackTargetSymbolParams FusedTracks { get; set; } = new FusedTrackTargetSymbolParams();
        public BeaconTargetParams BeaconTargets { get; set; } = new BeaconTargetParams();
        public MultiRadarTargetParams MultiRadarTargets { get; set; } = new MultiRadarTargetParams();
        public int PositionSymbolOffset { get; set; }
        public FMATargetSymbolParams FMATargetSymbols { get; set; } = new FMATargetSymbolParams();

        public GeoPoint PositionSymbolLocation(Aircraft aircraft, Radar radar)
        {
            var loc = aircraft.SweptLocation(radar);
            if (radar.RadarType != RadarType.SLANT_RANGE || PositionSymbolOffset == 0) return loc;
            var bearing = radar.Location.BearingTo(loc);
            return loc.FromPoint(PositionSymbolOffset / 32d, bearing);
        }
        public double TargetWidth(Aircraft aircraft, Radar radar, double scale, double pixelscale)
        {
            double size = 0;
            double minsize = 0;
            var loc = aircraft.SweptLocation(radar);
            switch (radar.RadarType)
            {
                case RadarType.SLANT_RANGE:
                    var dist = radar.Location.DistanceTo(loc);
                    double acp;
                    if (dist <= 10)
                    {
                        acp = SearchTargets.AzimuthExtents.Ten;
                    }
                    else if (dist <= 20)
                    {
                        var diff = SearchTargets.AzimuthExtents.Twenty - SearchTargets.AzimuthExtents.Ten;
                        var diffdist = (dist - 10) / 10;
                        acp = SearchTargets.AzimuthExtents.Ten + (diffdist * diff);
                    }
                    else if (dist <= 30)
                    {
                        var diff = SearchTargets.AzimuthExtents.Thirty - SearchTargets.AzimuthExtents.Twenty;
                        var diffdist = (dist - 20) / 10;
                        acp = SearchTargets.AzimuthExtents.Twenty + (diffdist * diff);
                    }
                    else if (dist <= 40)
                    {
                        var diff = SearchTargets.AzimuthExtents.Forty - SearchTargets.AzimuthExtents.Thirty;
                        var diffdist = (dist - 30) / 10;
                        acp = SearchTargets.AzimuthExtents.Thirty + (diffdist * diff);
                    }
                    else if (dist <= 50)
                    {
                        var diff = SearchTargets.AzimuthExtents.Fifty - SearchTargets.AzimuthExtents.Forty;
                        var diffdist = (dist - 40) / 10;
                        acp = SearchTargets.AzimuthExtents.Forty + (diffdist * diff);
                    }
                    else if (dist <= 60)
                    {
                        var diff = SearchTargets.AzimuthExtents.Sixty - SearchTargets.AzimuthExtents.Fifty;
                        var diffdist = (dist - 50) / 10;
                        acp = SearchTargets.AzimuthExtents.Fifty + (diffdist * diff);
                    }
                    else
                    {
                        acp = SearchTargets.AzimuthExtents.Sixty;
                    }
                    var angle = acp * acpangle;
                    minsize = ((SearchTargets.AzimuthExtentMinimum / 6076f) * dist) / scale;
                    size = dist * Math.Tan(angle) / scale;
                    if (minsize > size)
                    {
                        size = minsize;
                    }
                    break;
                case RadarType.FUSED:
                    minsize = FusedTracks.MinimumPixelDimension * pixelscale;
                    size = (FusedTracks.NormalSymbolDistanceDimension / 60.76d) / scale;
                    break;
                case RadarType.GROUND_RANGE:
                    return MultiRadarTargets.SymbolWidth;
            }
            if (minsize > size)
            {
                return minsize;
            }
            return size;
        }

        [Serializable()]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonObject]
        public class SearchTargetParams
        {
            public int RangeExtent { get; set; }
            [DisplayName("Azimuth Extent")]
            public AzimuthExtentValues AzimuthExtents { get; set; } = new AzimuthExtentValues();
            public int AzimuthExtentMinimum { get; set; }

            [Serializable()]
            [TypeConverter(typeof(ExpandableObjectConverter))]
            [JsonObject]
            public class AzimuthExtentValues
            {
                [DisplayName("0 to 10 nmi")]
                public int Ten { get; set; } = 28;
                [DisplayName("20 nmi")]
                public int Twenty { get; set; } = 19;
                [DisplayName("30 nmi")]
                public int Thirty { get; set; } = 16;
                [DisplayName("40 nmi")]
                public int Forty { get; set; } = 12;
                [DisplayName("50 nmi")]
                public int Fifty { get; set; } = 11;
                [DisplayName("60 nmi")]
                public int Sixty { get; set; } = 9;
            }
        }
        [Serializable()]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonObject]
        public class FusedTrackTargetSymbolParams
        {
            public int MinimumPixelDimension { get; set; }
            public int NormalSymbolDistanceDimension { get; set; }
            public int ReducedSymbolDistanceDimension { get; set; }
            public int SymbolOpacity { get; set; }
            public int LowConfidencePrimarySymbolRatio { get; set; }
            public int PrimarySymbolMaximumPixelDimension { get; set; }
        }
        [Serializable()]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonObject]
        public class BeaconTargetParams
        {
            public int RangeExtent { get; set;}
            public int AzimuthExtentFactor { get; set; }
            public int RangeOffset { get; set; }
        }

        [Serializable()]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonObject]
        public class MultiRadarTargetParams
        {
            public int SymbolFillRange { get; set; }
            public int SymbolHeight { get; set; }
            public int SymbolWidth { get; set; }
            public int UncorrSymbolPlotSize { get; set; } = 8;
        }

        [Serializable()]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonObject]
        public class FMATargetSymbolParams
        {
            public int Radius { get; set; }
            public int Thickness { get; set; }
        }
    }
}
