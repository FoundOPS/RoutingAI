using System;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// A limiting date & time window
    /// </summary>
    [DataContract(Name = "window", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Window : ICloneable
    {
        /// <summary>
        /// The start date and time of the window
        /// </summary>
        [DataMember(Name = "start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// The end date and time of the window
        /// </summary>
        [DataMember(Name = "end")]
        public DateTime End { get; set; }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Window w = new Window();
            w.Start = this.Start;
            w.End = this.End;
            return w;
        }
    }
}
