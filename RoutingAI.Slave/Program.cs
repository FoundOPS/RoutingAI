using libWyvernzora.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using libWyvernzora.ConsoleUtil;    

namespace RoutingAI.Slave
{
    class Program
    {
        static void Main()
        {
            Console.BufferWidth = 120;
            Console.ForegroundColor = ConsoleColor.White;

            #region Initialize Logger

            Logger console = new ConsoleLogger();
            Logger logfile = new PlainTextLogger("debug.log", false);

            // Make console logger reject all trivial and verbose messages
            console.RejectFlags = MessageFlags.Trivial | MessageFlags.Verbose;

            GlobalLogger.AttachLogger(console);
            GlobalLogger.AttachLogger(logfile);

            console.Run();
            logfile.Run();

            #endregion

            #region Process Command Line Arguments

            CommandLineArguments args = new CommandLineArguments(); // handy utility for parsing command line input
            for (int i = 0; i < args.Count; i++)
            {
                CommandLineArgument arg = args[i];
                String name = arg.Name.ToLower(); // make it case insensitive

                if (arg.Type == CommandLineArgument.ArgumentType.Option)
                {
                    // Process options here
                    switch (name)
                    {
                        case "version":     // print version info and such
                            Console.WriteLine("RoutingAI.Slave Server\n-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-\nCopyright (C) 2012-2013, FoundOPS LLC.");
                            break;
                        case "bw":
                            {
                                if (arg.Arguments.Length < 1) break;
                                Int32 bw;
                                if (!Int32.TryParse(arg.Arguments[0], out bw)) break;
                                Console.BufferWidth = bw;
                            }
                            break;
                        case "idletimeout":
                            {
                                if (arg.Arguments.Length < 1) break;
                                Int32 timeout;
                                if (!Int32.TryParse(arg.Arguments[0], out timeout)) break;
                                RoutingAI.Threading.ComputationThreadDispatcher.Instance.IdleThreadTimeout = timeout;
                            }
                            break;
                        case "verbose":
                            console.RejectFlags = MessageFlags.None;
                            break;
                    }
                }
                else
                {
                    // Process arguments here
                }

            }

            #endregion
          
            #region Initialize Server

            try
            {
                GlobalLogger.SendLogMessage("RoutingAI.Slave", MessageFlags.Routine, "Starting WCF Service...");
                using (ServiceHost svcHost = new ServiceHost(typeof(RoutingAiSlave), new Uri("http://localhost:8000/RoutingAi")))
                {
                    svcHost.AddServiceEndpoint(typeof(RoutingAI.ServiceContracts.IRoutingAiSlaveService),
                        new BasicHttpBinding(), "Slave");

                    svcHost.Open();

                    GlobalLogger.SendLogMessage("RoutingAI.Slave", MessageFlags.Routine, "WCF Service Started Successfully");

                    //Console.WriteLine("Press <ENTER> to terminate...");
                    Console.ReadLine();
                }
            }
            catch (HttpListenerException x)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Failed to start server host: {0}", x.Message);
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Please make sure RoutingAI.Slave is running as admin.");
                Console.WriteLine("Press <ENTER> to terminate...");
                Console.ReadLine();
            }
            catch (Exception x)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Unexpected error: {0}", x.Message);
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Stack Dump: {0}", x.StackTrace);
                Console.WriteLine("Press <ENTER> to terminate...");
                Console.ReadLine();
            }
            finally
            {
                console.Stop();
                logfile.Stop();
            }

            #endregion

        }
    }
}
