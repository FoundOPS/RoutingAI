using libWyvernzora;
using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libWyvernzora.Logging;
using RoutingAI.DataContracts;
using RoutingAI.Threading;

namespace RoutingAI.Slave
{
    class RoutingAiSlave : IRoutingAiSlaveService
    {
        const String GREETING_STRING = "Hello World（￣ー￣）";

        ComputationThreadDispatcher _dispatcher = ComputationThreadDispatcher.Instance;

        #region Server

        /// <summary>
        /// Returns version string
        /// </summary>
        /// <returns></returns>
        public string Ping()
        {
            return GREETING_STRING;
        }

        /// <summary>
        /// Returns a current thread count/total capacity pair
        /// </summary>
        /// <returns></returns>
        public Pair<int, int> GetServerCapacityInfo()
        {
            return new Pair<Int32, Int32>(_dispatcher.ThreadCount, _dispatcher.Capacity);
        }

        #endregion

        #region Computation Threads

        /// <summary>
        /// Sets up a new computation thread and puts it into ready state
        /// </summary>
        /// <param name="conf">Thread configuration</param>
        /// <returns></returns>
        public Guid CreateComputationThread()
        { return _dispatcher.NewThread(); }

        /// <summary>
        /// Gets info of the specified thread.
        /// If the thread does not exist, state of the returned info will be "dead"
        /// </summary>
        /// <param name="threadId">Unique id of the thread to find</param>
        /// <returns>Info of the specified thread</returns>
        public DataContracts.ComputationThreadInfo GetComputationThreadInfo(Guid threadId)
        { return _dispatcher.GetThreadInfo(threadId); }

        /// <summary>
        /// Stops computation in progress
        /// </summary>
        /// <param name="threadId"></param>
        public CallResponse AbortComputation(Guid threadId)
        {
            try
            {
                return _dispatcher.AbortThreadAction(threadId);
            }
            catch (Exception ex)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Critical | MessageFlags.Unexpected, 
                    "Unexpected exception in AbortComputation(): ID = {0}; Type = {1}; Message = {2}; Stack = {3}", 
                    threadId, ex.GetType().FullName, ex.Message, ex.StackTrace);
                return new CallResponse() { Success = false, Details = String.Format("Unexpected Error of type {0}, see RoutingAI.Slave instance logs for more details", ex.GetType().FullName) };
            }
        }

        /// <summary>
        /// Aborts all computation and removed the thread from dispatcher
        /// </summary>
        /// <param name="threadId"></param>
        public CallResponse DisposeComputationThread(Guid threadId)
        { return _dispatcher.DisposeThread(threadId); }

        #endregion



        public CallResponse StartComputingClusteringSolution(Guid threadId, SlaveConfig config, OptimizationRequest data)
        {
            DummyComputationTask dummyTask = new DummyComputationTask();
            return _dispatcher.RunComputation(threadId, dummyTask, null);
        }




    }
}
