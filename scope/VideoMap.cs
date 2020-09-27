using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    public class VideoMap
    {
        public string Name { get; set; }
        public List<Line> Lines { get; set; } = new List<Line>();
        public bool Visible { get; set; } = false;
        public override string ToString()
        {
            return Name;
        }
    }
}
