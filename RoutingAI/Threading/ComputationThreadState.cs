using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Threading
{
    public enum ComputationThreadState
    {
        Ready,      // thread is ready for the next command
        Working, // thread is computing  stuff and rejects all commands except "kill"
        Finished,   // thread is done computing last command and is waiting for the new command
        Exception,  // thread has encountered an error
        Dead        // thread is dead (either does not exist or was terminated or threadstate is not running)
    }
}
