using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DGScope
{
    public class BeaconCodeRange
    {
        private int start;
        private int? end;

        public string Begin
        {
            get
            {
                return Convert.ToString(start, 8).PadLeft(4, '0');
            }
            set
            {
                start = Convert.ToInt32(value, 8);
            }
        }

        public string? End
        {
            get
            {
                if (end == null)
                {
                    return null;
                }
                return Convert.ToString(end.Value, 8).PadLeft(4,'0');
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    end = Convert.ToInt32(value, 8);
                }
                else
                {
                    end = null;
                }
            }
        }

        public override string ToString()
        {
            if (end == null)
            {
                return Begin;
            }
            return Begin + " - " + End;
        }

        public bool IsInRange(string squawk)
        {
            if (!TryParse(squawk, out int beacon))
            {
                return false;
            }
            if (end == null)
            {
                return beacon == start;
            }
            return beacon >= start && beacon <= end;
        }

        private bool TryParse(string squawk, out int beacon_int)
        {
            beacon_int = 0;
            if (squawk == null)
            {
                return false;
            }
            var m = Regex.Match(squawk, "[0-7]{4}");
            if (!m.Success)
            {
                return false;
            }
            beacon_int = Convert.ToInt32(m.Value, 8);
            return true;
        }
    }
}
