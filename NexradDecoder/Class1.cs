//
// Created by Chris Harrell, 09/10/2010
//
// Base NexradDecoder class.

using ICSharpCode.SharpZipLib.BZip2;
using System;
using System.IO;

namespace NexradDecoder
{
    public abstract class NexradDecoder
    {

        Stream fs;

        protected HeaderBlock msg_header_block;
        int msg_header_block_offset;

        protected DescriptionBlock description_block;
        int description_block_offset;

        protected SymbologyBlock symbology_block;
        int symbology_block_offset;

        int graphic_block_offset;

        ///////////////////////////////////////////// 
        /* This constructor is executed when the   */
        /* object is first created                 */
        ///////////////////////////////////////////// 
        protected NexradDecoder()
        {
            initializeVariables();                           // Initialize method variables
        }


        ///////////////////////////////////////////// 
        /* Initialize method variables             */
        ///////////////////////////////////////////// 
        void initializeVariables()
        {

            msg_header_block = new HeaderBlock();                     // Array to hold Message Header Block data.
            description_block = new DescriptionBlock();                     // Array to hold Product Description Block data.
                                                                            // Array to hold the Product Symbology Block data.

            msg_header_block_offset = 30;
            description_block_offset = 48;
        }

        ///////////////////////////////////////////// 
        /* Create file handle resource             */
        ///////////////////////////////////////////// 
        public void setFileResource(string fileName)
        {
            if (File.Exists(fileName))
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
        }

        public void setStreamResource(Stream stream)
        {
            fs = stream;
        }

        ///////////////////////////////////////////// 
        /* Read a byte (1 byte)              */
        ///////////////////////////////////////////// 
        protected int readByte(bool negativeRange = false)
        {
            byte[] bytes = new byte[1];
            fs.Read(bytes, 0, 1);
            return bytes[0];
        }

        ///////////////////////////////////////////// 
        /* Read a half word (2 bytes)              */
        ///////////////////////////////////////////// 
        protected int readHalfWord(bool negativeRange = false)
        {
            byte[] bytes = new byte[2];
            fs.Read(bytes, 0, 2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }


        ///////////////////////////////////////////// 
        /* Read a two halfwords (4 bytes)          */
        /////////////////////////////////////////////
        protected int readWord(bool negativeRange = false)
        {
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);


            int result = bytes[0] << 32;
            result += bytes[1] << 24;
            result += bytes[2] << 16;
            result += bytes[3];
            return result;
        }

        ///////////////////////////////////////////// 
        /* Read 4 bit RLE data                     */
        /////////////////////////////////////////////
        protected int[] parseRLE()
        {
            byte[] dataArray = new byte[1];
            fs.Read(dataArray, 0, 1);
            var data = dataArray[0].ToString("X").PadLeft(2, '0');
            var split_data = data.ToCharArray();

            int length = int.Parse(split_data[0].ToString(), System.Globalization.NumberStyles.HexNumber);
            int value = int.Parse(split_data[1].ToString(), System.Globalization.NumberStyles.HexNumber);
            int[] valueArray = new int[length];

            // Reduce the color values if the radar is in clean air mode and the current product is one of many Base Reflectivity products
            if (description_block.Mode == 1 && (description_block.Code >= 16 && description_block.Code <= 21))
            {
                if (value >= 8) value -= 8;
                else if (value < 8) value = 0;
            }

            for (int i = 1; i < length; i++)
            {
                valueArray[i] = value;
            }

            return valueArray;

        }


