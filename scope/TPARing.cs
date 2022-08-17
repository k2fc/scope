using System;
using System.Drawing;
namespace DGScope
{
    public class TPARing : TPA
    {
        public override TPAType Type => TPAType.JRing;
        public TPARing(Aircraft aircraft, decimal miles, Color color, Font font, bool showsize) : base(aircraft, miles, color, font, showsize) { }
        
    }

    public class TPACone : TPA
    {
        public override TPAType Type => TPAType.PCone;
        public TPACone(Aircraft aircraft, decimal miles, Color color, Font font, bool showsize) : base(aircraft, miles, color, font, showsize) { }


    }

    public abstract class TPA
    {
        public abstract TPAType Type { get; }
        public Aircraft ParentAircraft { get; set; }
        public bool ShowSize { get; set; } = true;
        public decimal Miles { get; set; }
        Color color;
        public Color Color => Label.ForeColor;
        public TPA(Aircraft aircraft, decimal miles, Color color, Font font, bool showsize)
        {
            ParentAircraft = aircraft;
            Miles = miles;
            ShowSize = showsize;
            Label = new TransparentLabel()
            {
                AutoSize = true,
                ForeColor = color,
                Font = font
            };
        }
        public TransparentLabel Label { get; private set; }

    }

    public enum TPAType
    {
        JRing, PCone
    }
}
