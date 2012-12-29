using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// A limiting date/time window
    /// </summary>
    [DataContract(Name = "window", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Window : ICloneable
    {
        /// <summary>
        /// The times it can be completed/is available
        /// If null there is no time restriction
        /// </summary>
        [DataMember(Name = "time_windows")]
        public TimeWindow[] TimeWindow { get; set; }

        /// <summary>
        /// The dates it can be completed/is available
        /// If null there is no date restriction
        /// </summary>\
        [DataMember(Name = "date_windows")]
        public DateWindow[] DateWindow { get; set; }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Window w = new Window();
            w.TimeWindow = new TimeWindow[this.TimeWindow.Length];
            Array.Copy(this.TimeWindow, w.TimeWindow, this.TimeWindow.Length);
            w.DateWindow = new DateWindow[this.DateWindow.Length];
            Array.Copy(this.DateWindow, w.DateWindow, this.DateWindow.Length);

            return w;
        }
    }
}
