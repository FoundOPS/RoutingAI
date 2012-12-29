using libWyvernzora.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Librarian
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Initialize Server
            try
            {
                using (ServiceHost svcHost = new ServiceHost(typeof(LibrarianService), new Uri("http://localhost:8000/RoutingAi")))
                {
                    svcHost.AddServiceEndpoint(typeof(RoutingAI.ServiceContracts.ILibrarianService),
                        new BasicHttpBinding(), "Librarian");

                    svcHost.Open();

                    Console.WriteLine("Press <ENTER> to terminate...");
                    Console.ReadLine();
                }
            }
            catch (HttpListenerException x)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Failed to start server host: {0}", x.Message);
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Please make sure RoutingAI.Librarian is running as admin.");
            }
            catch (Exception x)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Unexpected error: {0}", x.Message);
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Stack Dump: {0}", x.StackTrace);
            }
            #endregion
        }
    }
}
