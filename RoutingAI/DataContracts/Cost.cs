using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using RoutingAI.Algorithms;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "cost", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public struct Cost : IComparable<Cost>
    {
        /// <summary>
        /// Distance involved in cost calculation
        /// </summary>
        [DataMember(Name = "distance")]
        public Int32 Distance { get; set; }
        /// <summary>
        /// Time involved in cost calculation
        /// </summary>
        [DataMember(Name = "time")]
        public Int32 Time { get; set; }
        /// <summary>
        /// Overtime involved in cost calculation
        /// </summary>
        [DataMember(Name = "overtime")]
        public Int32 Overtime { get; set; }
        /// <summary>
        /// Overall cost score used for routing
        /// Actual calculation depends on algorithm
        /// </summary>
        [DataMember(Name = "overall_cost")]
        public Int32 OverallCost { get; set; }

        public int CompareTo(Cost other)
        {
            return this.OverallCost.CompareTo(other.OverallCost);
        }

        public static Cost operator -(Cost lhs, Cost rhs) {

            Cost tmp = new Cost()
            {
                Distance = lhs.Distance - rhs.Distance,
                Time = lhs.Time - rhs.Time,
                Overtime = lhs.Overtime - rhs.Overtime,
                OverallCost = lhs.OverallCost - rhs.OverallCost
            };

            return tmp;
        }
        public static Cost operator +(Cost lhs, Cost rhs)
        {
            Cost tmp = new Cost()
            {
                Distance = lhs.Distance + rhs.Distance,
                Time = lhs.Time + rhs.Time,
                Overtime = lhs.Overtime + rhs.Overtime,
                OverallCost = lhs.OverallCost + rhs.OverallCost
            };

            return tmp;
        }
    }
}
