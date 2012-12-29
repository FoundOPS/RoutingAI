using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.ServiceContracts
{
    [ServiceContract(Namespace = "http://foundops.com/services/routing_ai")]
    public interface IRoutingAiSlaveService
    {
        [OperationContract]
        String Ping();
    }
}
