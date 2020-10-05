//
// Created by Chris Harrell, 09/10/2010
//
// Base NexradDecoder class.

namespace NexradDecoder
{
    public class RadialPacketDecoder : NexradDecoder
    {
        public override void parseLayers()
        {
            RadialSymbologyBlock symbology_block = new RadialSymbologyBlock();
            symbology_block.LayerDivider = readHalfWord();
            symbology_block.LayerLength = readWord();
            symbology_block.LayerPacketCode = readHalfWord();
            symbology_block.LayerIndexOfFirstRangeBin = readHalfWord();
            symbology_block.LayerNumberOfRangeBins = readHalfWord();
            symbology_block.I_CenterOfSweep = readHalfWord();
            symbology_block.J_CenterOFSweep = readHalfWord();
            symbology_block.ScaleFactor = readHalfWord() / 1000;
            symbology_block.NumberOfRadials = readHalfWord();
            symbology_block.Radials = new Radial[symbology_block.NumberOfRadials];
            for (int i = 0; i < symbology_block.NumberOfRadials; i++)
            {
                int bytes = readHalfWord();
                double startangle = readHalfWord() / 10;
                double angledelta = readHalfWord() / 10;
                symbology_block.Radials[i] = new Radial();
                symbology_block.Radials[i].StartAngle = startangle;
                symbology_block.Radials[i].RadialBytes = bytes;
                symbology_block.Radials[i].AngleDelta = angledelta;
                if (symbology_block.LayerPacketCode == -20705)
                {
                    symbology_block.Radials[i].ColorValues = new int[0];
                    for (int j = 0; j < bytes * 2; j++)
                    {
                        var tempcolorvalues = parseRLE();
                        symbology_block.Radials[i].ColorValues = ArrayMerge.Merge(symbology_block.Radials[i].ColorValues, tempcolorvalues);
                    }
                }
                else if (symbology_block.LayerPacketCode == 16)
                {
                    symbology_block.Radials[i].ColorValues = new int[bytes];
                    for (int j = 0; j < bytes; j++)
                    {
                        symbology_block.Radials[i].ColorValues[j] = readByte() / 16;
                    }
                }
            }
            base.symbology_block = symbology_block;
        }

        public override SymbologyBlock parsePSB()
        {
            return base.parsePSB(new RadialSymbologyBlock());
        }
    }
}
