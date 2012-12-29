using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// The span of time a job can be completed in. 
    /// Ex. From 3pm to 8pm
    /// </summary>
    [DataContract(Name = "time_window", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class TimeWindow
    {
        /// <summary>
        /// An inclusive start time (the time of day)
        /// </summary>
        [DataMember(Name = "start_time")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// An inclusive end time (the time of day)
        /// </summary>
        [DataMember(Name = "end_time")]
        public TimeSpan EndTime { get; set; }
    }
}
