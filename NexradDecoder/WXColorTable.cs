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
        public WXColor GetWXColor(double value)
        {
            var colors = Colors.OrderBy(x => x.MinValue).ToArray();
            if (colors.Length < 1)
                return null;
            if (value < colors[0].MinValue)
                return null;
            for (int i = 0; i < colors.Length - 1; i++)
            {
                if (value < colors[i + 1].MinValue && colors[i + 1].MinValue > colors[i].MinValue)
                {
                    return colors[i];
                }
            }
            return colors.Last();
        }
        public WXColorTable() 
        {
            var denseStipple = new List<byte>()
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
            var lightStipple = new List<byte>()
            {
                0, 0, 0, 0,
                0, 0, 0, 0,
                32, 32, 32, 32,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                2, 2, 2, 2,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                32, 32, 32, 32,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                2, 2, 2, 2,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0,
                0, 0, 0, 0


            };
            Colors = new List<WXColor>()
            {

                new WXColor(20, Color.FromArgb(38, 77, 77)),
                new WXColor(30, Color.FromArgb(38, 77, 77), Color.White, lightStipple),
                new WXColor(40, Color.FromArgb(38, 77, 77), Color.White, denseStipple),
                new WXColor(45, Color.FromArgb(100, 100, 51)),
                new WXColor(50, Color.FromArgb(100, 100, 51), Color.White, lightStipple),
                new WXColor(55, Color.FromArgb(100, 100, 51), Color.White, denseStipple)
            };
        }
        public WXColorTable(List<WXColor> colors) { Colors = colors; }
    }

    public class WXColor
    {
        public double MinValue { get; set; }
        [XmlIgnore]
        public Color MinColor { get; set; }
        [XmlIgnore]
        public Color StippleColor { get; set; }
        public WXColor(double minvalue, Color mincolor)
        {
            MinValue = minvalue;
            MinColor = mincolor;
            StipplePatternList = new List<byte>();
            StippleColor = Color.Transparent;
        }
        public WXColor(double minvalue, Color mincolor, Color stippleColor, List<byte> stipplePattern)
        {
            MinValue = minvalue;
            MinColor = mincolor;
            StipplePatternList = stipplePattern;
            StippleColor = stippleColor;
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
        public byte[] StipplePattern 
        {
            get
            {
                if (StipplePatternList == null)
                    return null;
                return StipplePatternList.ToArray();
            }
        }
        public List<byte> StipplePatternList { get; set; }
        

        public override string ToString()
        {
            return ">="+MinValue;
        }

    }
}
