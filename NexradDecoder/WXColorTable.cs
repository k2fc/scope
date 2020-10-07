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
        public Color GetColor(double value)
        {
            var colors = Colors.OrderBy(x => x.MinValue).ToArray();
            if (colors.Length < 1)
                return Color.Transparent;
            if (value < colors[0].MinValue)
                return Color.Transparent;
            for (int i = 0; i < colors.Length - 1; i++)
            {
                if (value < colors[i + 1].MinValue && colors[i+1].MinValue > colors[i].MinValue)
                {
                    var scaledvalue = (value - colors[i].MinValue) / (colors[i + 1].MinValue - colors[i].MinValue);
                    var rrange = colors[i].MaxColor.R - colors[i].MinColor.R;
                    var grange = colors[i].MaxColor.G - colors[i].MinColor.G;
                    var brange = colors[i].MaxColor.B - colors[i].MinColor.B;
                    var newr = (scaledvalue * rrange) + colors[i].MinColor.R;
                    var newg = (scaledvalue * grange) + colors[i].MinColor.G;
                    var newb = (scaledvalue * brange) + colors[i].MinColor.B;
                    return Color.FromArgb((int)newr, (int)newg, (int)newb);
                }
            }
            return colors.Last().MaxColor;
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
        public Color MaxColor { get; set; }
        public WXColor(double minvalue, Color mincolor, Color maxcolor)
        {
            MinValue = minvalue;
            MinColor = mincolor;
            MaxColor = maxcolor;
        }
        public WXColor() { }

        [XmlElement("MaxColor")]
        [Browsable(false)]
        public int MaxColorAsArgb
        {
            get
            {
                return MaxColor.ToArgb();
            }
            set
            {
                MaxColor = Color.FromArgb(value);
            }
        }
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

        public override string ToString()
        {
            return ">="+MinValue;
        }

    }
}
