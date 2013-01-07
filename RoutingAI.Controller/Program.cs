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

            CallResponse response = proxy.StartComputingClusteringSolution(id, config, req);
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
