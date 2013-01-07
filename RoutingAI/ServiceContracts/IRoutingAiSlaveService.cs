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
        #region Communications with Server Framework

        /* Methods here are for communication with slave server instance
         * instead of individual computation threads. */

        [OperationContract]
        String Ping();

        [OperationContract]
        Pair<Int32, Int32> GetServerCapacityInfo();

        #endregion

        #region Computation Thread Manipulation

        /* Methods here are for controlling individual computation threads */

        // Starts a computation thread and puts it into ready state
        [OperationContract]
        Guid ConfigureComputationThread(SlaveConfig conf);

        // Gets current status of a  computation thread
        [OperationContract]
        ComputationThreadInfo GetComputationThreadInfo(Guid threadId);

        // Stops computation in progress
        [OperationContract]
        CallResponse AbortComputation(Guid threadId);

        // Stops all computation, removes the thread and releases all resources
        [OperationContract]
        CallResponse KillComputationThread(Guid threadId); 

        #endregion

        #region Clustering

        // Starts calculating clustering solution
        [OperationContract]
        CallResponse StartComputingClusteringSolution(Guid threadId, OptimizationRequest data);
        
        #endregion

    }
}
