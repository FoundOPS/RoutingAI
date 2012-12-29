using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using RoutingAI.ServiceContracts;
using RoutingAI.DataContracts;
using System.Net;

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

            OptimizationRequest req = new OptimizationRequest()
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                Tasks = new RoutingAI.DataContracts.Task[] { },
                Workers = new Resource[] { }
            };
            SlaveConfig config = new SlaveConfig()
            {
                OptimizationRequest = req,
                OSRMServers = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse("192.168.2.31"), 5000) },
                RandomSeed = 11235813,
                RedisServers = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse("192.168.2.13"), 6379) }
            };
            Guid id = proxy.ConfigureComputationThread(config);
            Console.WriteLine(id);

            Console.WriteLine("Press <ENTER> to terminate...");
            Console.ReadLine();
        }
    }
}
