using System;
using RoutingAI.Algorithms;
using RoutingAI.Threading;
using RoutingAI.DataContracts;
using RoutingAI.Algorithms.AntColonyOptimizer;

namespace RoutingAI.Slave
{
    class AcoOptimizationTask : IComputationTask<Route>
    {
        private Route result;
        private Resource resource;
        private Task[] tasks;
        private OptimizationRequest request;

        public AcoOptimizationTask(OptimizationRequest request, Resource resource, Task[] tasks)
        {
            this.resource = resource;
            this.request = request;
            this.tasks = tasks;
        }

        public Route Result
        {
            get { return result; }
        }

        public void Compute(params object[] args)
        {
            Window window = request.Window;

            IDistanceAlgorithm<Task> distanceAlg = new GeoStraightDistanceAlgorithm();
            ICostFunction costAlg = new FuelTimeCostFunction(distanceAlg);

            AntColonyOptimizer aco = new AntColonyOptimizer(resource, window, distanceAlg, costAlg);
            result = aco.GenerateSolution(tasks);
        }

        public void HandleAbort()
        {
        }

        public object GetResult()
        {
            return result;
        }

        public void Dispose()
        {
        }
    }
}
