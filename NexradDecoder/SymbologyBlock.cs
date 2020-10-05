using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexradDecoder
{
    public abstract class SymbologyBlock
    {
        public int LayerDivider { get; set; }
        public int LayerLength { get; set; }
        public int LayerPacketCode { get; set; }
        public int Divider { get; set; }
        public int BlockID { get; set; }
        public int BlockLength { get; set; }
        public int NumOfLayers { get; set; }
    }

    public class RasterSymbologyBlock : SymbologyBlock
    {
        
        public int LayerPacketCode2 { get; set; }
        public int LayerPacketCode3 { get; set; }
        public int I_Coord_Start { get; set; }
        public int J_Coord_Start { get; set; }
        public int X_Scale_Int { get; set; }
        public int X_Scale_Fraction { get; set; }
        public int Y_Scale_Int { get; set; }
        public int Y_Scale_Fraction { get; set; }
        public int NumRows { get; set; }
        public int PackingDescriptor { get; set; }
        public Row[] Rows { get; set; }
    }

    public class RadialSymbologyBlock : SymbologyBlock
    {
        public int LayerIndexOfFirstRangeBin { get; set; }
        public int LayerNumberOfRangeBins { get; set; }
        public int I_CenterOfSweep { get; set; }
        public int J_CenterOFSweep { get; set; }
        public double ScaleFactor { get; set; }
        public int NumberOfRadials { get; set; }

        public Radial[] Radials { get; set; }

    }

    public class Radial
    {
        public int[] ColorValues { get; set; }
        public int RadialBytes { get; set; }
        public double StartAngle { get; set; }
        public double AngleDelta { get; set; }

    }

    public class Row
    {
        public int[] Data { get; set; }
        public int Bytes { get; set; }
    }
}
