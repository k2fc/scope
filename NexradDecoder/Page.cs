using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexradDecoder
{
    public class Page
    {
        public int Number { get; set; }
        public int Length { get; set; }
        public PageData Data { get; set; } = new PageData();
    }
    public class PageData
    {
        public List<Message> Messages { get; set; } = new List<Message>();
        public List<Vector> Vectors { get; set; } = new List<Vector>();
    }

    public class Vector
    {
        public int PosIBegin { get; set; }
        public int PosJBegin { get; set; }
        public int PosIEnd { get; set; }
        public int PosJEnd { get; set; }
        public int Color { get; set; }
    }
    public class Message
    {
        public int MessageID { get; set; }
        public int TextColor { get; set; }
        public int PosI { get; set; }
        public int PosJ { get; set; }
        public string MessageText { get; set; }
    }
}
