using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Utilities
{
    public static class ArraySampler
    {
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

            if (sampleSize == array.Length) {
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
    }
}
