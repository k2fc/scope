using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libmetar;

namespace DGScope
{
    public class Altitude
    {
        public int Value { get; set; }
        public AltitudeType AltitudeType { get; set;}
        public int TransitionAltitude { get; set; }
        public int PressureAltitude 
        { 
            get
            {
                if (AltitudeType == AltitudeType.Pressure)
                    return Value;
                var altimetervalue = Converter.Pressure(Altimeter, libmetar.Enums.PressureUnit.inHG).Value;
                var correction = (int)((altimetervalue - 29.92) * 1000);
                var newvalue = Value;
                if (this.AltitudeType == AltitudeType.True)
                    newvalue -= correction;
                return newvalue;
            }
            set
            {
                UpdateAltitude(value, AltitudeType.Pressure);
            }
        }
        public int TrueAltitude
        {
            get
            {
                if (AltitudeType == AltitudeType.True)
                    return Value;
                var altimetervalue = Converter.Pressure(Altimeter, libmetar.Enums.PressureUnit.inHG).Value;
                var correction = (int)((altimetervalue - 29.92) * 1000);
                var newvalue = Value;
                if (this.AltitudeType == AltitudeType.Pressure)
                    newvalue += correction;
                return newvalue;
            }
            set
            {
                UpdateAltitude(value, AltitudeType.True);
            }
        }

        private object convertLockObject = new object();

        public Altitude (){}

        public void SetAltitudeProperties (int TransitionAltitude, Pressure Altimeter)
        {
            this.TransitionAltitude = TransitionAltitude;
            this.Altimeter = Altimeter;
        }

        public void UpdateAltitude (int Value, AltitudeType type)
        {
            lock (convertLockObject)
            {
                this.Value = Value;
                AltitudeType = type;
            }
        }

        private Pressure Altimeter;
        public Altitude ConvertTo (AltitudeType type)
        {
            lock (convertLockObject)
            {
                if (type != AltitudeType)
                {
                    if (type == AltitudeType.Pressure)
                    {
                        if (this.AltitudeType == AltitudeType.True)
                        {
                            Value = PressureAltitude;
                            AltitudeType = AltitudeType.Pressure;
                        }
                    }
                    else if (type == AltitudeType.True)
                    {
                        if (this.AltitudeType == AltitudeType.Pressure)
                        {
                            Value = TrueAltitude;
                            AltitudeType = AltitudeType.True;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                return this;
            }
        }
        public override string ToString()
        {
            if (ConvertTo(AltitudeType.True).Value > TransitionAltitude)
            {
                return string.Format("FL{0}", (Value / 100).ToString("D3"));
            }
            return string.Format("{0}ft.", Value);
        }
    }
    public enum AltitudeType
    {
        Pressure, True
    }
}
