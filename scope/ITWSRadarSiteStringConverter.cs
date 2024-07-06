using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGScope
{
    internal class ITWSSensorIDStringConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var w = context.Instance as NexradDisplay;
            return new StandardValuesCollection(w.Sensors);
        }
    }
}
