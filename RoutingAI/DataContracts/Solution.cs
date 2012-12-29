using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// An ordered set of tasks assigned to a worker
    /// </summary>
    [DataContract(Name = "task_sequence", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class TaskSequence
    {
        /// <summary>
        /// Tasks in order
        /// </summary>
        [DataMember(Name = "tasks")]
        Task[] OrderedTasks { get; set; }

        /// <summary>
        /// The assigned resources
        /// </summary>
        [DataMember(Name = "resources")]
        Resource[] Resources { get; set; }
    }

    /// <summary>
    /// An optimization solution
    /// </summary>
    [DataContract(Name = "solution", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Solution
    {
        [DataMember(Name = "task_sequences")]
        public TaskSequence[] TaskSequences { get; set; }
    }
}
