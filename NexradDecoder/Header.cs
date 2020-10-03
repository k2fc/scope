using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexradDecoder
{
    public class HeaderBlock
    {
        public int Code { get; set; }
        public DateTime DateTime { get; set; }
        public int Length { get; set; }
        public int sourceID { get; set; }
        public int destinationID { get; set; }
        public int numberofBlocks { get; set; }
    }
}
