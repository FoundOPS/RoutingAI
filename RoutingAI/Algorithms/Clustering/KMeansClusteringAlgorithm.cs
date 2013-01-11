using System;
using System.Collections.Generic;
using System.Linq;

namespace RoutingAI.Algorithms.Clustering
{
    /// <summary>
    /// Implemenation of K-Means++
    /// http://en.wikipedia.org/wiki/K-means%2B%2B
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class KMeansClusteringAlgorithm<T> : IClusteringAlgorithm<T>
    {
        // Random Number Generator & Distance Algorithm
        protected readonly Random _rand = new Random();
        protected readonly IDistanceAlgorithm<T> _dist;

        // Problem-Specific Data & Parameters
        protected T[] _data;
        protected bool _converged = false;
        protected Int32 _iterations;
        protected Int32 _maxIterations;

        // Results
        protected T[] _centroids;
        protected List<T>[] _clusters;
        protected Int32 _totalDistance;
        protected Dictionary<T, Int32> _clusterAssignment;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Data to be clustered</param>
        /// <param name="clusterCount">Number of clusters to produce</param>
        /// <param name="alg">Distance calculation algorithm</param>
        /// <param name="maxIterations">Maximum number of iterations allowed</param>
        public KMeansClusteringAlgorithm(T[] data, Int32 clusterCount, IDistanceAlgorithm<T> alg, Int32 maxIterations)
        {
            this._data = data;
            _maxIterations = maxIterations;

            this._centroids = new T[clusterCount];
            this._dist = alg;

            this._clusters = new List<T>[clusterCount];
            this._clusterAssignment = new Dictionary<T, int>();

            for (int i = 0; i < _clusters.Length; i++) _clusters[i] = new List<T>();
        }

        /// <summary>
        /// Constructor
        /// Creates an unrestricted instance of KMeans++ algorithm
        /// </summary>
        /// <param name="data">Data to be clustered</param>
        /// <param name="clusterCount">Number of clusters to produce</param>
        /// <param name="alg">Distance calculation algorithm</param>
        public KMeansClusteringAlgorithm(T[] data, Int32 clusterCount, IDistanceAlgorithm<T> alg)
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
        public Int32 TotalDistance
        {
            get { return _totalDistance; }
        }

        /// <summary>
        /// Get the distances for each point to the centroid
        /// </summary>
        /// <param name="centroid">The centroid</param>
        /// <returns>Total distance for each point to the centroid</returns>
        private Int32[] SquaredDistances(T centroid)
        {
            return _data.Select(t => (int)Math.Pow(_dist.GetDistance(t, centroid), 2)).ToArray();
        }

        /// <summary>
        /// Runs the algorithm
        /// </summary>
        public virtual void Run()
        {
            InitializeCentroids();

            //iterate until converged and while under max iterations
            while (!this._converged && this._iterations < this._maxIterations)
            {
                Iterate();
            }
        }

        private Int32 Sum(Int32[] array)
        {
            Int32 sum = 0;
            for (int i = 0; i < array.Length; i++)
                sum += array[i];
            return sum;
        }

        /// <summary>
        /// Run the K++ initialiazation
        /// </summary>
        private void InitializeCentroids()
        {
            //1) sample a point uniformly at random from the data points
            Centroids[0] = this._data[_rand.Next(this._data.Length)];

            //2) calculate the distances to the initial point
            Int32[] distances = SquaredDistances(Centroids[0]);

            //the distances from every point to the initial point
            Int32 distancesSum = Sum(distances);

            //3) choose a new data point at random as a new center, using a
            //weighted probability distribution where a point x is chosen with
            //probability proportional to D(x)2
            //(Repeated until k centers have been chosen.)

            //determine the amount of tries
            var nTries = 2 + Math.Round(Math.Log(_clusters.Length));

            //intialize each cluster
            for (var k = 1; k < _clusters.Length; ++k)
            {
                var smallestDistanceSum = -1;
                var smallestCentroidIndex = -1;

                //try to find a point Log(k) times
                for (var i = 0; i < nTries; ++i)
                {
                    //start the stop condition = sum of the distances to the initial point * random number between (0,1)
                    var stop = _rand.NextDouble() * distancesSum;

                    //go through each point and decrement the stop variable until the
                    //current points distance to the initial centroid is less than the stop condition
                    //then stop and choose a sample
                    int sampleIndex;
                    for (sampleIndex = 0; sampleIndex < this._data.Length; ++sampleIndex)
                    {
                        if (stop <= distances[sampleIndex])
                            break;

                        stop -= distances[sampleIndex];
                    }

                    //go through each point and check if it is closer to the sample point than the initial point
                    var sampleDistances = SquaredDistances(_data[sampleIndex]);
                    for (var p = 0; p < this._data.Length; ++p)
                    {
                        var distanceToInitialPoint = distances[p];
                        var distanceToSamplePoint = (int)Math.Pow(_dist.GetDistance(this._data[p], this._data[sampleIndex]), 2);
                        sampleDistances[p] = distanceToInitialPoint > distanceToSamplePoint ? distanceToSamplePoint : distanceToInitialPoint;
                    }

                    //update the best centroid if
                    //a) one hasn't been set yet
                    //b) the sample distances sum is < initial centroid point
                    var sampleDistancesSum = Sum(sampleDistances);
                    if (smallestDistanceSum < 0 || sampleDistancesSum < smallestDistanceSum)
                    {
                        smallestDistanceSum = sampleDistancesSum;
                        smallestCentroidIndex = sampleIndex;
                    }
                }

                distancesSum = smallestDistanceSum;

                var newCentroid = _data[smallestCentroidIndex];
                Centroids[k] = newCentroid;

                //
                for (var i = 0; i < this._data.Length; ++i)
                {
                    var centroidDistancesSum = distances[i];
                    var distanceBetweenNewOldCentroids = Math.Pow(_dist.GetDistance(newCentroid, this._data[i]), 2);
                    distances[i] = centroidDistancesSum > distanceBetweenNewOldCentroids ? (int)distanceBetweenNewOldCentroids : centroidDistancesSum;
                }
            }

            _totalDistance = Sum(distances);
        }

        //Iterate to converge to a local minimum
        private void Iterate()
        {
            if (this._converged)
                return;

            this._converged = true;
            this._iterations++;

            var distances = new Int32[_clusters.Length];

            for (var i = 0; i < _clusters.Length; i++)
                distances[i] = 0;

            //find the closest centroid for each point
            foreach (var item in _data)
            {
                var shortestCentroidDistance = Int32.MaxValue;
                var closestCentroidIndex = 0;
                var centroidIndex = -1;

                foreach (var c in _centroids)
                {
                    centroidIndex++;
                    var distance = _dist.GetDistance(item, c);
                    if (distance >= shortestCentroidDistance) continue;

                    shortestCentroidDistance = distance;
                    closestCentroidIndex = centroidIndex;
                }

                var currentCentroidIndex = GetClusterIndex(item);

                //if the item is not attached to a centroid or was
                //assigned to a different centroid before, the result differs from the previous iteration
                if (currentCentroidIndex == -1 || closestCentroidIndex != currentCentroidIndex)
                    this._converged = false;

                //assign the item to the centroid
                _clusterAssignment[item] = closestCentroidIndex;

                //sum the distance
                distances[closestCentroidIndex] += shortestCentroidDistance;
            }

            _totalDistance = Sum(distances);
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
    }
}
