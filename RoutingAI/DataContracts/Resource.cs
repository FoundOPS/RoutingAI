using System;
using System.Runtime.Serialization;

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
        /// That Latitude where the resource starts
        /// </summary>
        [DataMember(Name = "origin_lat")]
        public Decimal OriginLatitude { get; set; }

        /// <summary>
        /// That Longitude where the resource starts
        /// </summary>
        [DataMember(Name = "origin_lon")]
        public Decimal OriginLongitude { get; set; }

        /// <summary>
        /// If provided, defines the latitude of the end-destination for this Resource.
        /// If not specified, the route calculated for this Resource terminates with the last visited Task.
        /// </summary>
        [DataMember(Name = "dest_lat")]
        public Decimal? DestinationLatitude { get; set; }

        /// <summary>
        /// If provided, defines the destination longitude.
        /// </summary>
        [DataMember(Name = "dest_lon")]
        public Decimal? DestinationLongitude { get; set; }

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
        public UInt32[] Skills { get; set; }

        /// <summary>
        /// Costs involved in moving this resource for 1 mile
        /// </summary>
        [DataMember(Name = "cost_per_mile")]
        public Int32 CostPerMile { get; set; }

        /// <summary>
        /// Costs involved in using this resource for 1 hour
        /// </summary>
        [DataMember(Name = "cost_per_hour")]
        public Int32 CostPerHour { get; set; }
    }
}
