using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "resource", Namespace = "http://foundops.com/services/routing_ai")]
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
    }
}
