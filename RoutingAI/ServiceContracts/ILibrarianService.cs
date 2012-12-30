using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using libWyvernzora;
using RoutingAI.DataContracts;

namespace RoutingAI.ServiceContracts
{
    [ServiceContract(Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public interface ILibrarianService
    {
        [OperationContract]
        void InitializeKMeans(Guid id);

        [OperationContract]
        void UpdateCATableEntry(Guid id, Int32 iteration, Int32 index, Pair<Task, Int32> data);

        [OperationContract]
        List<Pair<Int32, Int32>[]> GetCATable(Guid id);
    }
}
