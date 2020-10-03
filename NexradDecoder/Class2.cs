using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexradDecoder
{
    static class Program
    {
        static void Main(string[] args)
        {
            var reflectivitydecoder = new RadialPacketDecoder();
            reflectivitydecoder.setFileResource("E:\\Users\\Dennis\\Downloads\\KOKX_SDUS51_N0ROKX_202009292354");
            var header= reflectivitydecoder.parseMHB();
            var description = reflectivitydecoder.parsePDB();
            var symbology = reflectivitydecoder.parsePSB();
            GraphicBlock graphic;
            if (description.GraphicOffset != 0)
            {
                graphic = reflectivitydecoder.parseGAB();
            }
        }
    } 
}
