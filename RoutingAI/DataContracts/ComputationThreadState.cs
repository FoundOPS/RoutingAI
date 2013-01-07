using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using RoutingAI.Threading;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "optimization_request", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class ComputationThreadInfo
    {
        [DataMember(Name = "thread_id")]
        public Guid ThreadId { get; set; }

        [DataMember(Name = "status")]
        public ComputationThreadState State { get; set; }

        [DataMember(Name = "info")]
        public String AdditionalInfo { get; set; }

        [DataMember(Name = "accepts_commands")]
        public Boolean AcceptsCommands { get; set; }

        public override string ToString()
        {
            // TODO a better string representation
            return base.ToString();
        }
    }
}
