using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using libWyvernzora.Logging;
using RoutingAI.Algorithms;


namespace RoutingAI.Slave
{
    /*
     * Computation Thread class, part with general variables and methods not related
     * to actual computation
     */
    public partial class ComputationThread
    {
        
        protected readonly SlaveConfig _config;   // configuration of the computation thread
        protected readonly Guid _id;              // unique id of this thread
        protected Thread _thread;                 // actual worker thread wrapped by this object
        protected ComputationThreadInfo _info;    // thread info object
                                                // DO NOT MODIFY IT ANYWHERE ELSE EXCEPT CODE RUNNING ON WORKER THREAD!!
        protected Int64 _idleTime;                // Time when thread became idle (ticks). Refreshed whenever thread becomes idle, used for removing unused threads

        /// <summary>
        /// Gets the Guid representing this computation thread
        /// </summary>
        public Guid ID
        { get { return _id; } }
        /// <summary>
        /// Gets info about this thread
        /// </summary>
        public ComputationThreadInfo ThreadInfo
        {
            get
            {
                return _info;
            }
        }
        /// <summary>
        /// Gets a time value since when the thread is idle
        /// </summary>
        public Int64 IdleSince
        { get { return _idleTime; } }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="config"></param>
        public ComputationThread(SlaveConfig config)
        {
            this._config = config;
            this._id = Guid.NewGuid();
            this._info = new ComputationThreadInfo()
            {
                 AcceptsCommands = true,
                 AdditionalInfo = "none",
                 State = ComputationThreadState.Ready,
                 ThreadId = this._id
            };
            this._idleTime = DateTime.Now.Ticks;

            GlobalLogger.SendLogMessage("CompThread", MessageFlags.Routine, "ComputationThread initialized and ready to roll: {0}", _id);
        }

        #region API

        /// <summary>
        /// Terminates current action carried out by the thread
        /// </summary>
        /// <param name="force">
        /// If true, it will be a force kill and worker won't
        /// be allowed to end action gracefully.
        /// </param>
        public void AbortCurrentAction()
        {
            if (_thread == null)
            {
                GlobalLogger.SendLogMessage("CompThread", MessageFlags.Warning, "Attempt to stop idle computation thread: {0}", _id);
                return; // there is no action to stop
            }
            GlobalLogger.SendLogMessage("CompThread", MessageFlags.Routine, "Beginning to abort computation thread: {0}", _id);
            _thread.Abort();
            _thread.Join(); // wait for thread to actually stop.. ?
            _info.AcceptsCommands = true;
            _info.State = ComputationThreadState.Ready;
            GlobalLogger.SendLogMessage("CompThread", MessageFlags.Routine, "Computation thread now idle: {0}", _id);
        }
    
        


        #endregion

        #region Redis/OSRM/Caching

        protected readonly IDistanceAlgorithm<Task> _distanceAlg;



        #endregion

    }
}
