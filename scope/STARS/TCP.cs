using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.STARS
{
    [Serializable()]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TCP
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public GeoPoint HomeLocation { get; set; }
        public int[] DCBMapList { get; set; } = new int[36];
        public override string ToString()
        {
            return Symbol +"("+ Name + ")";
        }
    }
}
