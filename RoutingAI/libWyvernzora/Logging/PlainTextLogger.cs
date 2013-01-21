using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace libWyvernzora.Logging
{
    /// <summary>
    /// Basic implementation of Logger.
    /// Writes logs into a specified plain text file
    /// </summary>
    public class PlainTextLogger : Logger
    {
        private readonly StreamWriter writer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">Path of the log file</param>
        /// <param name="overwrite">If true, contents of the log file will be erased when the logger is created</param>
        public PlainTextLogger(String filePath, Boolean overwrite = true)
        {
            writer = new StreamWriter(filePath, !overwrite);
            submissionBatch = 1;
        }

        /// <summary>
        /// Submits currently queued messages for processing
        /// </summary>
        /// <param name="messages">Array of currently queued messages</param>
        protected override void SubmitMessageQueue(LoggerEventArgs[] messages)
        {
            foreach (LoggerEventArgs e in messages)
            {
                DateTime dt = new DateTime(e.TimeStamp * 10000);
                String time = String.Format("{0}:{1}:{2}.{3}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                writer.WriteLine("{0,-20}{1,-20}{2}", time, e.Tag, e.Message);
            }
            writer.Flush();
        }

        /// <summary>
        /// Disposes the Logger and releases all reasources
        /// associated with it
        /// </summary>
        public override void Dispose()
        {
        }

        /// <summary>
        /// Releases all resources taken by this Logger
        /// </summary>
        protected override void ReleaseResources()
        {
            writer.Close();
        }
    }
}
