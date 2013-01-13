using RoutingAI.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.DataContracts;
using RoutingAI.ServiceContracts;
using System.Net;
using libWyvernzora;
using System.Threading;
using libWyvernzora.Logging;

namespace RoutingAI.Controller
{
    /// <summary>
    /// Planet Colonizer Routing Task
    /// </summary>
    public sealed class PCRoutingTask : RoutingTaskBase
    {
        public PCRoutingTask(OptimizationRequest request)
            : base(request)
        {  }

        public override void Compute(params object[] args)
        {
            // Allocate server resources for this task
            if (!AllocateServerResources())
            {
                // Allocation failed... can't run the task
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Expected, "Compute: Could not allocate server resources: {{{0}}}", _request.Id);
                throw new Exception("Failed to allocate server resources. See logs for more details."); // Put thread into exception state
            }

            // Run clustering
            RunForAllThreads((Int32 index, Guid threadId, IRoutingAiSlaveService proxy) =>
            { proxy.ComputeClusteringSolution(threadId, GenerateConfiguration(index), _request); });

            // Wait for all threads to finish
            RunForAllThreads((Int32 index, Guid threadId, IRoutingAiSlaveService proxy) =>
            {
                ComputationThreadInfo threadInfo = null;
                do
                {
                    threadInfo = proxy.GetComputationThreadInfo(threadId);
                    Thread.Sleep(500);
                } while (threadInfo.State == ComputationThreadState.Working);
            });
            

        }
    }
}
