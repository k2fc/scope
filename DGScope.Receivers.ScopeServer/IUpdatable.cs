using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DGScope.Library
{
    public interface IUpdatable
    {
        DateTime LastMessageTime { get; }
        Guid Guid { get; }
        Update GetCompleteUpdate();
        Dictionary<PropertyInfo, DateTime> PropertyUpdatedTimes { get;}
        event EventHandler<UpdateEventArgs> Updated;
        event EventHandler<UpdateEventArgs> Created;
    }
}
