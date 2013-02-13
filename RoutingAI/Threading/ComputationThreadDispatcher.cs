using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libWyvernzora.Logging;
using RoutingAI.DataContracts;
using System.Threading;

namespace RoutingAI.Threading
{
    /// <summary>
    /// Dispatcher class for ComputationThread objects.
    /// This class is a singleton that manages threads, exposes necessary
    /// methods for accessing them and removes threads that have been idle
    /// for too long.
    /// </summary>
    public class ComputationThreadDispatcher : IDisposable
    {
        // Constants
        private const String TAG = "Dispatcher";
        /// <summary>
        /// Specifies the optimal number of threads per processor core
        /// </summary>
        public const Int32 THREADS_PER_PROCESSOR = 2;
        /// <summary>
        /// Specifies the default number of milliseconds a thread must be
        /// idle to be considered time-out
        /// </summary>
        public const Int64 IDLE_THREAD_TIMEOUT = 60 * 1000; // 1 minute
        /// <summary>
        /// The minimum interval between two RemoveIdleThreads() calls
        /// </summary>
        public const Int64 IDLE_REMOVAL_INTERVAL = 10 * 1000; // 10 seconds

        #region Singleton
        // Singleton Pattern
        private static ComputationThreadDispatcher _instance = null;

        /// <summary>
        /// Gets the global instance of dispatcher
        /// </summary>
        public static ComputationThreadDispatcher Instance
        {
            get
            {
                if (_instance == null) _instance = new ComputationThreadDispatcher();
                return _instance;
            }
        }

        #endregion

        // Fields
        private Int32 _capacity;
        private Int64 _idleTimeout;
        private Int64 _lastUpdate = 0;
        private Dictionary<Guid, ComputationThread> _threads;


        // Properties
        /// <summary>
        /// Gets the maximum number of threads allowed by the dispatcher
        /// </summary>
        public Int32 Capacity
        {
            get
            {
                RemoveIdleThreads();
                return _capacity;
            }
        }

        /// <summary>
        /// Returns the number of threads managed by this dispatcher
        /// </summary>
        public Int32 ThreadCount
        {
            get
            {
                RemoveIdleThreads();
                return _threads.Count;
            }
        }

        /// <summary>
        /// Gets or sets number of milliseconds before an idle thread is considered
        /// timed-out and removed from dispatcher
        /// </summary>
        public Int64 IdleThreadTimeout
        {
            get { return _idleTimeout / 10000; }
            set { _idleTimeout = value * 10000; }
        }


        // Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ComputationThreadDispatcher()
            : this(Environment.ProcessorCount * THREADS_PER_PROCESSOR)
        {
            
        }

        /// <summary>
        /// Overloaded constructor.
        /// Creates a dispatcher of the specified capacity
        /// </summary>
        /// <param name="capacity"></param>
        public ComputationThreadDispatcher(Int32 capacity)
        {
            _capacity = capacity;
            _threads = new Dictionary<Guid, ComputationThread>(capacity);

            IdleThreadTimeout = IDLE_THREAD_TIMEOUT;
        }


        // Methods
        /// <summary>
        /// Creates a new computation thread and returns its unique id
        /// </summary>
        /// <returns>Unique id representing the created thread</returns>
        public Guid NewThread()
        {
            RemoveIdleThreads(); // Check for idle threads first

            if (ThreadCount + 1 > _capacity)
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "NewThread: Dispatcher is overburdened! New thread will still be crated but check logs ASAP!");

            ComputationThread thread = new ComputationThread();
            lock (_threads) _threads.Add(thread.ID, thread);

            GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "NewThread: {{{0}}}", thread.ID);
            GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "Dispatcher capacity: {0} alive/{1} total", _threads.Count, _capacity);

            return thread.ID;
        }

        /// <summary>
        /// Creates a new computation thread and with specified unique id
        /// </summary>
        public void NewThread(Guid id)
        {
            RemoveIdleThreads(); // Check for idle threads first

            if (_threads.ContainsKey(id))
                throw new InvalidOperationException();

            ComputationThread ct = new ComputationThread(id);
            _threads.Add(id, ct);
        }

        /// <summary>
        /// Gets info of the specified thread
        /// </summary>
        /// <param name="threadId">Unique id representing the thread</param>
        /// <returns>Unique id representing the thread</returns>
        public ComputationThreadInfo GetThreadInfo(Guid threadId)
        {
            RemoveIdleThreads(); // Check for idle threads first

            if (_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "GetThreadIndo: {{{0}}}", threadId);
                return _threads[threadId].ThreadInfo; 
            }
            else
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected,
                    "GetThreadInfo: Thread does not exist: {{{0}}}", threadId);
                return new ComputationThreadInfo()
                {
                    AcceptsCommands = false,
                    AdditionalInfo = "Thread ID not found",
                    State = ComputationThreadState.Dead,
                    ThreadId = threadId
                };
            }
        }

        /// <summary>
        /// Stops whatever the specified thread is doing and forces it back
        /// to ready state
        /// </summary>
        /// <param name="threadId">Unique id representing the thread</param>
        /// <returns>CallResponse indicating whether the action succeeded</returns>
        public CallResponse AbortThreadAction(Guid threadId)
        {
            RemoveIdleThreads(); // Check for idle threads first

            if (_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "AbortThreadAction: {{{0}}}", threadId);
                _threads[threadId].AbortCurrentAction();
                return new CallResponse() { Success = true, Details = String.Empty };
            }
            else
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "AbortThreadAction: Thread does not exist: {{{0}}}", threadId);
                return new CallResponse() { Success = false, Details = "Thread ID not found" };
            }
        }

        /// <summary>
        /// Stops whatever the specified thread is doing and removes the thread
        /// from dispatcher, disposing all resources associated with it
        /// </summary>
        /// <param name="threadId">Unique id representing the thread</param>
        /// <returns>CallResponse indicating whether the action succeeded</returns>
        public CallResponse DisposeThread(Guid threadId)
        {
            RemoveIdleThreads(); // Check for idle threads first

            if (_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "DisposeThread: {{{0}}}", threadId);
                _threads[threadId].AbortCurrentAction();
                lock (_threads) _threads.Remove(threadId);
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Trivial, "DisposeThread: Success: {{{0}}}", threadId);
                return new CallResponse() { Success = true, Details = String.Empty };
            }
            else
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "DisposeThread: Thread does not exist: {{{0}}}", threadId);
                return new CallResponse() { Success = false, Details = "Thread ID not found" };
            }
        }
    
        /// <summary>
        /// Starts the computation on the specified thread
        /// </summary>
        /// <param name="threadId">Unique id representing the thread</param>
        /// <param name="task">IComputationTask to run</param>
        /// <param name="args">Arguments used by IComputationTask</param>
        /// <returns>CallResponse indicating whether the action succeeded</returns>
        public CallResponse RunComputation(Guid threadId, IComputationTask task, params Object[] args)
        {
            // Check if thread exists
            if (!_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "RunComputation: Thread does not exist: {{{0}}}", threadId);
                return new CallResponse() { Success = false, Details = "Thread ID not found" };
            }

            // Check if thread accepts commands
            if (!_threads[threadId].ThreadInfo.AcceptsCommands)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "RunComputation: Thread does not accept commands: {{{0}}}", threadId);
                return new CallResponse() { Success = false, Details = "Thread does not accept commands" };
            }

            // everything looks good, start working on the task
            _threads[threadId].RunComputation(task, args);

            RemoveIdleThreads(); // Check for idle threads

            return new CallResponse() { Success = true, Details = String.Empty };
        }

        /// <summary>
        /// Gets computation results if finished, null otherwise
        /// </summary>
        /// <param name="threadId">ID of the thread performing computation</param>
        /// <returns></returns>
        public object GetComputationResult(Guid threadId)
        {
            RemoveIdleThreads(); // Check for idle threads first

            if (_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "GetComputationResult: {{{0}}}", threadId);
                return _threads[threadId].Task.GetResult();
            }
            else
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected,
                    "GetComputationResult: Thread does not exist: {{{0}}}", threadId);
                return null;
            }
        }

        // Private Async Methods
        /// <summary>
        /// Removes all threads that have been idle for long enough
        /// to time out.
        /// In case if this method is called too often, it only executes
        /// once every IDLE_REMOVAL_INTERVAL milliseconds.
        /// </summary>
        public void RemoveIdleThreads()
        {
            Int64 now = DateTime.Now.Ticks;

            // Enforce minimum intervals
            Int64 tDiff = now - _lastUpdate;     // Get time elapsed from last executed call
            if (tDiff > IDLE_REMOVAL_INTERVAL * 10000)          // if elapsed time is greater than threshold...
                _lastUpdate = now;               // ...update the last call time
            else return;                                        // ..otherwise, abort the method

            // Remove threads
            Guid[] keys = _threads.Keys.ToArray();  // Copy keys, since we can't modify a collection while iterating through it
            foreach (Guid key in keys)
            {
                lock (_threads) // make sure no other thread is doing anything
                {
                    if (_threads[key].ThreadInfo.State != ComputationThreadState.Working && 
                        _threads[key].ThreadInfo.State != ComputationThreadState.Finished &&
                        now - _threads[key].IdleSince > _idleTimeout)
                    {
                        GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RemoveIdleThreads: Thread idle for too long, removing it: {{{0}}}", key);
                        _threads.Remove(key);
                    }
                }
            }

            
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ComputationThreadDispatcher()
        {
        }

        #endregion
    }
}
