using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class ScratchpadFilter
    {
        public string ScratchpadValue { get; set; }
        public int ScratchpadNum { get; set; } = 1;
        public ScratchpadFilterType ScratchpadFilterType { get; set; } = ScratchpadFilterType.Exclusion;
        public bool Match(Aircraft aircraft)
        {
            if (aircraft == null) return false;
            switch (ScratchpadNum)
            {
                case 1:
                    if (aircraft.Scratchpad == ScratchpadValue) return true;
                    return false;
                case 2:
                    if (aircraft.Scratchpad2 == ScratchpadValue) return true;
                    return false;
                default:
                    return false;
            }
        }
        public override string ToString()
        {
            return ScratchpadValue;
        }
    }
    public enum ScratchpadFilterType
    {
        Exclusion, Ineligible
    }
}
