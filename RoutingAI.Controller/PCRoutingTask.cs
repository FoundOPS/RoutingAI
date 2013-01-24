﻿using RoutingAI.Threading;
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
            RunForAllThreads((Int32 index, Guid threadId, IRoutingAiSlaveService proxy) => proxy.ComputeClusteringSolution(threadId, GenerateConfiguration(index), _request));

            // Wait for all threads to finish
            RunForAllThreads((Int32 index, Guid threadId, IRoutingAiSlaveService proxy) =>
            {
                ComputationThreadInfo threadInfo = null;
                do
                {
                    threadInfo = proxy.GetComputationThreadInfo(threadId);
                    if (threadInfo.State == ComputationThreadState.Working) Thread.Sleep(500); // Wait if thread not finished
                } while (threadInfo.State == ComputationThreadState.Working);
            });
            
            // Retrieve Clustering Solution
            List<ClusteringSolution> solutions = new List<ClusteringSolution>();
            RunForAllThreads((Int32 index, Guid id, IRoutingAiSlaveService proxy) => solutions.Add(proxy.GetClusteringSolution(id)));
            solutions.Sort();
            ClusteringSolution clusters = solutions.First();

            // Strip redundant data from OptimizationRequest
                //All tasks are already in CLusteringSolution so no need to pass them around
            _request.StripData();

            // Send out clusters for optimization
            RunForAllThreads((Int32 index, Guid id, IRoutingAiSlaveService proxy) =>
                {
#if DEBUG
                    if (index == 0)
                    {

                        // Send compute command
                        proxy.ComputeOptimizedSolution(id, GenerateConfiguration(index), _request,
                                                           _request.Resources[0], clusters.Clusters[0]);
                    }

#else
                    // Assign a fixed cluster if thread index is less than cluster count
                        // otherwise, assign a random cluster
                    int clusterIndex = index < clusters.Clusters.Length ? index : _rand.Next(clusters.Clusters.Length);

                    // Send compute command
                    proxy.ComputeOptimizedSolution(id, GenerateConfiguration(index), _request,
                                                       _request.Resources[clusterIndex], clusters.Clusters[clusterIndex]);
#endif
                });

            // Wait for all threads to finish
            RunForAllThreads((Int32 index, Guid threadId, IRoutingAiSlaveService proxy) =>
            {
                ComputationThreadInfo threadInfo = null;
                do
                {
                    threadInfo = proxy.GetComputationThreadInfo(threadId);
                    if (threadInfo.State == ComputationThreadState.Working) Thread.Sleep(500); // Wait if thread not finished
                } while (threadInfo.State == ComputationThreadState.Working);
            });

            // Dispose all threads
            HandleAbort();
        }
    }
}
