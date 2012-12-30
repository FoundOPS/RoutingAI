using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "resource", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Resource
    {
        /// <summary>
        /// The date/time window the resource is available.
        /// If null it is always available.
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

        /// <summary>
        /// Costs involved in using this resource for 1 hour
        /// outside of its availability window
        /// </summary>
        [DataMember(Name = "cost_per_hour_overtime")]
        public Int32 CostPerHourOvertime { get; set; }
    }
}
