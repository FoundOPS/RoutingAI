using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using libWyvernzora;
using RoutingAI.DataContracts;


namespace RoutingAI.ServiceContracts
{
    [ServiceContract(Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public interface IRoutingAiService
    {
        /// <summary>
        /// Create a new optimization request
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>Id of the created task</returns>
        [OperationContract]
        Guid Post(OptimizationRequest request);

        /// <summary>
        /// Gets solution/status of a task
        /// </summary>
        /// <param name="id">Id of the task</param>
        /// <returns></returns>
        [OperationContract]
        OptimizationResponse Get(Guid id);

        /// <summary>
        /// Stop an optimization request
        /// </summary>
        /// <param name="id"></param>
        [OperationContract]
        void Delete(Guid id);
    }
}
