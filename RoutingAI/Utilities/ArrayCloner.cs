using System;
using System.Collections.Generic;

namespace RoutingAI.Utilities
{
    public static class ArrayCloner
    {
        public static T[] Clone<T>(this T[] array) where T : ICloneable
        {
            var newArray = new T[array.Length];
            for (var i = 0; i < array.Length; i++)
                newArray[i] = (T)array[i].Clone();
            return newArray;
        }
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> items) where T : ICloneable
        {
            foreach (var item in items)
                yield return (T)item.Clone();
        }
    }
}
