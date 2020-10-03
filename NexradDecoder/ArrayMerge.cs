using System;
using System.Collections.Generic;
using System.Linq;

namespace NexradDecoder
{
    public static class ArrayMerge
    {
        public static T[] Merge<T>(this T[] sourceArray, params T[][] additionalArrays)
        {
            List<T> elements = sourceArray.ToList();

            if (additionalArrays != null)
            {
                foreach (var array in additionalArrays)
                    elements.AddRange(array.ToList());
            }

            return elements.ToArray();
        }
    }
}
