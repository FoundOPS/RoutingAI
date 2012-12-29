using libWyvernzora.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Slave
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Initialize Logger

            Logger console = new ConsoleLogger();
            Logger logfile = new PlainTextLogger("debug.log", false);

            GlobalLogger.AttachLogger(console);
            GlobalLogger.AttachLogger(logfile);

            console.Run();
            logfile.Run();

            #endregion

            #region Initialize Server
            try
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
            catch (HttpListenerException x)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Failed to start server host: {0}", x.Message);
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal,  "Please make sure RoutingAI.Slave is running as admin.");
            }
            catch (Exception x)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Unexpected error: {0}", x.Message);
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Stack Dump: {0}", x.StackTrace);
            }
            #endregion

            console.Stop();
            logfile.Stop();
        }
    }
}
