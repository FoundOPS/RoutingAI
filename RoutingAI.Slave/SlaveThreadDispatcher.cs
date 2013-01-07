using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libWyvernzora;
using RoutingAI.DataContracts;
using libWyvernzora.Logging;

namespace RoutingAI.Slave
{
    /// <summary>
    /// Object that does all the thread management
    /// </summary>
    public class SlaveThreadDispatcher
    {
        // Constants
        public const Int32 THREADS_PER_CORE = 2;

        #region Singleton

        private static SlaveThreadDispatcher _instance = null;

        public static SlaveThreadDispatcher Instance
        {
            get
            {
                if (_instance == null) _instance = new SlaveThreadDispatcher();
                return _instance;
            }
        }

        #endregion

        // Fields
        private readonly Int32 _capacity;
        private Dictionary<Guid, ComputationThread> _threads;

        // Properties
        /// <summary>
        /// Returns the maximum number of computation threads
        /// this dispatcher allows
        /// </summary>
        public Int32 Capacity
        { get { return _capacity; } }
        /// <summary>
        /// Returns the number of threads on this dispatcher
        /// </summary>
        public Int32 ThreadCount
        { get { return _threads.Count; } }
        /// <summary>
        /// Returns a pair(ThreadCount, Capacity)
        /// </summary>
        public Pair<Int32, Int32> ServerLoadInfo
        { get { return new Pair<int, int>(_threads.Count, _capacity); } }

        // Constructor
        public SlaveThreadDispatcher()
        {
            _capacity = Environment.ProcessorCount * THREADS_PER_CORE;
            _threads = new Dictionary<Guid, ComputationThread>(_capacity);

            GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Routine, "SlaveThreadDispatcher with capacity of {0} initialized", _capacity);
        }

        // Methods not affected by AccpetsCommands flag
        /// <summary>
        /// Creates a new computation thread and puts it into ready state
        /// </summary>
        /// <param name="config">Configuration used for creating the thread</param>
        /// <returns>Unique id of the created thread</returns>
        public Guid CreateNewThread(SlaveConfig config)
        {
            ComputationThread thread = new ComputationThread(config);
            _threads.Add(thread.ID, thread);

            GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Routine, "ComputationThread created: {0}", thread.ID);
            GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Trivial, "Dispatcher Capacity: {0} available of total {1}", _capacity - _threads.Count, _capacity);

            return thread.ID;
        }
        /// <summary>
        /// Gets info of the specified thread.
        /// If the thread does not exist, state of the returned info will be "dead"
        /// </summary>
        /// <param name="threadId">Unique id of the thread to find</param>
        /// <returns>Info of the specified thread</returns>
        public ComputationThreadInfo GetThreadInfo(Guid threadId)
        {
            if (!_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Warning, "Can't get info: thread does not exist: {0}", threadId);
                return new ComputationThreadInfo()
                {
                    AcceptsCommands = false,
                    AdditionalInfo = String.Format("ComputationThread {0} does not exist or already died", threadId),
                    State = ComputationThreadState.Dead,
                    ThreadId = new Guid()
                };
            }
            else
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Verbose & MessageFlags.Trivial, "GetThreadInfo called on thread {0}", threadId);
                return _threads[threadId].ThreadInfo;
            }
        }
        /// <summary>
        /// Stops computation in progress
        /// </summary>
        /// <param name="threadId"></param>
        public CallResponse AbortThreadComputation(Guid threadId)
        {
            if (_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Routine, "AbortThreadComputation: {0}", threadId);
                _threads[threadId].AbortCurrentAction();
                return new CallResponse() { Success = true, Details = "" };
            }
            else
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Warning, "Attempt to abort thread that does not exist: {0}", threadId);
                return new CallResponse() { Success = false, Details = "Specified thread was not found" };
            }
        }
        /// <summary>
        /// Aborts all computation and removed the thread from dispatcher
        /// </summary>
        /// <param name="threadId"></param>
        public CallResponse KillThread(Guid threadId)
        {
            if (!_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Warning, "Attempt to kill a thread that does not exist: {0}", threadId);
                return new CallResponse() { Success = false, Details = "Specified thread was not found" };
            }
            else
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Routine, "Killing thread: {0}", threadId);
                _threads[threadId].AbortCurrentAction();
                _threads.Remove(threadId);
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Routine, "Thread is dead: {0}", threadId);
                return new CallResponse() { Success = true, Details = "" };
            }
        }

        // Methods affected by AcceptsCommand flag
        /// <summary>
        /// Starts computing clustering solution on the specified thread
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public CallResponse ComputeClusteringSolution(Guid threadId, OptimizationRequest data)
        {
            // check if there is such a thread
            if (!_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Warning, "Attempt to compute on a thread that does not exist: {0}", threadId);
                return new CallResponse() { Success = false, Details = "Thread ID not found" };
            }

            // check if the thread accepts commands at this time
            if (!GetThreadInfo(threadId).AcceptsCommands)
            {
                GlobalLogger.SendLogMessage("Dispatcher", MessageFlags.Warning, "Attempt to compute on a thread that does not accept commands: {0}", threadId);
                return new CallResponse() { Success = false, Details = "Thread does not accept commands at this time" };
            }

            // everything seems ok now, start computation
            _threads[threadId].ComputeClusteringSolution(data);
            return new CallResponse() { Success = true, Details = "" };
        }
    }
}