using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RoutingAI.DataContracts
{
    [DataContract(Name = "response", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class CallResponse
    {
        [DataMember(Name = "success")]
        public Boolean Success { get; set; }

        [DataMember(Name = "details")]
        public String Details { get; set; }
    }
}