        /////////////////////////////////////////////
        /* Convert a binary value into decimal.    */
        /////////////////////////////////////////////	
        /*function str2dec(binaryString)
        {
            // Oddly enough bindec does not convert the binary data to decimal correctly.  The work 
            // around is to first convert the data to hex and then convert from hex to decimal.
            return hexdec(bin2hex(binaryString));
        }
        */
        /////////////////////////////////////////////
        /* Convert a decimal value into the decimal*/
        /* value of it's negative binary form.     */
        /* Save us all!!! There must be a better   */
        /* way!!!                                  */
        /////////////////////////////////////////////		
        ///
        /*function dec2negdec(value, bits)
        {
		binaryPadding = null;
		binaryValue = decbin(value);

            // decbin does not padd the resulting binary with 0's, which screws up my idea
            // to check the MSB for a negative flag.  Now I must append 0's if the binary
            // number requires it.
            if (strlen(binaryValue) < bits)
		{
			paddingBits = bits - strlen(binaryValue);
                for (i = 1;i <=paddingBits;i++)
			{
				binaryPadding.= '0';
                }
			binaryValue = binaryPadding. binaryValue;
            }

            // If the most significant bit is 1, then handle as a negative binary number
            if (binaryValue[0] == 1)
		{
			binaryValue = str_replace("0", "x", binaryValue);
			binaryValue = str_replace("1", "0", binaryValue);
			binaryValue = str_replace("x", "1", binaryValue);
			negDecimalValue = (bindec(binaryValue) + 1) * -1;

                return negDecimalValue;
            }

        // If the MSB is not 1, then return the original value
            else return value;
        }
        */

        /////////////////////////////////////////////
        /* Convert seconds to HH:MM:SS format      */
        /*                                         */
        /* Created by: jon@laughing-buddha.net.    */
        /////////////////////////////////////////////
        protected string sec2hms(int sec, bool padHours = false)
        {

            string hms = "";                                               // start with a blank string
            int hours = (int)(sec / 3600);

            hms += padHours ? hours.ToString().PadLeft(2, '0') : hours.ToString();

            int minutes = (int)((sec / 60) % 60);
            hms += ":" + minutes.ToString().PadLeft(2, '0');
            int seconds = (int)(sec % 60);
            hms += ":" + seconds.ToString().PadLeft(2, '0');

            return hms;

        }

        /////////////////////////////////////////////
        /* Parse the Graphic Alphanumeric Block    */
        /* pages into an array and return it.  To  */
        /* be called by the parseGAB() method.     */
        /////////////////////////////////////////////
        protected Page _parsePages()
        {
            Page page = new Page();
            page.Number = this.readHalfWord();
            page.Length = this.readHalfWord();
            int totalBytesToRead = page.Length;
            int vectorID = 0;
            int messageID = 0;

            while (totalBytesToRead > 0)
            {
                int packetCode = this.readHalfWord();
                int packetLength = this.readHalfWord();

                // If the packet code is 8 then decode it as a Text & Special Symbol Packet
                if (packetCode == 8)
                {
                    messageID++;
                    Message message = new Message();
                    message.TextColor = readHalfWord();
                    message.PosI = this.readHalfWord(true);
                    message.PosJ = this.readHalfWord(true);
                    message.MessageText = null;

                    // We have already 6 bytes of this packet.  Subtract it from the amount of 
                    // bytes thare still need to be read.
                    int packetBytesToRead = packetLength - 6;

                    // Read the remaining bytes (packetBytesToRead) to obtain the actual message
                    // that is encoded in the packet
                    for (int j = 0; j < packetBytesToRead; j++)
                    {
                        message.MessageText += (char)readByte();
                    }

                    // Subtract the total length of the packet (packetLength) from the total bytes
                    // in the page (totalBytesToRead).  We must account for the 4 bytes that were
                    // read while reading the packet code and packet length, because they are not included
                    // in the Packet Length.
                    totalBytesToRead -= (packetLength + 4);
                    page.Data.Messages.Add(message);
                }


                // If the packet code is 10 then decode it as a Unlinked Vector Packet
                else if (packetCode == 10)
                {
                    Vector vector = new Vector();
                    vector.Color = readHalfWord();

                    // We have already 2 bytes of this packet.  Subtract it from the amount of 
                    // bytes thare still need to be read.
                    int packetBytesToRead = packetLength - 2;

                    vectorID = 0;
                    while (packetBytesToRead > 0)
                    {
                        vectorID++;

                        vector.PosIBegin = readHalfWord(true);
                        vector.PosIBegin = readHalfWord(true);
                        vector.PosIEnd = this.readHalfWord(true);
                        vector.PosJEnd = this.readHalfWord(true);
                        page.Data.Vectors.Add(vector);
                        // Subtract the 8 bytes that we just read from the amount of packet bytes remaining 
                        // to be read (packetBytesToRead).
                        packetBytesToRead -= 8;
                    }

                    // Subtract the total length of the packet (packetLength) from the total bytes
                    // in the page (totalBytesToRead).  We must account for the 4 bytes that were
                    // read while reading the packet code and packet length, because they are not included
                    // in the Packet Length.
                    totalBytesToRead -= (packetLength + 4);
                }

            }
            return page;
        }

