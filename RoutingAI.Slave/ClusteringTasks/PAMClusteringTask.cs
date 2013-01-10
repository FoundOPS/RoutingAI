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
    public class PAMClusteringTask : IComputationTask<ClusteringSolution>
    {
        ClusteringSolution _solution = null;

        public ClusteringSolution Result
        {
            get { return _solution; }
        }

        public void Compute(params object[] args)
        {
            // Convert arguments
            SlaveConfig cfg = (SlaveConfig)args[0];
            OptimizationRequest data = (OptimizationRequest)args[1];

            // Dummy Pause
            //Thread.Sleep(60000); // 1 min


            if (cfg.SlaveIndex.First == 0)
            {
                PAMClusteringAlgorithm<Task> clusteringAlg = new PAMClusteringAlgorithm<Task>(data.Tasks, data.Resources.Length, new GeoStraightDistanceAlgorithm());
                clusteringAlg.Run();

                _solution = new ClusteringSolution();
                _solution.Clusters = new Task[clusteringAlg.Clusters.Length][];
                for (int i = 0; i < _solution.Clusters.Length; i++ )
                    _solution.Clusters[i] = clusteringAlg.Clusters[i].ToArray();
                _solution.Distance = clusteringAlg.TotalDistance;
            }
           
        }

        public void HandleAbort()
        {
            // do nothing
        }

        public object GetResult()
        {
            return _solution;
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}
