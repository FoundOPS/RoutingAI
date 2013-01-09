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
        /// </summary>
        /// <param name="worker">ResourceGroup that is going to perform tasks</param>
        /// <param name="from">Origin task</param>
        /// <param name="to">Destination task</param>
        /// <returns>Cost between tasks</returns>
        Cost GetCost(Resource worker, Task from, Task to);
    }
}
