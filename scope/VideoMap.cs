using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DGScope
{
    public class VideoMap
    {
        public int Number { get; set; }
        [JsonIgnore]
        [Browsable(false)]
        public int VertexBuffer { get; set; }
        public string Name { get; set; }
        public string Mnemonic { get; set; }
        [Browsable(false)]
        public bool Visible { get; set; }
        public MapCategory Category { get; set; } = MapCategory.A;
        public List<Line> Lines { get; set; } = new List<Line>();

        public override int GetHashCode()
        {
            return Number;
        }
        public override bool Equals(object obj)
        {
            VideoMap otherVideoMap = obj as VideoMap;
            if (otherVideoMap != null)
            {
                return Number.Equals(otherVideoMap.Number);
            }
            return false;
        }
        public override string ToString()
        {
            return Number + ": " + Name;
        }
    }

    public enum MapCategory
    { 
        A,
        B
    }

}
