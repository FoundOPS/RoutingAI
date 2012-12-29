using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using libWyvernzora;

namespace RoutingAI.ServiceContracts
{
    [ServiceContract(Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public interface ILibrarianService
    {
        [OperationContract]
        void InitializeKMeans(Guid id, Pair<Int32, Int32>[] initCA);

        [OperationContract]
        void InsertCATableRow(Guid id, Pair<Int32, Int32>[] row);

        [OperationContract]
        List<Pair<Int32, Int32>[]> GetCATable(Guid id);
    }
}
