using System;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// A skill to complete a task
    /// </summary>
    public class Skill
    {
        /// <summary>
        /// The type of skill
        /// </summary>
        [DataMember(Name = "type")]
        public UInt32 Type { get; set; }

        /// <summary>
        /// The time efficiency on a scale of 0 - 10
        /// 0: cannot complete to 10: most efficient
        /// Has an affect on estimated task time
        /// </summary>
        [DataMember(Name = "efficiency")]
        public UInt32 Efficiency { get; set; }
    }
}