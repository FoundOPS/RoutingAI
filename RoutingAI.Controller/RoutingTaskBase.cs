using RoutingAI.DataContracts;
using RoutingAI.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using libWyvernzora;
using RoutingAI.ServiceContracts;
using libWyvernzora.Logging;
using System.ServiceModel;

namespace RoutingAI.Controller
{
    /// <summary>
    /// Base class for all routing-related computation tasks
    /// </summary>
    public abstract class RoutingTaskBase : IComputationTask<Solution>
    {
        /// <summary>
        /// Specifies number of threads a task spawns on a slave server
        /// </summary>
        protected const Int32 THREAD_PER_SLAVE = 1;
        /// <summary>
        /// Logging tag for this class
        /// </summary>
        protected const String TAG = "RoutingTaskBase";

        /// <summary>
        /// Task-specific random number generator
        /// </summary>
        protected readonly Random _rand = new Random();

        /// <summary>
        /// Optimization Request containing data to be processed
        /// </summary>
        protected OptimizationRequest _request;
        /// <summary>
        /// Result of the computation; null if computation is incomplete
        /// </summary>
        protected Solution _solution = null;


        // Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config">Slave thread configuration</param>
        /// <param name="request">Optimization request</param>
        public RoutingTaskBase(OptimizationRequest request)
        {
            _request = request;
        }

        #region Server allocation and management

        // Server resources allocated for this task
        private IPEndPoint[] _redis;    // Redis servers that can be used by this task
        private IPEndPoint[] _osrm;     // OSRM servers that can be used by this task
        private IPEndPoint _librarian;  // 
        private Pair<Guid, IRoutingAiSlaveService>[] _threads;

        /// <summary>
        /// Allocates server resources from ServerResourceManager.
        /// </summary>
        /// <returns>True is allocation is successful; false otherwise</returns>
        protected Boolean AllocateServerResources()
        {
            // Get SRM instance
            ServerResourceManager resMan = ServerResourceManager.Instance;

            // Allocate Redis servers
            _redis = resMan.GetRedisServers(_request.RegionCode);
            if (_redis.Length == 0)
            {
                // No servers allocated, log it down and return failure
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "AllocateServerResources: No Redis servers allocated: {{{0}}}", _request.Id);
                return false;
            }

            // Allocate OSRM servers
            _osrm = resMan.GetOsrmServers(_request.RegionCode);
            if (_osrm.Length == 0)
            {
                // No servers allocated, log it down and return failure
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "AllocateServerResources: No OSRM servers allocated: {{{0}}}", _request.Id);
                return false;
            }

