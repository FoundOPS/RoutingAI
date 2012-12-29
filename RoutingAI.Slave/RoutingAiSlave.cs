using libWyvernzora;
using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libWyvernzora.Logging;

namespace RoutingAI.Slave
{
    class RoutingAiSlave : IRoutingAiSlaveService
    {
        #region Server

        public string Ping()
        {
            return "Hi FoundOPS!";
        }

        public libWyvernzora.Pair<int, int> GetServerCapacityInfo()
        {
            return new Pair<Int32, Int32>(0, 0);
        }

        #endregion

        #region Computation Threads

        public Guid ConfigureComputationThread(DataContracts.SlaveConfig conf)
        {
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial, "Configuration Received:");
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial,  "Id = {0}", conf.OptimizationRequest.Id);
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial,  "ClientId = {0}", conf.OptimizationRequest.ClientId);
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial,  "RedisServers = {0}", String.Join<System.Net.IPEndPoint>(", ", conf.RedisServers));
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial, "OsrmServers = {0}", String.Join<System.Net.IPEndPoint>(", ", conf.OSRMServers));
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial,  "RandomSeed = {0}", conf.RandomSeed);
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial,  "TaskCount = {0}", conf.OptimizationRequest.Tasks.Length);
            GlobalLogger.SendLogMessage("RoutingAiSlave", MessageFlags.Trivial, "WorkerCount = {0}", conf.OptimizationRequest.Workers.Length);

            return Guid.NewGuid();
        }

        public void StartComputationThread(Guid tid)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
