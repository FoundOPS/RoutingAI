using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace libWyvernzora.Logging
{
    /// <summary>
    /// Abstract implementation of a logger.
    /// Inherit this class to create specific loggers
    /// </summary>
    public abstract class Logger : IDisposable
    {
        /// <summary>
        /// Thread-safe queue for buffering 
        /// </summary>
        protected ConcurrentQueue<LoggerEventArgs> messages = new ConcurrentQueue<LoggerEventArgs>();
        /// <summary>
        /// Specifies batch size (number of messages that trigger a submissin operation)
        /// </summary>
        protected Int32 submissionBatch = 100;

        /// <summary>
        /// Indicates whether logger thread has been requested to stop
        /// </summary>
        protected Boolean _terminateRequest = false;

        /// <summary>
        /// Flags that this logger accepts.
        /// If a message does not contains any of these flags, it will be skipped
        /// </summary>
        protected MessageFlags _acceptFlags = MessageFlags.All;
        /// <summary>
        /// Flags that this logger does not accept.
        /// If a message contains any of these flags, it will be skipped
        /// </summary>
        protected MessageFlags _rejectFlags = MessageFlags.None;

        // Properties
        /// <summary>
        /// Determines the number of messages written to the physical storage each submission.
        /// </summary>
        /// <remarks>
        /// Setting this property to a larger number will decrease the frequency of access to the underlying storage,
        /// which will improve performance if each access poses an overhead. However, in case of unexpected termination
        /// of the program, large submission batch size will result in last log entries being lost.
        /// </remarks>
        public Int32 SubmissionBatchSize
        {
            get { return submissionBatch; }
            set
            {
               submissionBatch = value;
            }
        }
        /// <summary>
        /// Gets or sets flags that this logger accepts.
        /// If a message does not contains any of these flags, it will be skipped
        /// </summary>
        public MessageFlags AcceptFlags
        {
            get { return _acceptFlags; }
            set { _acceptFlags = value; }
        }
        /// <summary>
        /// Gets or sets flags that this logger does not accept.
        /// If a message contains any of these flags, it will be skipped
        /// </summary>
        public MessageFlags RejectFlags
        {
            get { return _rejectFlags; }
            set { _rejectFlags = value; }
        }


        // Logger thread method, must remain private!
        private void CheckSubmissionQueue()
        {
            // Do nothing if there is no need to flush messages
            if (messages.Count >= submissionBatch)
            {
                // Submit messages if needed

                /* A faster way of doing this might be to copy entire _messages into an array via ToArray() and clearing the queue,
                 * but considering that there can be new messages arriving between these two instructions, copying one by one
                 * seems more reliable. More investigation needed.
                 */

                LoggerEventArgs[] messages = new LoggerEventArgs[submissionBatch];
                for (int i = 0; i < submissionBatch; i++)
                {
                    if (!this.messages.TryDequeue(out messages[i]))
                        throw new Exception("Logger.RunLogger(): Failed to dequeue the next log message!");
                }
                SubmitMessageQueue(messages);
            }
        }

        /// <summary>
        /// Event handler for OnLogMessage event
        /// </summary>
        /// <param name="sender">Event origin</param>
        /// <param name="e">Event data</param>
        public void HandleMessage(Object sender, LoggerEventArgs e)
        {
            EnqueueMessage(e);
        }

        /// <summary>
        /// Enqueues a message for processing
        /// </summary>
        /// <param name="arg"></param>
        public virtual void EnqueueMessage(LoggerEventArgs arg)
        {
            if ((arg.Flags & _rejectFlags) != 0) return;
            if ((arg.Flags & _acceptFlags) == 0 && _acceptFlags != MessageFlags.All) return;
            messages.Enqueue(arg);
            CheckSubmissionQueue();
        }

        /// <summary>
        /// When implemented, writes a collection of log messages to underlying physical storage
        /// </summary>
        /// <param name="messages">Collection of messages to write</param>
        protected abstract void SubmitMessageQueue(LoggerEventArgs[] messages);

        /// <summary>
        /// Releases all resources used by logger.
        /// </summary>
        protected abstract void ReleaseResources();

        /// <summary>
        /// Member of IDisposable interface, do cleanup here.
        /// </summary>
        public abstract void Dispose();


    }

}
