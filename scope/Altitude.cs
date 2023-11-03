using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class Altitude
    {
        public int Value { get; set; }
        private AltitudeType _alttype = AltitudeType.Unknown;
        public AltitudeType AltitudeType {
            get => _alttype;
            set
            {
                _alttype = value;
            }
                 
        }
        public int TransitionAltitude { get; set; }
        public int PressureAltitude 
        { 
            get
            {
                if (AltitudeType == AltitudeType.Pressure)
                    return Value;
                var altimetervalue = Altimeter.Value;
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
                var altimetervalue = Altimeter.Value;
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

        public static Altitude Clone (Altitude altitude)
        {
            Altitude newAlt = new Altitude();
            newAlt.TransitionAltitude = altitude.TransitionAltitude;
            newAlt.Altimeter = altitude.Altimeter;
            newAlt.Value = altitude.Value;
            newAlt.AltitudeType = altitude.AltitudeType;
            return newAlt;
        }
        public Altitude Clone()
        {
            return Clone(this);
        }

        private object convertLockObject = new object();

        public Altitude (){}

        public void SetAltitudeProperties (int TransitionAltitude, Altimeter Altimeter)
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

        private Altimeter Altimeter = new Altimeter();
        public Altitude ConvertTo (AltitudeType type)
        {
            Altitude newAlt = new Altitude();
            lock (convertLockObject)
            {
                if (type != AltitudeType)
                {
                    if (type == AltitudeType.Pressure)
                    {
                        if (this.AltitudeType == AltitudeType.True)
                        {
                            newAlt.Value = PressureAltitude;
                            newAlt.AltitudeType = AltitudeType.Pressure;
                        }
                    }
                    else if (type == AltitudeType.True)
                    {
                        if (this.AltitudeType == AltitudeType.Pressure)
                        {
                            newAlt.Value = TrueAltitude;
                            newAlt.AltitudeType = AltitudeType.True;
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    newAlt.Value = Value;
                    newAlt.AltitudeType = AltitudeType;
                }
                return newAlt;
            }
        }
        public override string ToString()
        {
            if (AltitudeType == AltitudeType.Pressure)
            {
                return string.Format("FL{0}", (Value / 100).ToString("D3"));
            }
            return string.Format("{0}ft.", Value);
        }
    }
    public enum AltitudeType
    {
        Pressure = 0, 
        True = 1,
        Unknown = 2
    }
}
