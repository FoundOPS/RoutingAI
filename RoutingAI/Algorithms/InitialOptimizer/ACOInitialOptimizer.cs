﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.DataContracts;

namespace RoutingAI.Algorithms.InitialOptimizer
{
    /// <summary>
    /// Initial Optimizer utilizing Ant Colony Optimization algorithm.
    /// For more details: http://en.wikipedia.org/wiki/Ant_colony_optimization
    /// Note: Requires n^2 space
    /// </summary>
    /// <remarks>
    /// Implementation of the ACO allows certain tasks to be left out when
    /// constraints cannot be satisfied. During further optimization,
    /// consider passing these "unvisited" tasks to another worker.
    /// </remarks>
    public class ACOInitialOptimizer : IInitialOptimizer
    {
        // Constants
        private const Int32 BASE_PHEROMON_DEPOSIT = 65535;  // Amount of pheromon to deposit that is later scaled according to edge length
                                                            // Optimal value would be average length of all edges

        // Persistent Values
        private Int32[,] pheromon;  // Buffer for pheromon values between two vertecies
        private IDistanceAlgorithm<Task> dist;  // Distance algorithm (distance buffering is up to distance algorithm)
        private Random rand = new Random(); // Random number generator

        private Task[] tasks;   // Tasks to optimize
        private Window window;  // Window of optimization. Start is very important!
        private Resource resource;  // Resource we are optimizing for

        // Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dist">Distance algorithm</param>
        public ACOInitialOptimizer(IDistanceAlgorithm<Task> dist)
        {
            this.dist = dist;
        }


        #region ACO Specific Methods

        /// <summary>
        /// Sends a new agent to tour the graph
        /// </summary>
        /// <param name="start">Start task; null by default</param>
        private void Iterate(Task start = null)
        {
            // Agent data
            Int32 distance = 0; // total distance
            Window optimized    // a window representing already routed part
                = new Window() { Start = window.Start, End = window.Start };
            SortedList<Int32, Task> unvisited    // List of unvisited nodes
                = new SortedList<int, Task>();
            List<Task> path     // Records the path of the agent
                = new List<Task>();

            // Copy all tasks to unvisited list
            foreach (Task t in tasks)
                unvisited.Add(t.Index, t);

            // Pick a random start unless start parameter is specified
            if (start == null) 
                start = unvisited.Values[rand.Next(unvisited.Count)];
            unvisited.Remove(start.Index);

            // Agent tours the graph
            while (true)    // Stopping condition will be determined at runtime (no more nodes to visit)
            {
                // Filter all unvisited nodes and get a list of pairs (pheromon, node)
                    // all tasks that cannot be visited should be dropped at this step
                    // CONSIDERATION: Use a constraint interface instead of hard code?? We may need to expand algorithm to process truck capacity as well...


                // If no nodes can be visited, break the loop

                // Generate a weighted random decision

                

            }
        }


        #endregion


        public Solution GenerateSolution(ClusteringSolution solution, int clusterIndex)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
