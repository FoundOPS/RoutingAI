using libWyvernzora.Logging;
using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoutingAI.Threading
{
    public class ComputationThread
    {
        // Constants
        private const String TAG = "CompThread";


        // Fields
        protected readonly Guid _id;                // Unique id representing this thread
        protected Int64 _idleSince;                 // Time since when this thread did nothing useful
        protected Thread _thread;                   // Actual thread wrapped by this ComputationThread
        protected IComputationTask _task;            // Current computation task
        protected ComputationThreadInfo _info;      // Thread info, do not modify unless you know what you are doing


        // Properties
        /// <summary>
        /// Gets the unique id representing this thread
        /// </summary>
        public Guid ID
        { get { return _id; } }

        /// <summary>
        /// Gets info about this thread
        /// </summary>
        public ComputationThreadInfo ThreadInfo
        { get { return _info; } }

        /// <summary>
        /// Gets a time value since when this thread did nothing useful
        /// </summary>
        public Int64 IdleSince
        { get { return _idleSince; } }

        /// <summary>
        /// Gets the computation task that is currently running on this thread
        /// May be null if there is no task assigned
        /// </summary>
        public IComputationTask Task
        {
            get { return _task; }
        }


        // Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public ComputationThread()
            : this(Guid.NewGuid())
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        public ComputationThread(Guid id)
        {
            this._id = id;
            this._info = new ComputationThreadInfo()
            {
                AcceptsCommands = true,
                AdditionalInfo = "none",
                State = ComputationThreadState.Ready,
                ThreadId = _id
            };
            this._idleSince = DateTime.Now.Ticks;

            GlobalLogger.SendLogMessage(TAG, MessageFlags.Trivial, "Computation thread initialized and ready to roll: {{{0}}}", _id);
        }


        // Methods
        /// <summary>
        /// Starts computing the specified task
        /// </summary>
        /// <param name="task">Task to compute</param>
        /// <param name="args">Arguments for computing the task</param>
        public void RunComputation(IComputationTask task, params Object[] args)
        {
            // Check if the thread accepts commands
            if (!_info.AcceptsCommands)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "RunComputation: Thread rejects commands: {{{0}}}", _id);
                throw new InvalidOperationException("Thread is not in the correct state to accept the computation task");
            }

            // Assign current task
            _task = task;

            // Initialize worker thread
            ParameterizedThreadStart threadStart = new ParameterizedThreadStart((Object o)=>RunComputationAsync((Object[])o));
            _thread = new Thread(threadStart);

            // Change thread info
            _info.AcceptsCommands = false;
            _info.State = ComputationThreadState.Working;

            // Start computing
            _thread.Start(args);
        }

        /// <summary>
        /// Aborts currently running computation and puts the thread back
        /// into ready state
        /// </summary>
        public void AbortCurrentAction()
        {
            if (_thread == null || _info.State != ComputationThreadState.Working)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine | MessageFlags.Expected, "AbortCurrentAction: Already Idle: {{{0}}}", _id);
                return;
            }

            GlobalLogger.SendLogMessage(TAG, MessageFlags.Trivial, "AbortCurrentAction: Stopping: {{{0}}}", _id);
            _thread.Abort();
            _thread.Join(); // wait for action to stop before returning
            _info.AcceptsCommands = true;
            _info.State = ComputationThreadState.Ready;
            _idleSince = DateTime.Now.Ticks;
            GlobalLogger.SendLogMessage(TAG, MessageFlags.Trivial, "AbortCurrentAction: Success: {{{0}}}", _id);
        }


        // Private wrapper methods
        /// <summary>
        /// Inner wrapper method for running task computation
        /// Includes logging stuff and exception handling stuff
        /// </summary>
        /// <param name="args"></param>
        private void RunComputationAsync(Object[] args)
        {
            try
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RunComputation: Started: {{{0}}}", _id);
                // Run actual task
                _task.Compute(args);
                // Task done, change thread info
                _info.AcceptsCommands = true;
                _info.State = ComputationThreadState.Finished;

                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RunComputation: Success: {{{0}}}", _id);
            }
            catch (ThreadAbortException ex)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RunComputation: Abort start: {{{0}}}", _id);

                // Thread was aborted, handle gracefully
                _task.HandleAbort();

                _info.AcceptsCommands = true;
                _info.State = ComputationThreadState.Ready;
                _info.AdditionalInfo = "Last action aborted";
                _idleSince = DateTime.Now.Ticks;

                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RunComputation: Abort success: {{{0}}}", _id);
            }
            catch (Exception ex)
            {
                _info.AcceptsCommands = true;
                _info.State = ComputationThreadState.Exception;
                _info.AdditionalInfo = String.Format("{{ThreadID = {0}; ExceptionType = {1}; Message = {2}; StackTrace = {3}}}",
                    _id, ex.GetType().FullName, ex.Message, ex.StackTrace);
                _idleSince = DateTime.Now.Ticks;

                GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Unexpected, 
                    "RunComputation: Unexpected Exception: {{ThreadID = {0}; ExceptionType = {1}; Message = {2}; StackTrace = {3}}}", 
                    _id, ex.GetType().FullName, ex.Message, ex.StackTrace);
            }
        }
    }
}
