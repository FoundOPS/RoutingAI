using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    public enum Stage
    {
        Queued,
        Preprocessing,
        Processing,
        Completed
    }

    [DataContract(Name = "optimization_response", Namespace = "http://foundops.com/services/routing_ai")]
    public class OptimizationResponse
    {
        /// <summary>
        /// 1-100 percent progress of current stage
        /// (Only set in preprocessing stage)
        /// </summary>
        [DataMember(Name = "progress")]
        public UInt16? Progress { get; set; }

        /// <summary>
        /// The stage the request is in
        /// </summary>
        [DataMember(Name = "stage")]
        public Stage Stage { get; set; }

        /// <summary>
        /// The solution.
        /// Null unless the stage is completed
        /// </summary>
        [DataMember(Name = "solution")]
        public Solution Solution { get; set; }
    }
}
