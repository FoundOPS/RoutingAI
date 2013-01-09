using RoutingAI.API.OSRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns>Distance between objects</returns>
        Int32 GetDistance(T start, T end);
    }
}