        /////////////////////////////////////////////
        /* Parse the Message Header Block into an  */
        /* array and return it.                    */
        /////////////////////////////////////////////
        public HeaderBlock parseMHB()
        {
            HeaderBlock msg_header_block = new HeaderBlock();
            fs.Seek(msg_header_block_offset, SeekOrigin.Begin);
            msg_header_block.Code = readHalfWord();                            // HW 1
            msg_header_block.DateTime = readTimeStamp(); // convertFromJulian((int)(readHalfWord() + 2440586.5)); // HW 2
            msg_header_block.Length = readWord();                              // HW 5 & HW 6
            msg_header_block.sourceID = readHalfWord();                        // HW 7
            msg_header_block.destinationID = readHalfWord();                   // HW 8
            msg_header_block.numberofBlocks = readHalfWord();                  // HW 9
            this.msg_header_block = msg_header_block;
            return msg_header_block;
        }
        static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime.AddDays(-1);
        }

        DateTime readTimeStamp()
        {
            DateTime dateTime = UnixTimeStampToDateTime(readHalfWord() * 86400);
            var seconds = readWord();
            dateTime = dateTime.AddSeconds(seconds);
            return dateTime;
        }
        private DateTime convertFromJulian(int jDate)
        {
            int day = jDate % 1000;
            int year = (jDate - day) / 1000;
            var date1 = new DateTime(year, 1, 1);
            return date1.AddDays(day - 1);
        }

        /////////////////////////////////////////////
        /* Parse the Product Description Block     */
        /* into an array and return it.            */
        /////////////////////////////////////////////
        public DescriptionBlock parsePDB()
        {
            DescriptionBlock description_block = new DescriptionBlock();
            fs.Seek(description_block_offset, SeekOrigin.Begin);

            description_block.Divider = readHalfWord(true);                                //HW 10	
            description_block.Latitude = (double)readWord() / 1000;                                //HW 11 - 12
            description_block.Longitude = (double)readWord() / 1000;                           //HW 13 - 14
            description_block.Height = readHalfWord(true);                                 //HW 15
            description_block.Code = readHalfWord(true);                                   //HW 16
            description_block.Mode = readHalfWord();                                       //HW 17
            description_block.VolumeCoveragePattern = readHalfWord();                      //HW 18
            description_block.SequenceNumber = readHalfWord();                             //HW 19

            description_block.ScanNumber = readHalfWord();                                 //HW 20
            description_block.ScanTime = readTimeStamp();                 //HW 22 - 23
            description_block.GenerationTime = readTimeStamp();           //HW 25 - 26
            description_block.ProductSpecific_1 = readHalfWord();                          //HW 27
            description_block.ProductSpecific_2 = readHalfWord();                          //HW 28
            description_block.ElevationNumber = readHalfWord();                            //HW 29

            description_block.ProductSpecific_3 = readHalfWord() / 10;                     //HW 30
            description_block.Threshold_1 = readHalfWord();                                //HW 31
            description_block.Threshold_2 = readHalfWord();                                //HW 32
            description_block.Threshold_3 = readHalfWord();                                //HW 33
            description_block.Threshold_4 = readHalfWord();                                //HW 34
            description_block.Threshold_5 = readHalfWord();                                //HW 35
            description_block.Threshold_6 = readHalfWord();                                //HW 36
            description_block.Threshold_7 = readHalfWord();                                //HW 37
            description_block.Threshold_8 = readHalfWord();                                //HW 38
            description_block.Threshold_9 = readHalfWord();                                //HW 39

            description_block.Threshold_10 = readHalfWord();                               //HW 40
            description_block.Threshold_11 = readHalfWord();                               //HW 41
            description_block.Threshold_12 = readHalfWord();                               //HW 42
            description_block.Threshold_13 = readHalfWord();                               //HW 43
            description_block.Threshold_14 = readHalfWord();                               //HW 44
            description_block.Threshold_15 = readHalfWord();                               //HW 45
            description_block.Threshold_16 = readHalfWord();                               //HW 46
            description_block.ProductSpecific_4 = readHalfWord();                          //HW 47
            description_block.ProductSpecific_5 = readHalfWord();                          //HW 48
            description_block.ProductSpecific_6 = readHalfWord();                          //HW 49

            description_block.ProductSpecific_7 = readHalfWord();                          //HW 50
            description_block.ProductSpecific_8 = readHalfWord();                          //HW 51
            description_block.ProductSpecific_9 = readHalfWord();                          //HW 52
            description_block.ProductSpecific_10 = readHalfWord();                         //HW 53
            description_block.Version = readByte();                                        //HW 54
            readByte();
            description_block.SymbologyOffset = readWord();                                //HW 55 - 56
            description_block.GraphicOffset = readWord();                                  //HW 57 - 58
            description_block.TabularOffset = readWord();                                  //HW 59 - 60
            this.description_block = description_block;
            return description_block;
        }


