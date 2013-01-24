using System;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "optimization_request", Namespace = "http://foundops.com/services/routing_ai/1.0")]
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
        /// The tasks to be completed
        /// </summary>
        [DataMember(Name = "tasks")]
        public Task[] Tasks { get; set; }

        /// <summary>
        /// The resources to assign to routes
        /// </summary>
        [DataMember(Name = "resources")]
        public Resource[] Resources { get; set; }

        /// <summary>
        /// Region code specifying which routing servers/cache servers to use
        /// </summary>
        [DataMember(Name = "region")]
        public String RegionCode { get; set; }

        /// <summary>
        /// Overall target optimization window
        /// It may be a day, a week, a month or whatever you need
        /// Tasks that fall out side this window will be considered "incomplete"
        /// </summary>
        [DataMember(Name = "window")]
        public Window Window { get; set; }

        /// <summary>
        /// Removes Task data from the OptimizationRequest object.
        /// Use it after gathering clustering solution.
        /// </summary>
        public void StripData()
        {
            Tasks = new Task[0];
        }
    }
}
