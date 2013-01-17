using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms.InitialOptimizer
{
    /// <summary>
    /// Interface representing Initial Solution Generator
    /// </summary>
    public interface IInitialOptimizer : IDisposable
    {
        /// <summary>
        /// Generates an Initial Solution from a Clustering Solution
        /// </summary>
        /// <param name="solution">Solution produced by the clustering algorithm</param>
        /// <returns>Unoptimized Solution</returns>
        Solution GenerateSolution(ClusteringSolution solution, Int32 clusterIndex);
    }
}
