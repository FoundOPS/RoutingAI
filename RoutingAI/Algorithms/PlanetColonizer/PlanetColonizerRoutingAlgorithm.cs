using RoutingAI.Algorithms.PlanetColonizer.Interfaces;
using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoutingAI.Algorithms.PlanetColonizer
{
    public class PlanetColonizerRoutingAlgorithm : Population<TaskDistribution>
    {
        #region Fields

        // Configuration
        protected SlaveConfig configuration;
        
        // Worker thread and controls
        protected Thread workerThread;
        protected Boolean workerRunning = true;

        // Algorithm Data
        private String osrmSrv;

        private Boolean isInitialized = false;
        private Boolean isOptimizingSeq;
        private Int32 seqOptimizationIters;

        #endregion

        #region Properties

        /// <summary>
        /// Gets configuration of the routing algorithm instance
        /// </summary>
        public SlaveConfig Configuration
        { get { return configuration; } }
        /// <summary>
        /// Gets resources available to the routing algorithm
        /// </summary>
        public Resource[] Resources
        { get { return configuration.OptimizationRequest.Workers; } }
        /// <summary>
        /// Gets tasks for routing algorithm to schedule
        /// </summary>
        public Task[] Tasks
        { get { return configuration.OptimizationRequest.Tasks; } }
        /// <summary>
        /// Gets or sets cost function use by this algorithm
        /// </summary>
        public ICostFunction CostFunction { get; set; }

        /// <summary>
        /// Gets a boolean indicating whether the algorithm is
        /// done running
        /// </summary>
        protected bool IsComplete
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets the current solution produced by the algorithm, or null
        /// if there is no result available.
        /// NOTE: solution may not be optimal if algorithm is still running
        /// </summary>
        protected Solution Solution
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public PlanetColonizerRoutingAlgorithm(SlaveConfig config, ICostFunction costfunc)
        {
            this.configuration = config;
            this.CostFunction = costfunc;
        }

        public void StartAlgorithm()
        {
            workerThread = new Thread(new ThreadStart(RunAlgorithm));
            workerThread.Start();
        }

        protected void RunAlgorithm()
        {

        }


        #region Optimization

        public Int32 MutationForce
        {
            get
            {
                return (Int32)((InitialMutationRate - MutationRate) / (Double)InitialMutationRate * 10);
            }
        }

        public Int32 MaxMutations
        {
            get { return 0; }
        }

        #endregion


        protected override void RegeneratePopulation()
        {
            throw new NotImplementedException();
        }
    }
}
