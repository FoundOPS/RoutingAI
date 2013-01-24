using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "clustering_solution", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class ClusteringSolution : IComparable<ClusteringSolution>
    {
        [DataMember(Name = "clusters")]
        public Task[][] Clusters { get; set; }

        [DataMember(Name = "distance")]
        public Int32 Distance { get; set; }

        #region IComparable Members

        public int CompareTo(ClusteringSolution other)
        {
            return this.Distance.CompareTo(other.Distance);
        }

        #endregion

    }
}
