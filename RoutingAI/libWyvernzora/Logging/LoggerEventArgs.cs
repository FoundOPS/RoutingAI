using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libWyvernzora.Logging
{
    /// <summary>
    /// Represents a log message
    /// </summary>
    public class LoggerEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="time">Current time in milliseconds</param>
        /// <param name="tag">Tag of the log message</param>
        /// <param name="flag">Flags of the log message</param>
        /// <param name="message">Contents of the log message</param>
        public LoggerEventArgs(Int64 time, String tag, MessageFlags flag, String message)
        {
            this.TimeStamp = time;
            this.Tag = tag;
            this.Message = message;
            this.Flags = flag;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tag">Tag of the log message</param>
        /// <param name="flag">Flags of the log message</param>
        /// <param name="message">Contents of the log message</param>
        public LoggerEventArgs(String tag, MessageFlags flag, String message)
            : this(DateTime.Now.Ticks / 10000, tag, flag, message) { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tag">Tag of the log message</param>
        /// <param name="flag">Flags of the log message</param>
        /// <param name="format">Format string for the log message</param>
        /// <param name="args">Arguments for the log message</param>
        public LoggerEventArgs(String tag, MessageFlags flag, String format, params Object[] args)
            : this(tag, flag, String.Format(format, args)) { }

        /// <summary>
        /// Gets or sets the time value when this message was created
        /// </summary>
        public Int64 TimeStamp { get; set; }
        /// <summary>
        /// Gets or sets the tag of the message
        /// </summary>
        public String Tag { get; set; }
        /// <summary>
        /// Gets or sets the message string
        /// </summary>
        public String Message { get; set; }
        /// <summary>
        /// Gets or sets message flags
        /// </summary>
        public MessageFlags Flags { get; set; }
    }
}
