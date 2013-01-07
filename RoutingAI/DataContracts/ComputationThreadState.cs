using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    public enum ComputationThreadState
    {
        Ready,      // thread is ready for the next command
        Clustering, // thread is currently computing clusters and rejects all commands except "kill"
        Optimizing, // thread is computing optimizations and rejects all commands except "kill"
        Finished,   // thread is done computing last command and is waiting for the new command
        Exception,  // thread has encountered an error
        Dead        // thread is dead (either does not exist or was terminated or threadstate is not running)
    }

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
    }
}
