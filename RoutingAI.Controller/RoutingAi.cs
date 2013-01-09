using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RoutingAI.Threading;
using RoutingAI.DataContracts;

namespace RoutingAI.Controller
{
    public class RoutingAi : IRoutingAiService
    {
        ComputationThreadDispatcher _dispatcher = ComputationThreadDispatcher.Instance;
        
        public CallResponse Post(OptimizationRequest request)
        {
            throw new NotImplementedException();
        }

        public OptimizationResponse Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public CallResponse Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
