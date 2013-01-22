using RoutingAI.API.OSRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libWyvernzora;

namespace RoutingAI.Algorithms
{
    /// <summary>
    /// Represents a function that computes distance between two objects
    /// </summary>
    /// <typeparam name="T">Type of objects the distance function operates on</typeparam>
    public interface IDistanceAlgorithm<T>
    {
        /// <summary>
        /// When implemented, computes distance between objects
        /// </summary>
        /// <param name="start">Origin object</param>
        /// <param name="end">Destination object</param>
        /// <returns>Distance between objects in meters</returns>
        Int32 GetDistance(T start, T end);

        /// <summary>
        /// When implemented, computes time between objects
        /// </summary>
        /// <param name="start">Origin Object</param>
        /// <param name="end">Destination Object</param>
        /// <returns>Time between objects in seconds</returns>
        Int32 GetTime(T start, T end);

        /// <summary>
        /// When implemented, computes distance and time between objects
        /// </summary>
        /// <param name="start">Origin Object</param>
        /// <param name="end">Destination Object</param>
        /// <returns>Pair(Distance, Time) between objects</returns>
        Pair<Int32, Int32> GetDistanceTime(T start, T end);
    }
}
