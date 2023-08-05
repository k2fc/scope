﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.STARS
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [JsonObject]
    public class PrefSet
    {
        public GeoPoint ScreenCenterPoint { get; set; }
        public PointF PreviewAreaLocation { get; set; }
        public List<int> DisplayedMaps { get; set; } = new List<int>();
        public bool RangeRingsDisplayed { get; set; }
        public GeoPoint RangeRingLocation { get; set; }
        public int RangeRingSpacing { get; set; }
        public List<string> QuickLookedTCPs { get; set; } = new List<string>();
        public int AltitudeFilterAssociatedMax { get; set; } = 99900;
        public int AltitudeFilterAssociatedMin { get; set; } = -9900;
        public int AltitudeFilterUnAssociatedMax { get; set; } = 99900;
        public int AltitudeFilterUnAssociatedMin { get; set; } = -9900;
        public BrightnessSettings Brightness { get; set;  } = new BrightnessSettings();

        [Serializable()]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [JsonObject]
        public class BrightnessSettings
        {
            private int dcb = 100;
            private int bkc = 100;
            private int mpa = 100;
            private int mpb = 100;
            private int fdb = 100;
            private int lst = 100;
            private int pos = 100;
            private int ldb = 100;
            private int oth = 100;
            private int tls = 100;
            private int rr = 100;
            private int cmp = 100;
            private int bcn = 100;
            private int pri = 100;
            private int hst = 100;
            private int wx = 100;
            private int wxc = 100;
            
            public int DCB
            {
                get => getMinMaxValue(dcb, 25, 100, false);
                set => dcb = getMinMaxValue(value, 25, 100, false);
            }
            public int Background
            {
                get => getMinMaxValue(bkc, 25, 100, false);
                set => bkc = getMinMaxValue(value, 25, 100, false);
            }
            public int MapA
            {
                get => getMinMaxValue(mpa, 5, 100, false);
                set => mpa = getMinMaxValue(value, 5, 100, false);
            }
            public int MapB
            {
                get => getMinMaxValue(mpb, 5, 100, false);
                set => mpb = getMinMaxValue(value, 5, 100, false);
            }
            public int FullDataBlocks
            {
                get => getMinMaxValue(fdb, 5, 100, true);
                set => fdb = getMinMaxValue(value, 5, 100, true);
            }
            public int Lists
            {
                get => getMinMaxValue(lst, 25, 100, false);
                set => lst = getMinMaxValue(value, 25, 100, false);
            }
            public int PositionSymbols
            {
                get => getMinMaxValue(pos, 5, 100, true);
                set => pos = getMinMaxValue(value, 5, 100, true);
            }
            public int LimitedDataBlocks
            {
                get => getMinMaxValue(ldb, 5, 100, true);
                set => ldb = getMinMaxValue(value, 5, 100, true);
            }
            public int OtherFDBs
            {
                get => getMinMaxValue(oth, 5, 100, true);
                set => oth = getMinMaxValue(value, 5, 100, true);
            }
            public int Tools
            {
                get => getMinMaxValue(tls, 5, 100, true);
                set => tls = getMinMaxValue(value, 5, 100, true);
            }
            public int RangeRings
            {
                get => getMinMaxValue(rr, 5, 100, true);
                set => rr = getMinMaxValue(value, 5, 100, true);
            }
            public int Compass
            {
                get => getMinMaxValue(cmp, 5, 100, true);
                set => cmp = getMinMaxValue(value, 5, 100, true);
            }
            public int BeaconTargets
            {
                get => getMinMaxValue(bcn, 5, 100, true);
                set => bcn = getMinMaxValue(value, 5, 100, true);
            }
            public int PrimaryTargets
            {
                get => getMinMaxValue(pri, 5, 100, true);
                set => pri = getMinMaxValue(value, 5, 100, true);
            }
            public int History
            {
                get => getMinMaxValue(hst, 5, 100, true);
                set => hst = getMinMaxValue(value, 5, 100, true);
            }
            public int Weather
            {
                get => getMinMaxValue(wx, 5, 100, false);
                set => wx = getMinMaxValue(value, 5, 100, false);
            }
            public int WeatherContrast
            {
                get => getMinMaxValue(wxc, 5, 100, false);
                set => wxc = getMinMaxValue(value, 5, 100, false);
            }

            private static int getMinMaxValue (int value, int min, int max, bool orZero)
            {
                if (value >= min && value <= max)
                {
                    return value;
                }
                else if (value < min && (value > 0 || !orZero))
                {
                    return min;
                }
                else if (value > max)
                {
                    return max;
                }
                else
                {
                    return 0;
                }
            }
        }
        
    }
}
