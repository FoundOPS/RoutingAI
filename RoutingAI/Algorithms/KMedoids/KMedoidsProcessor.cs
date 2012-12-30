using RoutingAI.API.OSRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms.KMedoids
{
    /// <summary>
    /// A non-parallel implementation of K-Medoids Clustering Algorithm
    /// </summary>
    public class KMedoidsProcessor<T>
    {
        #region Fields

        private readonly Random _rand = new Random();
        private T[] _problemData;
        private T[] _centroids;
        private Dictionary<T, Int32> _clusterAssignment;
        private List<T>[] _meanTasks;
        private Int32 _totalDistance = Int32.MaxValue;

        private IDistanceAlgorithm<T> _distanceAlg;

        #endregion

        #region Properties

        /// <summary>
        /// Returns array of centroid Ts
        /// </summary>
        public T[] Centroids
        { get { return _centroids; } }
        /// <summary>
        /// Returns clustered Ts
        /// </summary>
        public List<T>[] Clusters
        { get { return _meanTasks; } }
        /// <summary>
        /// Returns total distance of all Ts to their centroids
        /// </summary>
        public Int32 TotalDistance
        { get { return _totalDistance; } }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clusterCount">Number of clusters to produce</param>
        /// <param name="problemData">Array of Ts to cluster</param>
        public KMedoidsProcessor(Int32 clusterCount, T[] problemData, IDistanceAlgorithm<T> alg)
        {
            // Check if there are enough tasks to fill clusters
            if (problemData.Length < clusterCount)
                clusterCount = problemData.Length;

            // Initialize stuff here
            this._distanceAlg = alg;
            this._centroids = new T[clusterCount];
            this._meanTasks = new List<T>[clusterCount];
            this._problemData = problemData;
            this._clusterAssignment = new Dictionary<T, int>();

            for (int i = 0; i < clusterCount; i++)
                _meanTasks[i] = new List<T>();

            // Pick random centroids from problem data
            for (int i = 0; i < _centroids.Length; i++)
            {
                T c = _problemData[_rand.Next(_problemData.Length)];
                if (!_centroids.Contains(c))
                    _centroids[i] = c;
                else
                    i--;
            }

            // Assign tasks to nearest centroid
            Iterate(false);

            Int32 oldDistance = _totalDistance;
            do
            {
                oldDistance = _totalDistance;
                Iterate();
            } while (_totalDistance < oldDistance);
        }
        /// <summary>
        /// Returns index of the cluster a T is assigned to
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Int32 GetClusterIndex(T c)
        {
            return _clusterAssignment[c];
        }

        /// <summary>
        /// Runs one iteration of K-Medoids Algorithm
        /// </summary>
        /// <param name="selectCentroids">Pass false if you want to use current centroids</param>
        public void Iterate(Boolean selectCentroids = true)
        {
            if (selectCentroids)
            {
                _totalDistance = 0;

                for (int i = 0; i < _meanTasks.Length; i++)
                {
                    List<T> ct = _meanTasks[i];

                    int minTotalDist = 0;
                    T centroid = FindCentroid(ct, out minTotalDist);

                    _totalDistance += minTotalDist;

                    ct.Clear();
                    _centroids[i] = centroid;
                }
            }

            for (int i = 0; i < _meanTasks.Length; i++)
                _meanTasks[i].Clear();

            foreach (T c in _problemData)
            {
                int centroidIndex = FindNearestCentroid(c);
                _clusterAssignment[c] = centroidIndex;
                _meanTasks[centroidIndex].Add(c);
            }
        }
        /// <summary>
        /// Finds nearest centroid T
        /// </summary>
        /// <param name="c">T to process</param>
        /// <returns>Index of centroid</returns>
        private Int32 FindNearestCentroid(T c)
        {
            Int32 minDistance = Int32.MaxValue;
            Int32 minIndex = -1;

            for (int i = 0; i < _centroids.Length; i++)
            {
                int distance = _distanceAlg.GetDistance(c, _centroids[i]);
                if (distance < minDistance)
                {
                    minIndex = i;
                    minDistance = distance;
                }
            }

            return minIndex;
        }
        /// <summary>
        /// Finds centroid T from a set of Ts
        /// </summary>
        /// <param name="coords">Collection of Ts to consider</param>
        /// <param name="distance">Distance of all Ts to their centroid</param>
        /// <returns></returns>
        private T FindCentroid(IEnumerable<T> coords, out Int32 distance)
        {
            int minDistance = Int32.MaxValue;
            T minC = default(T);

            foreach (T from in coords)
            {
                distance = 0;
                foreach (T to in coords)
                    distance += _distanceAlg.GetDistance(from, to);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minC = from;
                }
            }

            distance = minDistance;
            return minC;
        }

        #endregion
    }
}