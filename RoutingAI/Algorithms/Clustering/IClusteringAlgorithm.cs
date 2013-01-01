using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms.Clustering
{
    /// <summary>
    /// Represents a Clustering Algorithm
    /// </summary>
    /// <typeparam name="T">Type of items to cluster</typeparam>
    public interface IClusteringAlgorithm<T>
    {
        /// <summary>
        /// Gets an array of centroids calculated by the algorithm
        /// </summary>
        T[] Centroids { get; }
        /// <summary>
        /// Gets an array of clusters calculated by the algorithm
        /// </summary>
        List<T>[] Clusters { get; }
        /// <summary>
        /// Gets the sum of distance of all nodes with their centroids
        /// </summary>
        Int32 TotalDistance { get; }

        /// <summary>
        /// Runs the algorithm
        /// </summary>
        void Run();
        /// <summary>
        /// Gets index of centroid that the specified item is assigned to
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Index of centroid; -1 if item is not found</returns>
        Int32 GetClusterIndex(T item);
    }
}
