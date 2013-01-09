using RoutingAI.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.DataContracts;
using RoutingAI.ServiceContracts;
using System.Net; 
using libWyvernzora;
using System.Threading;
using libWyvernzora.Logging;  

namespace RoutingAI.Controller
{
    /// <summary>
    /// Planet Colonizer Routing Task
    /// </summary>
    public sealed class PCRoutingTask : IComputationTask<Solution>
    {
        const int DEFAULT_THREADS_PER_SLAVE = 4;
        const string TAG = "PCRoutingTask";

        private Random _rand = new Random();
        private Solution _result;
        private List<Pair<Guid, IRoutingAiSlaveService>> _threads = new List<Pair<Guid,IRoutingAiSlaveService>>();    // 1 entry per thread
        private List<SlaveConfig> _threadConfig = new List<SlaveConfig>();    // 1 entry per thread

        public Solution Result
        {
            get { return _result; }
        }


        public void Compute(params object[] args)
        {
            // Convert arguments and calculate info
            OptimizationRequest request = (OptimizationRequest)args[0];
            Int64 problemComplexity = request.Workers.Length * request.Tasks.Length;
            Int32 serverCount = 2 * (Int32)Math.Ceiling(Math.Log10(problemComplexity));

            // Allocate appropriate servers
            ServerResourceManager srvMgr = ServerResourceManager.Instance;
            IPEndPoint[] slaveIPs = srvMgr.GetSlaveServers(serverCount);
            IPEndPoint[] osrmIPs = srvMgr.GetOsrmServers(request.RegionCode);
            IPEndPoint[] redisIPs = srvMgr.GetRedisServers(request.RegionCode);
            IPEndPoint librIP = srvMgr.GetLibrarianServer();

            // Initialize slave threads
            for (int i = 0; i < slaveIPs.Length; i++)
            {
                IRoutingAiSlaveService proxy = ServiceProxyHelper.GetSlaveProxy(slaveIPs[i]);
                Pair<Int32, Int32> srvLoad = proxy.GetServerCapacityInfo();
                Int32 tCount = DEFAULT_THREADS_PER_SLAVE;
                if (tCount < srvLoad.Second - srvLoad.First)
                    tCount = srvLoad.Second - srvLoad.First;

                // Spawn threads on server
                for (int k = 0; k < tCount; k++)
                {
                    Guid id = proxy.CreateComputationThread();
                    _threads.Add(new Pair<Guid, IRoutingAiSlaveService>(id, proxy));
                }
            }
            
            // Create configuration for each of them
            for (int i = 0; i < _threads.Count; i++)
                _threadConfig.Add(new SlaveConfig(){
                     LibrarianServer = librIP,
                     OSRMServers = osrmIPs,
                     RedisServers = redisIPs,
                     SlaveIndex = new Pair<int,int>(i, _threads.Count),
                     RandomSeed = _rand.Next()
                    });


            // Do clustering
                // start computing
            for (int i = 0; i < _threads.Count; i++)
                _threads[i].Second.ComputeClusteringSolution(_threads[i].First, _threadConfig[i], request);
                // wait for thread 0 to complete
            while (_threads[0].Second.GetComputationThreadInfo(_threads[0].First).State == ComputationThreadState.Working) Thread.Sleep(1000);
                // here computation is either finished or there was an error
            ComputationThreadInfo info = _threads[0].Second.GetComputationThreadInfo(_threads[0].First);
            if (info.State == ComputationThreadState.Exception)
            {
                // there was an error, log it
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Expected, "Slave encountered an error while computing: {0}: JobID = {{{1}}}", info.AdditionalInfo, request.Id);
                return; // abort job, add more graceful handling later
            }

            ClusteringSolution clusteringSolution = _threads[0].Second.GetClusteringSolution(_threads[0].First);

            // Do optimization


            // Finish up

            _result = new Solution();
            _result.TaskSequences = new TaskSequence[request.Workers.Length];
            for (int i = 0; i < _result.TaskSequences.Length; i++ )
            {
                _result.TaskSequences[i] = new TaskSequence()
                {
                   OrderedTasks = clusteringSolution.Clusters[i].ToArray(),
                   Resources = new Resource[] { request.Workers[i] }
                };
            }
        }


        public void HandleAbort()
        {
            foreach (Pair<Guid, IRoutingAiSlaveService> thread in _threads)
            {
                thread.Second.DisposeComputationThread(thread.First);
            }
        }


        public object GetResult()
        {
            return _result;
        }


        public void Dispose()
        {
            foreach (Pair<Guid, IRoutingAiSlaveService> thread in _threads)
            {
                thread.Second.DisposeComputationThread(thread.First);
            }
        }
    }
}
