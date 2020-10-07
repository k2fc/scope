using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexradDecoder
{
    public class DescriptionBlock
    {
        public int Divider { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Height { get; set; }
        public int Code { get; set; }
        public int Mode { get; set; }
        public int VolumeCoveragePattern { get; set; }
        public int SequenceNumber { get; set; }
        public int ScanNumber { get; set; }
        public DateTime ScanTime { get; set; }
        public DateTime GenerationTime { get; set; }
        public int ProductSpecific_1 { get; set; }
        public int ProductSpecific_2 { get; set; }
        public int ElevationNumber { get; set; }
        public int ProductSpecific_3 { get; set; }
        public int[] Threshold { get; set; } = new int[16];
        /*
        public int Threshold_1 { get; set; }
        public int Threshold_2 { get; set; }
        public int Threshold_3 { get; set; }
        public int Threshold_4 { get; set; }
        public int Threshold_5 { get; set; }
        public int Threshold_6 { get; set; }
        public int Threshold_7 { get; set; }
        public int Threshold_8 { get; set; }
        public int Threshold_9 { get; set; }
        public int Threshold_10 { get; set; }
        public int Threshold_11 { get; set; }
        public int Threshold_12 { get; set; }
        public int Threshold_13 { get; set; }
        public int Threshold_14 { get; set; }
        public int Threshold_15 { get; set; }
        public int Threshold_16 { get; set; }
        */
        public int ProductSpecific_4 { get; set; }
        public int ProductSpecific_5 { get; set; }
        public int ProductSpecific_6 { get; set; }
        public int ProductSpecific_7 { get; set; }
        public int ProductSpecific_8 { get; set; }
        public int ProductSpecific_9 { get; set; }
        public int ProductSpecific_10 { get; set; }
        public int Spot_Blank { get; set; }
        public int SymbologyOffset { get; set; }
        public int GraphicOffset { get; set; }
        public int TabularOffset { get; set; }

        public int Version { get; set; }
    }
}
