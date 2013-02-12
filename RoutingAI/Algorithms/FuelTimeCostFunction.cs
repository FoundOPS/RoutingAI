using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.DataContracts;
using libWyvernzora;

namespace RoutingAI.Algorithms
{
    /// <summary>
    /// Cost function entirely based on fuel cost and worker time.
    /// Does not take overtime into account.
    /// Tasks completed too late are not penalized.
    /// </summary>
    public class FuelTimeCostFunction : ICostFunction
    {
        private const Double MILES_PER_METER = 0.000621371;
        private readonly IDistanceAlgorithm<Task> dist;
 
        /// <summary>
        /// Constructor.
        /// Creates a FuelTimeCostFunction from a distance algorithm.
        /// </summary>
        /// <param name="distAlg">Distance algorithm to use</param>
        public FuelTimeCostFunction(IDistanceAlgorithm<Task> distAlg)
        {
            dist = distAlg;
        }

        public Cost GetCost(Resource worker, Destination from, Destination to)
        {
            Cost cost = new Cost();

            cost.Distance = dist.GetDistance(from.Task, to.Task);
            cost.Time = (int)(to.EstimatedArrival - from.EstimatedArrival).TotalSeconds;
            cost.Overtime = 0;
            cost.OverallCost =
                (int) (worker.CostPerHour * (cost.Time / 60.0) + worker.CostPerMile * (cost.Distance * MILES_PER_METER));

            return cost;
        }


        public Cost GetCost(Resource worker, Destination task)
        {
            Cost cost = new Cost();

            cost.Distance = 0;
            cost.Time = (int) (task.EstimatedDeparture - task.EstimatedArrival).TotalSeconds;
            cost.Overtime = 0;
            cost.OverallCost = (int) (worker.CostPerHour * (cost.Time / 60.0));

            return cost;
        }
    }
}
