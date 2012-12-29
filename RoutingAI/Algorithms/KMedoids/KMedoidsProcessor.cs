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
    public class KMedoidsProcessor
    {
        #region Fields

        private readonly Random _rand = new Random();
        private Coordinate[] _problemData;
        private Coordinate[] _centroids;
        private Dictionary<Coordinate, Int32> _clusterAssignment;
        private List<Coordinate>[] _meanTasks;
        private Int32 _totalDistance = Int32.MaxValue;

        private IDistanceAlgorithm _distanceAlg = new StraightDistanceAlgorithm();

        #endregion

        #region Properties

        /// <summary>
        /// Returns array of centroid coordinates
        /// </summary>
        public Coordinate[] Centroids
        { get { return _centroids; } }
        /// <summary>
        /// Returns clustered coordinates
        /// </summary>
        public List<Coordinate>[] Clusters
        { get { return _meanTasks; } }
        /// <summary>
        /// Returns total distance of all coordinates to their centroids
        /// </summary>
        public Int32 TotalDistance
        { get { return _totalDistance; } }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clusterCount">Number of clusters to produce</param>
        /// <param name="problemData">Array of coordinates to cluster</param>
        public KMedoidsProcessor(Int32 clusterCount, Coordinate[] problemData)
        {
            // Check if there are enough tasks to fill clusters
            if (problemData.Length < clusterCount)
                clusterCount = problemData.Length;

            // Initialize stuff here
            this._centroids = new Coordinate[clusterCount];
            this._meanTasks = new List<Coordinate>[clusterCount];
            this._problemData = problemData;

            for (int i = 0; i < clusterCount; i++)
                _meanTasks[i] = new List<Coordinate>();

            // Pick random centroids from problem data
            for (int i = 0; i < _centroids.Length; i++)
            {
                Coordinate c = _problemData[_rand.Next(_problemData.Length)];
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
                    List<Coordinate> ct = _meanTasks[i];

                    int minTotalDist = 0;
                    Coordinate centroid = FindCentroid(ct, out minTotalDist);

                    _totalDistance += minTotalDist;

                    ct.Clear();
                    _centroids[i] = centroid;
                }
            }

            for (int i = 0; i < _meanTasks.Length; i++)
                _meanTasks[i].Clear();

            foreach (Coordinate c in _problemData)
            {
                int centroidIndex = FindNearestCentroid(c);
                _clusterAssignment[c] = centroidIndex;
                _meanTasks[centroidIndex].Add(c);
            }
        }
        /// <summary>
        /// Finds nearest centroid coordinate
        /// </summary>
        /// <param name="c">Coordinate to process</param>
        /// <returns>Index of centroid</returns>
        private Int32 FindNearestCentroid(Coordinate c)
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
        /// Finds centroid coordinate from a set of coordinates
        /// </summary>
        /// <param name="coords">Collection of coordinates to consider</param>
        /// <param name="distance">Distance of all coordinates to their centroid</param>
        /// <returns></returns>
        private Coordinate FindCentroid(IEnumerable<Coordinate> coords, out Int32 distance)
        {
            int minDistance = Int32.MaxValue;
            Coordinate minC = null;

            foreach (Coordinate from in coords)
            {
                distance = 0;
                foreach (Coordinate to in coords)
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