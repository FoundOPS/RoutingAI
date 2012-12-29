using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Slave
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost svcHost = new ServiceHost(typeof(RoutingAiSlave), new Uri("http://localhost:8000/RoutingAi")))
            {
                svcHost.AddServiceEndpoint(typeof(RoutingAI.ServiceContracts.IRoutingAiSlaveService),
                    new BasicHttpBinding(), "Slave");

                svcHost.Open();

                Console.WriteLine("Press <ENTER> to terminate...");
                Console.ReadLine();
            }
        }
    }
}
