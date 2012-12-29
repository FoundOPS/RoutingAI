using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// The span of dates a job can be completed in.
    /// Ex. From 1-1-2012 to 1-4-2012
    /// </summary>
    [DataContract(Name = "date_window", Namespace = "http://foundops.com/services/routing_ai")]
    public class DateWindow
    {
        /// <summary>
        /// An inclusive start date
        /// </summary>
        [DataMember(Name = "start_date")]
        public TimeSpan StartDate { get; set; }

        /// <summary>
        /// An inclusive end date
        /// </summary>
        [DataMember(Name = "end_date")]
        public TimeSpan EndDate { get; set; }
    }
}
