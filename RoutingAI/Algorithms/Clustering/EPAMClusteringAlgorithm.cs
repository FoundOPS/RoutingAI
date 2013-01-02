using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.Utilities;

namespace RoutingAI.Algorithms.Clustering
{
    public class EPAMClusteringAlgorithm<T> : IClusteringAlgorithm<T>
    {
        // Random Number Generator & Distance Algorithm
        protected readonly Random _rand = new Random();
        protected readonly IDistanceAlgorithm<T> _dist;

        // Problem-Specific Data & Parameters
        protected T[] _data;
        protected Int32 _maxIterations;

        // Results
        protected T[] _centroids;
        protected List<T>[] _clusters;
        protected Int32 _totalDistance;
        protected Dictionary<T, Int32> _clusterAssignment;

        /// <summary>
        /// Constructor
        /// Creates a iteration-restricted instance of PAM algorithm
        /// </summary>
        /// <param name="data">Data to be clustered</param>
        /// <param name="clusterCount">Number of clusters to produce</param>
        /// <param name="alg">Distance calculation algorithm</param>
        /// <param name="maxIterations">Maximum number of iterations allowed</param>
        public EPAMClusteringAlgorithm(T[] data, Int32 clusterCount, IDistanceAlgorithm<T> alg, Int32 maxIterations)
        {
            this._data = data;
            this._centroids = new T[clusterCount];
            this._dist = alg;
            this._maxIterations = maxIterations;

            this._clusters = new List<T>[clusterCount];
            this._clusterAssignment = new Dictionary<T, int>();

            for (int i = 0; i < _clusters.Length; i++) _clusters[i] = new List<T>();
        }
        /// <summary>
        /// Constructor
        /// Creates an unrestricted instance of PAM algorithm
        /// </summary>
        /// <param name="data">Data to be clustered</param>
        /// <param name="clusterCount">Number of clusters to produce</param>
        /// <param name="alg">Distance calculation algorithm</param>
        public EPAMClusteringAlgorithm(T[] data, Int32 clusterCount, IDistanceAlgorithm<T> alg)
            : this(data, clusterCount, alg, Int32.MaxValue) { }

        #region IClusteringAlgorithm Members

        /// <summary>
        /// Returns an array of centroid items
        /// </summary>
        public T[] Centroids
        {
            get { return _centroids; }
        }
        /// <summary>
        /// Returns clustered data
        /// </summary>
        public List<T>[] Clusters
        {
            get { return _clusters; }
        }
        /// <summary>
        /// Returns sum of distances from each item to their respective centroids
        /// </summary>
        public int TotalDistance
        {
            get { return _totalDistance; }
        }

        /// <summary>
        /// Runs the algorithm
        /// </summary>
        public virtual void Run()
        {
            Int32[] accelBuffer = new Int32[_data.Length];
            for (int i = 0; i < accelBuffer.Length; i++) accelBuffer[i] = Int32.MaxValue;

                _totalDistance = AssignRandomCentroids(_data, _centroids, _clusters, _clusterAssignment, accelBuffer);
            _totalDistance = RunEPAM(_data, _centroids, _clusters, _clusterAssignment, accelBuffer);
        }
        /// <summary>
        /// Returns index of cluster an item is assigned to
        /// </summary>
        /// <param name="item">Item to search</param>
        /// <returns></returns>
        public int GetClusterIndex(T item)
        {
            if (!_clusterAssignment.ContainsKey(item))
                return -1;
            return _clusterAssignment[item];
        }

        #endregion

