using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoutingAI.Algorithms.Interfaces
{
    /// <summary>
    /// Interface representing Initial Solution Generator
    /// </summary>
    public interface IInitialOptimizer : IDisposable
    {
        /// <summary>
        /// Generates an Initial Solution from a Clustering Solution
        /// </summary>
        /// <param name="tasks">Tasks to be optimized</param>
        /// <returns>Unoptimized Solution</returns>
        Route GenerateSolution(Task[] tasks);
    }
}
