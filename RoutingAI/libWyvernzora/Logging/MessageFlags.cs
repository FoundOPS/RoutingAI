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
        Trivial = 1,
        Verbose = 2,
        Routine = 4,
        Default = 8,
        Warning = 16,
        Error = 32,
        Critical = 64,
        Fatal = 128,
        Expected = 256,
        Unexpected = 512,
        All = 0x7FFFFFFF
    }
}
