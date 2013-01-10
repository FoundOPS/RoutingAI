﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using RoutingAI.ServiceContracts;
using RoutingAI.DataContracts;
using System.Net;
using libWyvernzora.Logging;
using libWyvernzora.ConsoleUtil;


namespace RoutingAI.Controller
{
    class Program
    {
        const String TAG = "RoutingAI.Ctrller";

        static void Main()
        {
            Console.BufferWidth = 120;
            Console.Title = "RoutingAI.Controller © 2012-2013, FoundOPS LLC.";
            Console.ForegroundColor = ConsoleColor.White;

            Console.Clear();
            Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
            Console.WriteLine("RoutingAI.Controller Server Software");
            Console.WriteLine("Copyright (C) 2012-2013, FoundOPS LLC");
            Console.WriteLine("-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-\n\n");

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

            String configFile = "config.xml";   // default path of configuration file

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
                        case "verbose":
                            console.RejectFlags = MessageFlags.None;
                            break;
                        case "configfile":
                            if (arg.Arguments.Length < 1) break;
                            else configFile = arg.Arguments[0];
                            break;
                    }
                }
                else
                {
                    // Process arguments here
                }

            }

            #endregion

            #region Initialize ServerResourceManager

            GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "Initializing Server Resource Manager...");
            try
            {
                ServerResourceManager.InitializeFromConfig(configFile);
            }
            catch (Exception ex)
            {   // configuration cant be loaded. Log problem and shut down controller
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Fatal | MessageFlags.Expected, "Failed to load config file: {0} - {1}", ex.GetType().FullName, ex.Message);
                return;
            }

            #endregion

            #region Initialize Service

            try
            {
                GlobalLogger.SendLogMessage("RoutingAI.Ctrller", MessageFlags.Routine, "Starting WCF Service...");
                using (ServiceHost svcHost = new ServiceHost(typeof(RoutingAi), new Uri("http://localhost:8000/RoutingAi")))
                {
                    svcHost.AddServiceEndpoint(typeof(RoutingAI.ServiceContracts.IRoutingAiService),
                        new BasicHttpBinding(), "Controller");

                    svcHost.Open();

                    GlobalLogger.SendLogMessage("RoutingAI.Ctrller", MessageFlags.Routine, "WCF Service Started Successfully");

                    Console.ReadLine();
                }
            }
            catch (HttpListenerException x)
            {
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Failed to start server host: {0}", x.Message);
                GlobalLogger.SendLogMessage("Critical", MessageFlags.Fatal, "Please make sure RoutingAI.Controller is running as admin.");
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
        
        static void TestSlave()
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
                Resources = new Resource[] { }
            };
            SlaveConfig config = new SlaveConfig()
            {
                OSRMServers = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse("192.168.2.31"), 5000) },
                RandomSeed = 11235813,
                RedisServers = new IPEndPoint[] { new IPEndPoint(IPAddress.Parse("192.168.2.13"), 6379) }
            };
            Guid id = proxy.CreateComputationThread();
            Console.WriteLine(id);

            Console.Write("Press <ENTER> to continue..."); Console.ReadLine();

            ComputationThreadInfo info = proxy.GetComputationThreadInfo(id);
            Console.WriteLine("ThreadInfo {{ID = {{{2}}}, State = {0}, AcceptsCommands = {1}, Info = {3}}}", info.State, info.AcceptsCommands, info.ThreadId, info.AdditionalInfo);

            Console.Write("Press <ENTER> to continue..."); Console.ReadLine();

            proxy.DisposeComputationThread(new Guid());

            Console.Write("Press <ENTER> to continue..."); Console.ReadLine();

            CallResponse response = proxy.ComputeClusteringSolution(id, config, req);
            Console.WriteLine("CallResponse {{ Success = {0}, Details = {1}}}", response.Success, response.Details);
            info = proxy.GetComputationThreadInfo(id);
            Console.WriteLine("ThreadInfo {{ID = {{{2}}}, State = {0}, AcceptsCommands = {1}, Info = {3}}}", info.State, info.AcceptsCommands, info.ThreadId, info.AdditionalInfo);

            Console.Write("Press <ENTER> to continue..."); Console.ReadLine();

            Boolean flag = true;

            while (flag)
            {
                info = proxy.GetComputationThreadInfo(id);
                Console.WriteLine("ThreadInfo {{ID = {{{2}}}, State = {0}, AcceptsCommands = {1}, Info = {3}}}", info.State, info.AcceptsCommands, info.ThreadId, info.AdditionalInfo);

                Console.Write("Press <ENTER> to continue..."); Console.ReadLine();

                response = proxy.DisposeComputationThread(id);

                if (info.AcceptsCommands || !response.Success) flag = false;

                if (!response.Success)
                {
                    Console.WriteLine("Failed to kill the thread: {0}", response.Details);
                }
            }


            Console.WriteLine("Press <ENTER> to terminate...");
            Console.ReadLine();

        }
    }
}
