using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoutingAI.Algorithms
{
    /// <summary>
    /// Abstract base class for all routing algorithms.
    /// </summary>
    public abstract class RoutingAlgorithmBase
    {
        // Configuration
        protected SlaveConfig configuration;

        // Problem Data
        protected Resource[] resources;
        protected Task[] tasks;



    }
}
