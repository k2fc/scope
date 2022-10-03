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
        public double? Track { get; set; } = null;
        public TPACone(Aircraft aircraft, decimal miles, Color color, Font font, bool showsize, double? track = null) : base(aircraft, miles, color, font, showsize) 
        {
            Track = track;
        }


    }

    public abstract class TPA
    {
        public abstract TPAType Type { get; }
        public Aircraft ParentAircraft { get; set; }
        public bool ShowSize { get; set; } = true;
        public decimal Miles { get; set; }
        public Color Color { get => Label.ForeColor; set => Label.ForeColor = value; }
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
