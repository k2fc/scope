namespace NexradDecoder
{
    class RasterPacketDecoder : NexradDecoder
    {
        public override void parseLayers()
        {
            RasterSymbologyBlock symbology_block = new RasterSymbologyBlock();
            symbology_block.LayerDivider = readHalfWord();
            symbology_block.LayerLength = readWord();
            symbology_block.LayerPacketCode = readHalfWord(); 
		    symbology_block.LayerPacketCode2 = readHalfWord();
		    symbology_block.LayerPacketCode3 = readHalfWord();	
		    symbology_block.I_Coord_Start = readHalfWord();
		    symbology_block.J_Coord_Start = readHalfWord();
		    symbology_block.X_Scale_Int = readHalfWord();
		    symbology_block.X_Scale_Fraction = readHalfWord();
		    symbology_block.Y_Scale_Int = readHalfWord();
		    symbology_block.Y_Scale_Fraction = readHalfWord();
		    symbology_block.NumRows = readHalfWord();
		    symbology_block.PackingDescriptor = readHalfWord();
            symbology_block.Rows = new Row[symbology_block.NumRows];
            for (int i = 0; i < symbology_block.NumRows; i++)
            {
                var rowBytes = readHalfWord();
                symbology_block.Rows[i] = new Row();
                symbology_block.Rows[i].Bytes = rowBytes;
                symbology_block.Rows[i].Data = new int[0];
                for (int j = 0; j < rowBytes; j++)
                {
                    var tempColorValues = parseRLE();
                    symbology_block.Rows[i].Data = ArrayMerge.Merge(symbology_block.Rows[i].Data,tempColorValues);
                }
            }
            base.symbology_block = symbology_block;
        }
        public override SymbologyBlock parsePSB()
        {
            return base.parsePSB(new RasterSymbologyBlock());
        }
    }
}
