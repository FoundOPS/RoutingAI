using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libWyvernzora;
using RoutingAI;
using RoutingAI.DataContracts;
using RoutingAI.Algorithms;
using libWyvernzora.Logging;
using System.Threading;

namespace RoutingAI.Slave
{
    public partial class ComputationThread
    {
        ClusteringSolution _clusteringSolution = null;

        /// <summary>
        /// Starts computing clustering solution async
        /// </summary>
        /// <param name="request">OptimizationRequest containing data to be clustered</param>
        public void ComputeClusteringSolution(OptimizationRequest request)
        {
            if (!_info.AcceptsCommands)
            {
                GlobalLogger.SendLogMessage("CompThread", MessageFlags.Error, "Thread does not accept commands, can't compute clustering: {0}", _id);
                throw new InvalidOperationException();
            }

            // Initialize worker thread
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart((Object o) => ComputeClusteringSolutionAsync((OptimizationRequest)o));
            _thread = new Thread(threadStart);

            // Change thread info accordingly
            _info.AcceptsCommands = false;
            _info.State = ComputationThreadState.Clustering;
            
            // Start computation
            _thread.Start(request);
        }
         
        private void ComputeClusteringSolutionAsync(OptimizationRequest data)
        {
            try
            {
                GlobalLogger.SendLogMessage("CompThread", MessageFlags.Routine, "Clustering computation started: {0}", _id);

                Thread.Sleep(10000);
                _info.AcceptsCommands = true;
                _info.State = ComputationThreadState.Finished;

                GlobalLogger.SendLogMessage("CompThread", MessageFlags.Routine, "Clustering computation finished successfully: {0}", _id);
            }
            catch (ThreadAbortException ex)
            {
                _info.State = ComputationThreadState.Ready;
                _info.AcceptsCommands = true;
                _info.AdditionalInfo = "Clustering computation was aborted";
            }
            catch (Exception ex)
            {
                _info.AcceptsCommands = true;
                _info.AdditionalInfo = String.Format("ExceptionType={0};Message={1};Stack={2}", ex.GetType().FullName, ex.Message, ex.StackTrace);
                _info.State = ComputationThreadState.Exception;
                GlobalLogger.SendLogMessage("CompThread", MessageFlags.Unexpected | MessageFlags.Critical, "ComputationThread({0}) encountered an unexpected error!", _id);
                GlobalLogger.SendLogMessage("CompThread", MessageFlags.Unexpected | MessageFlags.Critical, "ThreadId={3};ExceptionType={0};Message={1};Stack={2}", ex.GetType().FullName, ex.Message, ex.StackTrace, _id);

            }
        }
    }
}
