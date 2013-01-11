using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RoutingAI.Utilities;

namespace RoutingAI.Algorithms.Clustering
{
    /// <summary>
    /// A concurrent implementation of CLARA clustering algorithm
    /// </summary>
    /// <typeparam name="T">Type of elements to cluster</typeparam>
    public class CLARAClusteringAlgorithm<T> : PAMClusteringAlgorithm<T>
    {
        /// <summary>
        /// Number of worker threads to spawn per processor core
        /// </summary>
        const Int32 WORKER_PER_PROCESSOR = 2;

        // Multithreading
        Thread[] _workers;

        // Parameters
        private Int32 _sampleSize;
        private Object mutex = new Object();   // mutex lock

        // Current best
        private volatile Int32 _bestDistance = Int32.MaxValue;

        /// <summary>
        /// Gets or sets number of sampling runs on the data set per worker
        /// </summary>
        public Int32 SamplingRuns
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items to cluster</param>
        /// <param name="clusterCount">Number of clusters to produce</param>
        /// <param name="samplingRuns">Number of sampling runs</param>
        /// <param name="alg">Distance calculation algorithm</param>
        /// <param name="maxIterations">Maximum number of iterations allowed</param>
        public CLARAClusteringAlgorithm(T[] items, Int32 clusterCount, Int32 samplingRuns, IDistanceAlgorithm<T> alg, Int32 maxIterations)
            : base(items, clusterCount, alg, maxIterations)
        {
            this.SamplingRuns = samplingRuns;
            this._sampleSize = (int)Math.Sqrt(items.Length);
            this._workers = new Thread[Environment.ProcessorCount * WORKER_PER_PROCESSOR];
        }
        public CLARAClusteringAlgorithm(T[] items, Int32 clusterCount, Int32 samplingRuns, IDistanceAlgorithm<T> alg)
            : this(items, clusterCount, samplingRuns, alg, Int32.MaxValue) { }

        /// <summary>
        /// Runs the algorithm
        /// </summary>
        public override void Run()
        {
            // Initialize workers
            for (int i = 0; i < _workers.Length; i++)
                _workers[i] = new Thread(new ThreadStart(RunWorker));
            // Start worker threads
            foreach (Thread t in _workers)
                t.Start();
            // Wait for workers to get done
            foreach (Thread t in _workers)
                t.Join();

            // Fix up centroid assignments
            _totalDistance = AssignItemsToCentroids(_data, _centroids, _clusters, _clusterAssignment, ref _dissimilarity);
        }

        #region Multithreading and Utilities

        private void RunWorker()
        {
            for (int i = 0; i < SamplingRuns; i++)
            {
                // Get random sample fron data
                T[] sample = _data.GetRandomSample(_sampleSize);

                // Initialize temporary data stores
                T[] centroids = new T[_centroids.Length];
                List<T>[] clusters = new List<T>[_centroids.Length];
                for (int k = 0; k < clusters.Length; k++)
                    clusters[k] = new List<T>();

                // Run sampling
                Double dissimilarity = Double.MaxValue;
                AssignRandomCentroids(sample, centroids, clusters, null);
                Int32 distance = RunPAM(sample, centroids, clusters, null, ref dissimilarity);

                // Report results
                SubmitSamplingResults(distance, dissimilarity, centroids);
            }
        }

        private void SubmitSamplingResults(Int32 distance, Double dissimilarity, T[] centroids)
        {
            lock (mutex)
            {
                if (distance <= _bestDistance)
                {
                    _bestDistance = distance;
                    _dissimilarity = dissimilarity;
                    Array.Copy(centroids, _centroids, centroids.Length);
                }
              
            }
        }

        #endregion
    }
}
