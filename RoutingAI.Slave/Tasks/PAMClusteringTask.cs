using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.Threading;
using RoutingAI.DataContracts;
using RoutingAI.Algorithms;
using RoutingAI.Algorithms.Clustering;
using System.Threading;

namespace RoutingAI.Slave
{
    /// <summary>
    /// Task for running PAM Clustering Algorithm on a data set using ComputationThread model.
    /// </summary>
    public class PAMClusteringTask : IComputationTask<ClusteringSolution>
    {
        private ClusteringSolution _solution = null;    // Clustering solution
        private SlaveConfig _config;    // Slave Configuration
        private OptimizationRequest _request;   // Data to be clustered

        /// <summary>
        /// Gets the result of the computation.
        /// Returns null is computation is not complete.
        /// </summary>
        public ClusteringSolution Result
        {
            get { return _solution; }
        }

        
        // Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">SlaveCongig object received from Controller server.</param>
        /// <param name="request">OptimizationRequest object received from Controller server.</param>
        public PAMClusteringTask(SlaveConfig config, OptimizationRequest request)
        {
            _config = config;
            _request = request;
        }


        // Methods
        /// <summary>
        /// Runs computation
        /// </summary>
        /// <param name="args">Arguments used for computation</param>
        public void Compute(params object[] args)
        {
            // Create an instance of PAM CLustering Algorithm and run it
            PAMClusteringAlgorithm<Task> clusteringAlg = new PAMClusteringAlgorithm<Task>(_request.Tasks, _request.Resources.Length, new GeoStraightDistanceAlgorithm());
            clusteringAlg.Run();

            // Initialize solution and copy clusters to the solution
            _solution = new ClusteringSolution();
            _solution.Clusters = new Task[clusteringAlg.Clusters.Length][];
            for (int i = 0; i < _solution.Clusters.Length; i++)
                _solution.Clusters[i] = clusteringAlg.Clusters[i].ToArray();
            _solution.Distance = clusteringAlg.TotalDistance;
        }

        /// <summary>
        /// Gracefully handles abort requests
        /// </summary>
        public void HandleAbort()
        {
            // do nothing
        }

        /// <summary>
        /// Returns result of the computation
        /// </summary>
        /// <returns>Result of the computation; null if computation is incomplete</returns>
        public object GetResult()
        {
            return _solution;
        }

        /// <summary>
        /// Releases all resources used by this computation task
        /// </summary>
        public void Dispose()
        {
            // do nothing
        }
    }
}
