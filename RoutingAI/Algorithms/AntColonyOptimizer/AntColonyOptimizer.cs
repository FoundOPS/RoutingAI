using System;
using System.Collections.Generic;
using System.Linq;
using RoutingAI.DataContracts;
using RoutingAI.Algorithms.Interfaces;
using RoutingAI.Utilities;

namespace RoutingAI.Algorithms.AntColonyOptimizer
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
    public class AntColonyOptimizer : IInitialOptimizer
    {

        // Persistent Values
        private readonly IDistanceAlgorithm<Task> dist;  // Distance algorithm (distance buffering is up to distance algorithm)
        private readonly Random rand = new Random(); // Random number generator
        private readonly Task[] tasks;   // Tasks to optimize
        private readonly Window window;  // Window of optimization. Start is very important!
        private readonly Resource resource;  // Resource we are optimizing for

        private Double[,] pheromon;  // Buffer for pheromon values between two vertecies
        private Int32 minDistance;  // Known minimum distance between nodes

        private Int32 bestCost = Int32.MaxValue;    // Cost of the best solution
        private Task[] bestSolution = null;     // Best Solution


        // Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dist">Distance algorithm</param>
        /// <param name="res">Resource that will own the tasks</param>
        /// <param name="wind">Window that optimization is running for</param>
        /// <param name="tasks">Array of Task objects that are going to be optimized</param>
        public AntColonyOptimizer(Resource res, Window wind, Task[] tasks, IDistanceAlgorithm<Task> dist)
        {
            resource = res;
            window = wind;
            this.tasks = tasks;
            this.dist = dist;

            pheromon = new double[tasks.Length, tasks.Length];

            // Initialize Internal Index
            for (int i = 0; i < tasks.Length; i++)
                this.tasks[i].Index = i;
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
                ConstraintFilterBase filter = new DummyConstraintFilter(resource, optimized, unvisited.Values, path);
                List<Task> candidates = filter.ToList();    // Force-fetch all values and avoid multiple Linq queries

                // If no nodes can be visited, break the loop
                if (candidates.Count == 0)
                    break;      // Iteration stops here when there are no more nodes to be considered

                // Generate a weighted random decision
                Task current = path[path.Count - 1];
                Task next = candidates.PickWeighted((Task t) => pheromon[current.Index, t.Index]);

                // Update agent data
                Int32 d = dist.GetDistance(current, next);
                distance += d;
                if (d < minDistance) minDistance = d;
                // TODO Increase Optimized Window to include next task
                unvisited.Remove(next.Index);
                path.Add(next);

                // Deposit pheromon
                    // TODO consider time in pheromon deposit as well (?)
                pheromon[current.Index, next.Index] += (double) minDistance / (double) d;
            }

            // At this point path is complete, update current values
                // TODO convert from distance to cost
                // for now just using  distance
            if (distance < bestCost)
            {
                bestCost = distance;
                bestSolution = path.ToArray();
            }
        }

        #endregion


        public Solution GenerateSolution(ClusteringSolution solution, int clusterIndex)
        {
            // TODO this implementation is only for debugging! make it better
                // Note: this implementation should work (in theory), but it can be improved
            for (int i = 0; i < 10000; i++)
            {
                Iterate();
            }

            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
