using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libWyvernzora.Logging
{
    [Flags]
    public enum MessageFlags
    {
        None = 0,
        Trivial,
        Verbose,
        Routine,
        Default,
        Warning,
        Error,
        Critical,
        Fatal,
        Expected,
        Unexpected,
        All = 0x7FFFFFFF
    }
}
