using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexradDecoder
{
    public class GraphicBlock
    {
        public int Divider { get; set; }
        public int BlockID { get; set; }
        public int BlockLength { get; set; }
        public Page[] Pages { get; set; }
    }
}
