using System;
using RoutingAI.Algorithms;
using RoutingAI.Threading;
using RoutingAI.DataContracts;
using RoutingAI.Algorithms.AntColonyOptimizer;

namespace RoutingAI.Slave
{
    class AcoOptimizationTask : IComputationTask<Solution>
    {
        private Solution result;
        private Resource resource;
        private Task[] tasks;
        private OptimizationRequest request;

        public AcoOptimizationTask(OptimizationRequest request, Resource resource, Task[] tasks)
        {
            this.resource = resource;
            this.request = request;
            this.tasks = tasks;
        }

        public Solution Result
        {
            get { return result; }
        }

        public void Compute(params object[] args)
        {
            Window window = request.Window;

            AntColonyOptimizer aco = new AntColonyOptimizer(resource, window, new GeoStraightDistanceAlgorithm());
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
