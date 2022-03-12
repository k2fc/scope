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
        public WXColorTable() { }
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
            StipplePatternList = null;
            StippleColor = Color.Transparent;
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
