using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingAI.Utilities;
using libWyvernzora;

namespace RoutingAI.Algorithms.KMedoids
{
    public class KMedoidsSampledProcessor<T>: KMedoidsProcessor<T>
    {
        private T[] _overallData;
        private List<Pair<Int32, T[]>> _centroidHistory;


        /// <summary>
        /// Size of sample to be processed each iteration
        /// </summary>
        public Int32 SampleSize
        { get; set; }
        public Int32 SamplingRuns { get; set; }


        public KMedoidsSampledProcessor(Int32 clusterCount, T[] problemData, Int32 sampleSize, Int32 samplingIterations, IDistanceAlgorithm<T> alg, Int32 maxIterations = Int32.MaxValue)
            : base(clusterCount, problemData, alg, maxIterations)
        {
            this._overallData = problemData;
            this.SampleSize = sampleSize;
            this.SamplingRuns = samplingIterations;
            this._centroidHistory = new List<Pair<Int32, T[]>>();
        }

        public override void Run()
        {
            AssignRandomCentroids();

            // Do sampling here
            for (int i = 0; i < SamplingRuns; i++)
            {
                // Sample data
                this._problemData = _overallData.GetRandomSample(SampleSize);
                // Run KMedoids on sample
                RunKMedois();
                // Record sampling results
                T[] centroids = new T[_centroids.Length];
                Array.Copy(_centroids, centroids, centroids.Length);
                _centroidHistory.Add(new Pair<Int32, T[]>(_totalDistance, _centroids));
            }

            // Summarize Samples
            Pair<Int32, T[]> result = _centroidHistory.OrderBy((Pair<Int32, T[]> p) => { return p.First; }).First();

            this._centroids = result.Second;
            this._totalDistance = result.First;

            // Fix up assignment
            _problemData = _overallData;
            Iterate(false);
        }


    }
}
