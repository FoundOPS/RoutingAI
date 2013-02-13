using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.ServiceContracts;
using System.ServiceModel;
using System.Net;

namespace RoutingAI.Controller
{
    /// <summary>
    /// Static class with utility methods for getting WCF service proxies
    /// </summary>
    public static class ServiceProxyHelper
    {
        /// <summary>
        /// Gets a RoutingAI Slave Service proxy for the specified endpoint
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        public static IRoutingAiSlaveService GetSlaveProxy(IPEndPoint ep)
        {
            EndpointAddress endpoint = new EndpointAddress(String.Format("http://{0}/RoutingAi/Slave", ep.ToString()));
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.MaxReceivedMessageSize = Int32.MaxValue;
            binding.MaxBufferSize = Int32.MaxValue;
            return ChannelFactory<IRoutingAiSlaveService>.CreateChannel(binding, endpoint);
        }
    }
}
