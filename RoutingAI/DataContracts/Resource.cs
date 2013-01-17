using System;
using System.Runtime.Serialization;
using RoutingAI.API.OSRM;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// A resource for completing tasks
    /// Ex. 3 employees and 1 vehicle
    /// </summary>
    [DataContract(Name = "resource", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Resource
    {
        /// <summary>
        /// Id of the resource
        /// </summary>
        [DataMember(Name = "resource_id")]
        public UInt32 Id { get; set; }

        /// <summary>
        /// That Origin where the resource starts
        /// </summary>
        [DataMember(Name = "origin")]
        public Coordinate Origin { get; set; }

        /// <summary>
        /// If provided, defines the end-destination for this Resource.
        /// If null, the route calculated for this Resource terminates with the last visited Task.
        /// </summary>
        [DataMember(Name = "dest")]
        public Coordinate Destination { get; set; }

        /// <summary>
        /// When the resource is available
        /// Take into account the service time at the last visited Task and, if a destination is specified, the driving time to get there
        /// NOTE: Overtime is not allowed yet
        /// </summary>
        [DataMember(Name = "availability")]
        public Window Availability { get; set; }

        /// <summary>
        /// Skills a resource has to complete a job
        /// </summary>
        [DataMember(Name = "skills")]
        public Skill[] Skills { get; set; }

        /// <summary>
        /// Costs involved in moving this resource for 1 mile in cents
        /// </summary>
        [DataMember(Name = "cost_per_mile")]
        public UInt32 CostPerMile { get; set; }

        /// <summary>
        /// Costs involved in using this resource for 1 hour in cents
        /// </summary>
        [DataMember(Name = "cost_per_hour")]
        public UInt32 CostPerHour { get; set; }
    }
}
