using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "optimization_request", Namespace = "http://foundops.com/services/routing_ai")]
    public class OptimizationRequest
    {
        /// <summary>
        /// A unique Id for the request
        /// </summary>
        [DataMember(Name = "id")]
        public Guid Id { get; set; }

        /// <summary>
        /// A unique Id for the client that request belongs to
        /// </summary>
        [DataMember(Name = "client_id")]
        public Guid ClientId { get; set; }

        /// <summary>
        /// The dates & times a job can be done
        /// </summary>
        [DataMember(Name = "window")]
        public Window Window { get; set; }

        /// <summary>
        /// The tasks to be completed
        /// </summary>
        [DataMember(Name = "tasks")]
        public Task[] Tasks { get; set; }

        /// <summary>
        /// The workers to assign to routes
        /// </summary>
        [DataMember(Name = "workers")]
        public Resource[] Workers { get; set; }
    }
}