        #region Utility Methods
        /* NOTE:
         * Methods here must be thread safe!
         */
        /// <summary>
        /// Runs PAM
        /// This method can be run from multiple threads
        /// </summary>
        /// <param name="items">Items to cluster</param>
        /// <param name="centroids">Array of centroid items for storing results</param>
        /// <param name="clusters">Cluster data table for storing results</param>
        /// <param name="assignment">Cluster assignment map for storing results</param>
        /// <returns>Total distance for resulting clustering solution</returns>
        protected Int32 RunEPAM(T[] items, T[] centroids, List<T>[] clusters, Dictionary<T, Int32> assignment, Int32[] accelBuffer)
        {
            Int32 oldDistance = Int32.MaxValue;
            Int32 totalDistance = 0;
            Int32 iterations = 0;
            do
            {
                oldDistance = totalDistance;

                SelectCentroids(centroids, clusters);
                totalDistance = AssignItemsToCentroids(items, centroids, clusters, assignment, accelBuffer);

                iterations++;
            } while (totalDistance < oldDistance && iterations < _maxIterations);

            return totalDistance;
        }
        /// <summary>
        /// Finds nearest item from the centroids array
        /// </summary>
        /// <param name="centroids">Array of items to search</param>
        /// <param name="current">Current origin item</param>
        /// <returns>Index of the nearest node</returns>
        protected Int32 FindNearest(T[] centroids, T current, out Int32 minDistance)
        {
            minDistance = Int32.MaxValue;
            Int32 minIndex = -1;

            for (int i = 0; i < centroids.Length; i++)
            {
#if DEBUG
                if (centroids[i] == null || current == null)
                    System.Diagnostics.Debugger.Break();
#endif
                int distance = _dist.GetDistance(current, centroids[i]);
                if (distance < minDistance)
                { minIndex = i; minDistance = distance; }
            }

            return minIndex;
        }
        /// <summary>
        /// Determines the centroid item from a collection
        /// </summary>
        /// <param name="currentCentroid">Current centroid item to be replaced</param>
        /// <param name="items">Collection of items to process</param>
        /// <returns></returns>
        protected virtual T FindCentroid(T currentCentroid, IEnumerable<T> items)
        {
            Int32 minDistance = Int32.MaxValue;
            T minItem = currentCentroid;

            foreach (T from in items)
            {
                Int32 distance = 0;
                foreach (T to in items)
                    distance += _dist.GetDistance(from, to);
                if (distance < minDistance)
                { minDistance = distance; minItem = from; }
            }

            return minItem;
        }
        /// <summary>
        /// Selects centroids based on current clusters
        /// </summary>
        /// <param name="centroids">Array of current centroid items, this methods overwrites it</param>
        /// <param name="clusters">Array of current cluster lists, this method overwrites it</param>
        protected void SelectCentroids(T[] centroids, List<T>[] clusters)
        {
            for (int i = 0; i < clusters.Length; i++)
            {
                centroids[i] = FindCentroid(centroids[i], clusters[i]);
                clusters[i].Clear();
            }
        }
        /// <summary>
        /// Picks random centroids and assigns items to them
        /// </summary>
        /// <param name="items">Items to be assigned</param>
        /// <param name="centroids">
        /// Array of centroid items, this method overwrites it.
        /// NOTE: MUST be initialized, even if empty!
        /// </param>
        /// <param name="clusters">Cluster data, this method overwrites it</param>
        /// <param name="assignment">Cluster assignment data, this method overwrites it</param>
        /// <returns>Sum of distances for this cluster assignment</returns>
        protected Int32 AssignRandomCentroids(T[] items, T[] centroids, List<T>[] clusters, Dictionary<T, Int32> assignment, Int32[] accelBuffer)
        {
            // Pick random centroids
            T[] tmp = items.GetRandomSample(centroids.Length);
            Array.Copy(tmp, centroids, centroids.Length);

            // Assign Items
            return AssignItemsToCentroids(items, centroids, clusters, assignment, accelBuffer);
        }
        /// <summary>
        /// Assigns every item to the closest centroid
        /// </summary>
        /// <param name="items">Items to be assigned</param>
        /// <param name="centroids">Array of centroid items</param>
        /// <param name="clusters">Cluster data, this method overwrites it</param>
        /// <param name="assignment">Cluster assignment data, this method overwrites it</param>
        /// <param name="accelBuffer">Cluster assignment buffer used for acceleration</param>
        /// <returns>Sum of distances for this cluster assignment</returns>
        protected Int32 AssignItemsToCentroids(T[] items, T[] centroids, List<T>[] clusters, Dictionary<T, Int32> assignment, Int32[] accelBuffer)
        {
            // Clear current cluster data
            for (int i = 0; i < clusters.Length; i++) clusters[i].Clear();

            Int32 totalDistance = 0;
            for (int i = 0; i < items.Length; i++)
            {
                T item = items[i];

                if (assignment.ContainsKey(item) && _dist.GetDistance(item, centroids[assignment[item]]) <= accelBuffer[i])
                {
                    totalDistance += accelBuffer[i];
                    clusters[assignment[item]].Add(item);
                }
                else
                {
                    Int32 distance;
                    Int32 centroidIndex = FindNearest(centroids, item, out distance);
                    accelBuffer[i] = distance;

                    totalDistance += distance;
                    if (assignment != null) assignment[item] = centroidIndex;
                    clusters[centroidIndex].Add(item);
                }

            }

            return totalDistance;
        }

        #endregion
    }
}
