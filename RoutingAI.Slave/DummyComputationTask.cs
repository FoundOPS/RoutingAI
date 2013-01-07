using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.Threading;
using System.Threading;

namespace RoutingAI.Slave
{
    public class DummyComputationTask : IComputationTask
    {
        public void Compute(params object[] args)
        {
            Thread.Sleep(10000);
        }

        public void HandleAbort()
        {
            
        }

        public object GetResult()
        {
            return null;
        }

        public void Dispose()
        {
            
        }
    }
}
