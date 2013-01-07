using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libWyvernzora.Logging;
using RoutingAI.DataContracts;
using System.Threading;

namespace RoutingAI.Threading
{
    public class ComputationThreadDispatcher : IDisposable
    {
        // Constants
        public const String TAG = "Dispatcher";
        public const Int32 THREADS_PER_PROCESSOR = 2;
        public const Int64 IDLE_THREAD_TIMEOUT = 60 * 1000; // 1 minute


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


        // Fields
        private Int32 _capacity;
        private Int64 _idleTimeout;
        private Thread _maintain;
        private Dictionary<Guid, ComputationThread> _threads;


        // Properties
        /// <summary>
        /// Gets the maximum number of threads allowed by the dispatcher
        /// </summary>
        public Int32 Capacity
        { get { return _capacity; } }

        /// <summary>
        /// Returns the number of threads managed by this dispatcher
        /// </summary>
        public Int32 ThreadCount
        { get { return _threads.Count; } }

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
            _maintain = new Thread(new ThreadStart(MaintainThreadsAsync));
            _maintain.Start();
        }


        // Methods
        
        public Guid NewThread()
        {
            ComputationThread thread = new ComputationThread();
            lock (this) _threads.Add(thread.ID, thread);

            GlobalLogger.SendLogMessage(TAG, MessageFlags.Trivial, "NewThread: {{{0}}}", thread.ID);
            GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "Dispatcher capacity: {0} alive/{1} total", _threads.Count, _capacity);

            return thread.ID;
        }

        public ComputationThreadInfo GetThreadInfo(Guid threadId)
        {
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

        public CallResponse AbortThreadAction(Guid threadId)
        {
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

        public CallResponse DisposeThread(Guid threadId)
        {
            if (_threads.ContainsKey(threadId))
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "DisposeThread: {{{0}}}", threadId);
                _threads[threadId].AbortCurrentAction();
                lock (this) _threads.Remove(threadId);
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Trivial, "DisposeThread: Success: {{{0}}}", threadId);
                return new CallResponse() { Success = true, Details = String.Empty };
            }
            else
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "DisposeThread: Thread does not exist: {{{0}}}", threadId);
                return new CallResponse() { Success = false, Details = "Thread ID not found" };
            }
        }
    
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
            return new CallResponse() { Success = true, Details = String.Empty };
        }


        // Private Async Methods
        private void MaintainThreadsAsync()
        {
            try
            {
                while (true)
                {
                    Int64 now = DateTime.Now.Ticks;
                    Guid[] keys = _threads.Keys.ToArray();
                    foreach (Guid key in keys)
                    {
                        lock (this)
                        {
                            if (_threads[key].ThreadInfo.State != ComputationThreadState.Working &&
                                now - _threads[key].IdleSince > _idleTimeout)
                            {
                                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "MaintainThreadsAsync: Thread idle for too long, removing it: {{{0}}}", key);
                                _threads.Remove(key);
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException)
            { /* Do Nothing */ }
        }

        #region IDisposable Members

        public void Dispose()
        {
            _maintain.Abort();
        }

        ~ComputationThreadDispatcher()
        {
            _maintain.Abort();
        }

        #endregion


    }
}
