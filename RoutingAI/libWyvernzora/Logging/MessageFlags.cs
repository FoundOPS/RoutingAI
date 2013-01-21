using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libWyvernzora.Logging
{
    /// <summary>
    /// Represents flags describing log messages
    /// </summary>
    [Flags]
    public enum MessageFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        None = 0,
        /// <summary>
        /// Message is Trivial
        /// It may occur a lot of times, and it may be a bad idea
        /// to display it on GUI
        /// </summary>
        Trivial = 1,
        /// <summary>
        /// Message is Verbose
        /// It may occur a lot of times, and it may be a bad idea
        /// to display it on GUI
        /// </summary>
        Verbose = 2,
        /// <summary>
        /// Message is Routine
        /// It can be displayed in GUI
        /// </summary>
        Routine = 4,
        /// <summary>
        /// Default Flag
        /// Same as Routine
        /// </summary>
        Default = 8,
        /// <summary>
        /// Message is a Warning
        /// It should be slightly highlighted
        /// </summary>
        Warning = 16,
        /// <summary>
        /// Message is an Error
        /// It should be highlighted
        /// </summary>
        Error = 32,
        /// <summary>
        /// Message is critical
        /// It should be highlighted
        /// </summary>
        Critical = 64,
        /// <summary>
        /// Message is Fatal, which usually means this is 
        /// the last message before program crash
        /// It should be highlighted
        /// </summary>
        Fatal = 128,
        /// <summary>
        /// Message log an expected event/error
        /// </summary>
        Expected = 256,
        /// <summary>
        /// Message logs an unexpected event/error
        /// </summary>
        Unexpected = 512,
        /// <summary>
        /// All flags
        /// </summary>
        All = 0x7FFFFFFF
    }
}
