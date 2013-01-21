using System;
using System.Collections.Generic;
using System.Linq;

namespace RoutingAI.Utilities
{
    public static class EnumerableEx
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

        /// <summary>
        /// Gets a random sample of the array
        /// </summary>
        /// <typeparam name="T">Type of array elements</typeparam>
        /// <param name="array">Array to sample</param>
        /// <param name="sampleSize">Number of elements in the sample array</param>
        /// <returns></returns>
        public static T[] GetRandomSample<T>(this T[] array, Int32 sampleSize)
        {
            if (sampleSize > array.Length)
                throw new ArgumentException("Sample cannot be larger that base array!");

            Random rand = new Random();
            Int32 selected = 0;
            T[] sample = new T[sampleSize];

            if (sampleSize == array.Length)
            {
                Array.Copy(array, sample, sampleSize);
                return sample;
            }

            for (int i = 0; i < array.Length && selected < sampleSize; i++)
            {
                Double prob = (double)sampleSize / (double)(array.Length - i);
                if ((rand.Next(1000000) / 1000000.0f) <= prob)
                    sample[selected++] = array[i];
            }

            return sample;
        }

        /// <summary>
        /// Picks a random element from a collection.
        /// Chances of an element being picked are weighted.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection</typeparam>
        /// <param name="source">IEnumerable to pick from</param>
        /// <param name="weight">Delegate to get the weight value from item</param>
        /// <returns>Picked item</returns>
        public static T PickWeighted<T>(this IEnumerable<T> source, Func<T, Double> weight)
        {
            List<T> srcCopy = source.ToList();  // Copy IEnumerable into a list to avoid bottlenecks
            return srcCopy.PickWeighted(weight);
        }

        /// <summary>
        /// Picks a random element from a list.
        /// Chances of an element being picked are weighted.
        /// </summary>
        /// <typeparam name="T">Type of elements in the list</typeparam>
        /// <param name="source">List to pick from</param>
        /// <param name="weight">Delegate to get the weight value from item</param>
        /// <returns>Picked item</returns>
        public static T PickWeighted<T>(this List<T> source, Func<T, Double> weight)
        {
            Random rand = new Random();

            // Check if there are elements
            if (source.Count == 0)
                return default(T);

            // Total weight. May be 0 or negative!
            Double totalWeight = source.Sum((T item) => weight(item));

            if (totalWeight > 0)    // In case if total weight is positive and therefore valid
            {                       // use default behavior to pick a random element
                while (true)
                {
                    Double r = rand.NextDouble() * totalWeight;

                    foreach (var item in source)
                    {
                        Double w = weight(item);

                        if (r < w)
                            return item;
                        r -= w;
                    }
                }
            }
            else
            {   // If total weight is negative or zero (and is therefore invalid)
                // Pick an element at random without considering weights
                Int32 r = rand.Next(source.Count);
                return source[r];
            }
        }

    }
}
