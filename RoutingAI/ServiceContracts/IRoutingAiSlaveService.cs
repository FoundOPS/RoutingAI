using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using libWyvernzora;
using RoutingAI.DataContracts;

namespace RoutingAI.ServiceContracts
{
    [ServiceContract(Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public interface IRoutingAiSlaveService
    {
        #region Communications with Server

        /* Methods here are for communication with slave server instance
         * instead of individual computation threads. */

        [OperationContract]
        String Ping();

        [OperationContract]
        Pair<Int32, Int32> GetServerCapacityInfo();

        #endregion

        #region Computation Thread Manipulation

        /* Methods here are for controlling individual computation threads */

        [OperationContract]
        Guid ConfigureComputationThread(SlaveConfig conf);

        [OperationContract]
        void StartComputationThread(Guid tid);

        #endregion

    }
}
