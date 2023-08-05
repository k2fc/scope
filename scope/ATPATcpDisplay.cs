using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class ATPATCPDisplay
    {
        public string TCP { get; set; }
        public ATPAStatus ConeType { get; set; } = ATPAStatus.Alert;
        public override string ToString()
        {
            return TCP + " - " + ConeType;
        }
    }

}
