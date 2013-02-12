using System;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// A task with an estimated arrival / departure
    /// </summary>
    [DataContract(Name = "task_sequence", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Destination
    {
        /// <summary>
        /// The task
        /// </summary>
        [DataMember(Name = "tasks")]
        public Task Task { get; set; }

        /// <summary>
        /// The estimated arrival
        /// </summary>
        [DataMember(Name = "est_arrival")]
        public DateTime EstimatedArrival { get; set; }

        /// <summary>
        /// The estimated departure
        /// </summary>
        [DataMember(Name = "est_departure")]
        public DateTime EstimatedDeparture { get; set; }
    }

    /// <summary>
    /// An ordered set of tasks assigned to a resource
    /// </summary>
    [DataContract(Name = "task_sequence", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Route
    {
        /// <summary>
        /// The assigned resource
        /// </summary>
        [DataMember(Name = "resource")]
        public Resource Resource { get; set; }

        /// <summary>
        /// Destinations ordered by arrival
        /// </summary>
        [DataMember(Name = "destinations")]
        public Destination[] Destinations { get; set; }

        /// <summary>
        /// Cost of the route
        /// </summary>
        [DataMember(Name = "cost")]
        public Cost Cost { get; set; }

        /// <summary>
        /// Tasks that have been assigned to the cluster but were
        /// not scheduled
        /// </summary>
        [DataMember(Name = "incomplete")]
        public Task[] Incomplete { get; set; }
    }

    /// <summary>
    /// An optimization solution
    /// </summary>
    [DataContract(Name = "solution", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Solution
    {
        /// <summary>
        /// The set of routes
        /// </summary>
        [DataMember(Name = "routes")]
        public Route[] Routes { get; set; }

        /// <summary>
        /// Incomplete tasks are the ones optimized decided to leave out of
        /// schedule for such reasons as task falling outside of availability window
        /// </summary>
        [DataMember(Name = "incomplete")]
        public Task[] Incomplete { get; set; }
    }
}
