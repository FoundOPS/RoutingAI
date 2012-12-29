using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libWyvernzora.Logging
{
    public class LoggerEventArgs : EventArgs
    {
        public LoggerEventArgs(Int64 time, String tag, MessageFlags flag, String message)
        {
            this.TimeStamp = time;
            this.Tag = tag;
            this.Message = message;
            this.Flags = flag;
        }
        public LoggerEventArgs(String tag, MessageFlags flag, String message)
            : this(DateTime.Now.Ticks / 10000, tag, flag, message) { }
        public LoggerEventArgs(String tag, MessageFlags flag, String format, params Object[] args)
            : this(tag, flag, String.Format(format, args)) { }

        public Int64 TimeStamp { get; set; }
        public String Tag { get; set; }
        public String Message { get; set; }
        public MessageFlags Flags { get; set; }
    }
}
