using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoutingAI.Algorithms
{
    /// <summary>
    /// Represents a function that computes cost between two tasks
    /// </summary>
    public interface ICostFunction
    {
        /// <summary>
        /// When implemented, computes cost between two tasks
        /// Includes time spent on Origin destination but not time spent on destination destination
        /// </summary>
        /// <param name="worker">ResourceGroup that is going to perform tasks</param>
        /// <param name="from">Origin destination</param>
        /// <param name="to">Destination destination</param>
        /// <returns>Cost between destinations</returns>
        Cost GetCost(Resource worker, Destination from, Destination to);

        /// <summary>
        /// When implemented, computes cost of one single task without considering other tasks
        /// </summary>
        /// <param name="worker">ResourceGroup that is going to perform tasks</param>
        /// <param name="task">Destination</param>
        /// <returns>Cost of the destination</returns>
        Cost GetCost(Resource worker, Destination task);
    }

    /// <summary>
    /// Extensions for ICostFunction Interface
    /// </summary>
    public static class CostFunctionEx
    {
        /// <summary>
        /// Calculates the cost of a sequence of tasks
        /// </summary>
        /// <param name="costFunc">Cost function</param>
        /// <param name="worker">Worker the sequence is assigned to</param>
        /// <param name="path">Sequence of tasks</param>
        /// <returns>Cost of the path</returns>
        public static Cost GetPathCost(this ICostFunction costFunc, Resource worker, IList<Destination> path)
        {
            if (path.Count == 0)
                return new Cost(); // there is no cost for empty path

            Destination last = null;
            Cost cost = new Cost();

            
            foreach (Destination t in path)
            {
                // If this is not the first task, start computing cost
                if (last != null)
                    cost += costFunc.GetCost(worker, last, t);

                // Move on to the next task
                last = t;

                // Substract profit from costs
                cost.OverallCost -= (int)t.Task.Value;
            }

            // Last task was not taken into account, do it now
            cost += costFunc.GetCost(worker, last);

            return cost;
        }
    }
}
