using DGScope.Library;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DGScope.Receivers.Falcon
{
    internal class CDRFile
    {
        public List<Update> Updates { get; } = new List<Update>();
        public List<string> Sites { get; } = new List<string>();

        public DateTime StartOfData
        {
            get
            {
                return Updates.First().TimeStamp;
            }
        }
        public DateTime EndOfData
        {
            get
            {
                return Updates.Last().TimeStamp;
            }
        }

        public TimeSpan LengthOfData
        {
            get
            {
                return EndOfData - StartOfData;
            }
        }
        public CDRFile() { }
        public static CDRFile FromFile(string filepath)
        {
            CDRFile newFile = new CDRFile();
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (var zipstream = new GZipStream(stream, CompressionMode.Decompress, false))
                {
                    uint length = 0;
                    do
                    {
                        length = getLength(zipstream);
                        if (length > 0)
                        {
                            try
                            {
                                newFile.Updates.Add(getUpdate(zipstream, length));
                            }
                            catch { }
                        }
                    } while (length > 0);
                }
            }
            return newFile;
        }
        private static Update getUpdate (Stream stream, uint length)
        {
            var bytes = new byte[length];
            stream.Read(bytes, 0, (int)length);
            Update update = null;
            using (var ms = new MemoryStream(bytes))
            {
                update = Serializer.Deserialize<Update>(ms);
            }
            return update;
        }
        private static uint getLength (Stream stream)
        {
            bool lengthread = false;
            uint length = 0;
            for (int i = 0; !lengthread && i < 5; i++)
            {
                int data = stream.ReadByte();
                if (data < 0)
                {
                    return 0;
                }
                else if (data < 0x80)
                {
                    lengthread = true;
                }
                length += (uint)((data & 0x7f) << (i * 7));
            }
            return length;
        }
    }
}
