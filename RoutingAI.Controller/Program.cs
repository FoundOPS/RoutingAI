using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using RoutingAI.ServiceContracts;
using RoutingAI.DataContracts;

namespace RoutingAI.Controller
{
    class Program
    {
        static void Main(string[] args)
        {
            EndpointAddress endpoint = new EndpointAddress("http://localhost:8000/RoutingAi/Slave");

            IRoutingAiSlaveService proxy = ChannelFactory<IRoutingAiSlaveService>.CreateChannel(new BasicHttpBinding(), endpoint);

            String message = proxy.Ping();

            Console.WriteLine(message);
            Console.WriteLine("Press <ENTER> to terminate...");
            Console.ReadLine();
        }
    }
}