            // Allocate a librarian server
            _librarian = resMan.GetLibrarianServer();
            if (_librarian == null)
            {
                // No librarian allocated, log it down and return failure
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "AllocateServerResources: No Librarian server allocated: {{{0}}}", _request.Id);
                return false;
            }

            // Allocate slave servers and threads
            List<Pair<Guid, IRoutingAiSlaveService>> threads    // temporary list for storing allocation results
                = new List<Pair<Guid, IRoutingAiSlaveService>>();
            Int32 threadCount = (int)(Math.Log10(_request.Tasks.Length * _request.Resources.Length) * 3) + 1;
            for (int i = 0; i < threadCount; i++)
            {
                SlaveServerInfo slave = resMan.GetSlaveServer();

                // in case if the slave server is overburdened, log a warning message but still create the thread
                if (slave.RemainingCapacity <= 0)
                { GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "AllocateServerResources: Server {0} overburdened: {{{1}}}", slave.Endpoint, _request.Id); }
                // in case if the slave server is CRITICALLY overburdened, log an error message and return failure
                if (slave.RemainingCapacity <= -(slave.TotalCapacity / 2))
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, 
                        "AllocateServerResources: Server {0} is critically overburdened: {{{1}}}", slave.Endpoint, _request.Id);

                    // Release all threads already created
                    foreach (Pair<Guid, IRoutingAiSlaveService> t in threads)
                        t.Second.DisposeComputationThread(t.First);

                    return false;
                }

                // Get a new proxy, create a new thread and store them in the thread list
                IRoutingAiSlaveService proxy = ServiceProxyHelper.GetSlaveProxy(slave.Endpoint);
                Guid tid = proxy.CreateComputationThread();
                threads.Add(new Pair<Guid,IRoutingAiSlaveService>(tid, slave.Proxy));
            }
            _threads = threads.ToArray();

            // If the method didnt't return until here, allocation was successful
            return true;
        }

        /// <summary>
        /// Gets an array of endpoints representing Redis servers
        /// allocated for this task
        /// </summary>
        protected IPEndPoint[] RedisServers
        { get { return _redis; } }

        /// <summary>
        /// Gets an array of endpoints representing OSRM servers
        /// allocated for this task
        /// </summary>
        protected IPEndPoint[] OsrmServers
        { get { return _osrm; } }

        /// <summary>
        /// Gets an endpoint representing librarian server allocated
        /// for this task
        /// </summary>
        protected IPEndPoint Librarian
        { get { return _librarian; } }


        /// <summary>
        /// Gets the number of computation threads this task is running on
        /// </summary>
        protected Int32 ThreadCount
        { get { return _threads.Length; } }

        /// <summary>
        /// Gets the Guid of the specified computation thread
        /// </summary>
        /// <param name="threadIndex">Index of the computation thread</param>
        /// <returns>Unique id representing the thread</returns>
        protected Guid GetThreadId(Int32 threadIndex)
        { return _threads[threadIndex].First; }

        /// <summary>
        /// Gets the WCF service proxy for the specified computation thread
        /// </summary>
        /// <param name="threadIndex">Index of the computation thread</param>
        /// <returns>WCF Service Proxy for the thread</returns>
        protected IRoutingAiSlaveService GetThreadProxy(Int32 threadIndex)
        { return _threads[threadIndex].Second; }

        /// <summary>
        /// Runs an action for all slave threads
        /// </summary>
        /// <param name="action">Action(Int32 index, Guid threadId, IRoutingAiSlaveService proxy)</param>
        protected void RunForAllThreads(Action<Int32, Guid, IRoutingAiSlaveService> action)
        {
            for (int i = 0; i < ThreadCount; i++)
            {
                Guid id = GetThreadId(i);
                IRoutingAiSlaveService proxy = GetThreadProxy(i);
                action(i, id, proxy);
            }
        }


        /// <summary>
        /// Generates a SlaveConfig object for the specified thread
        /// </summary>
        /// <param name="threadIndex">Index of the thread</param>
        /// <returns>SlaveConfig object generated for the thread</returns>
        protected SlaveConfig GenerateConfiguration(Int32 threadIndex)
        {
            return new SlaveConfig()
            {
                LibrarianServer = _librarian,
                OSRMServers = _osrm,
                RandomSeed = _rand.Next(),
                RedisServers = _redis,
                SlaveIndex = new Pair<int, int>(threadIndex, ThreadCount)
            };
        }

        #endregion
        
        #region IComputationTask Members

        /// <summary>
        /// Gets the result of the computation
        /// </summary>
        public Solution Result
        {
            get { return _solution; }
        }

        /// <summary>
        /// Starts computing Routing Solution
        /// </summary>
        /// <param name="args">Arguments used by routing algorithm</param>
        public abstract void Compute(params object[] args);

        /// <summary>
        /// Handles abort requests gracefully
        /// </summary>
        public virtual void HandleAbort()
        {
            Dispose();
        }

        /// <summary>
        /// Gets routing solution produced by the routing algorithm
        /// </summary>
        /// <returns>Routing Solution</returns>
        public object GetResult()
        {
            return _solution;
        }

        /// <summary>
        /// Disposes the task and releases all resources
        /// </summary>
        public virtual void Dispose()
        {
            for (int i = 0; i < ThreadCount; i++)
            {
                GetThreadProxy(i).DisposeComputationThread(GetThreadId(i));
            }
        }

        #endregion

    }
}
