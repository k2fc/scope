using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace DGScope
{
    public class WXColorTable
    {
        
        public List<WXColor> Colors { get; set; } = new List<WXColor>();
        public bool[] LevelsEnabled;
        public bool[] LevelsAvailable;
        public WXColor GetWXColor(double value)
        {
            var colors = Colors.OrderBy(x => x.MinValue).ToArray();
            if (colors.Length < 1)
                return null;
            if (value < colors[0].MinValue)
                return null;
            if (LevelsEnabled == null || LevelsEnabled.Length != colors.Length)
                LevelsEnabled = new bool[colors.Length];
            if (LevelsAvailable == null || LevelsAvailable.Length != colors.Length)
                LevelsAvailable = new bool[colors.Length];
            for (int i = colors.Length - 1; i >= 0; i--)
            {
                if (value > colors[i].MinValue)
                {
                    LevelsAvailable[i] = true;
                    if (LevelsEnabled[i])
                    { 
                        return colors[i];
                    }
                }
            }
            return null;
        }
        public WXColorTable() 
        {
        }
        public WXColorTable(List<WXColor> colors) 
        { 
            Colors = colors;
            LevelsEnabled = new bool[colors.Count];
            LevelsAvailable = new bool[colors.Count];
            for (int i = 0; i < colors.Count; i++)
            {
                LevelsEnabled[i] = true;
            }
        }
    }

    public class WXColor
    {
        private static byte[] denseStipple = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            32, 32, 32, 32, 0, 0, 0, 0,
            0, 0, 0, 0, 4, 4, 4, 4,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            32, 32, 32, 32, 0, 0, 0, 0,
            0, 0, 0, 0, 4, 4, 4, 4,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            32, 32, 32, 32, 0, 0, 0, 0,
            0, 0, 0, 0, 4, 4, 4, 4,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            32, 32, 32, 32, 0, 0, 0, 0,
            0, 0, 0, 0, 4, 4, 4, 4,
            0, 0, 0, 0, 0, 0, 0, 0
        };
        private static byte[] lightStipple = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            32, 32, 32, 32, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            2, 2, 2, 2, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            32, 32, 32, 32, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            2, 2, 2, 2, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
        };
        public double MinValue { get; set; }
        [XmlIgnore]
        public Color MinColor { get; set; }
        [XmlIgnore]
        public Color StippleColor { get; set; }

        public WXColor(double minvalue, Color mincolor)
        {
            MinValue = minvalue;
            MinColor = mincolor;
            StippleColor = Color.Transparent;
        }
        public WXColor(double minvalue, Color mincolor, Color stippleColor, StippleType stippleType)
        {
            MinValue = minvalue;
            MinColor = mincolor;
            StippleColor = stippleColor;
            Stipple = stippleType;
        }
        public WXColor() { }

        
        [XmlElement("MinColor")]
        [Browsable(false)]
        public int MinColorAsArgb
        {
            get
            {
                return MinColor.ToArgb();
            }
            set
            {
                MinColor = Color.FromArgb(value);
            }
        }
        [XmlElement("StippleColor")]
        [Browsable(false)]
        public int StippleColorAsArgb
        {
            get
            {
                return StippleColor.ToArgb();
            }
            set
            {
                StippleColor = Color.FromArgb(value);
            }
        }
        [XmlIgnore]
        [Browsable(false)]
        public byte[] StipplePattern 
        {
            get
            {
                switch (Stipple)
                {
                    case StippleType.LIGHT:
                        return lightStipple;
                    case StippleType.DENSE:
                        return denseStipple;
                }
                return new byte[256];
            }
        }
        public StippleType Stipple { get; set; } = StippleType.NONE;

        public override string ToString()
        {
            return ">="+MinValue;
        }
        public enum StippleType
        {
            NONE = 0,
            LIGHT = 1,
            DENSE = 2
        }
    }
}