        /////////////////////////////////////////////
        /* Parse the Product Symbology Block into  */
        /* an array and return it.                 */
        /////////////////////////////////////////////
        protected SymbologyBlock parsePSB(SymbologyBlock symbology_block)
        {
            symbology_block_offset = (description_block.SymbologyOffset * 2) + msg_header_block_offset;
            fs.Seek(symbology_block_offset, SeekOrigin.Begin);
            if (description_block.ProductSpecific_8 == 1)
            {
                using (MemoryStream copy = new MemoryStream())
                {
                    fs.CopyTo(copy);
                    copy.Seek(0, SeekOrigin.Begin);
                    using (BZip2InputStream inputStream = new BZip2InputStream(copy))
                    {
                        var fslength = fs.Length;
                        fs.Seek(0, SeekOrigin.End);
                        inputStream.CopyTo(fs);
                        fs.Seek(fslength, SeekOrigin.Begin);

                    }
                }
            }
            symbology_block.Divider = readHalfWord();
            symbology_block.BlockID = readHalfWord();
            symbology_block.BlockLength = readWord();
            symbology_block.NumOfLayers = readHalfWord();

            for (int i = 1; i <= symbology_block.NumOfLayers; i++)
            {
                parseLayers();
            }

            return this.symbology_block;
        }
        public abstract SymbologyBlock parsePSB();
        public abstract void parseLayers();

        /////////////////////////////////////////////
        /* Parse the Graphic Alphanumeric Block    */
        /* into an array and return it.            */
        /////////////////////////////////////////////
        public GraphicBlock parseGAB()
        {
            GraphicBlock graphic_block = new GraphicBlock();
            graphic_block_offset = (description_block.GraphicOffset * 2) + msg_header_block_offset;

            fs.Seek(graphic_block_offset, SeekOrigin.Begin);
            graphic_block.Divider = readHalfWord(true);
            graphic_block.BlockID = readHalfWord();
            graphic_block.BlockLength = readWord();
            graphic_block.Pages = new Page[readHalfWord()];

            for (int i = 0; i < graphic_block.Pages.Length; i++)
            {
                graphic_block.Pages[i] = _parsePages();
            }

            return graphic_block;
        }

        public enum MessageCodes
        {
            BaseReflectivity
        }

    }

}
