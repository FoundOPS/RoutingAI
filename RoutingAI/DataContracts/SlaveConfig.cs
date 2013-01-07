using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using libWyvernzora;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "slave_config", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class SlaveConfig
    {
        #region Controller --> Slave Configuration Data

        // Index of the slave/total number of slaves
        [DataMember(Name = "slave_index")]
        public Pair<Int32, Int32> SlaveIndex { get; set; }

        [DataMember(Name = "redis_servers")]
        public IPEndPoint[] RedisServers { get; set; }

        [DataMember(Name = "osrm_server")]
        public IPEndPoint[] OSRMServers { get; set; }

        [DataMember(Name = "librarian_server")]
        public IPEndPoint LibrarianServer { get; set; }

        [DataMember(Name = "rand_seed")]
        public Int32 RandomSeed { get; set; }

        #endregion
    }
}
