using libWyvernzora;
using libWyvernzora.Logging;
using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.ServiceModel;
using System.Threading;

namespace RoutingAI.Controller
{
    /// <summary>
    /// Class that manages all servers used by RoutingAI.
    /// </summary>
    /// <remarks>
    /// ServerResourceManager periodically pings all servers and marks them
    /// inactive and excludes them from available servers.
    /// Since this class detects problems in the entire server infrastructure,
    /// its log entries are most likely marked with Routine, Critical or Fatal.
    /// </remarks>
    public class ServerResourceManager
    {
        /// <summary>
        /// Represents a server managed by ServerResourceManager
        /// </summary>
        private class ServerInfo
        {           
            // Properties
            public IPEndPoint Endpoint { get; set; }
            public Boolean IsResponsive { get; set; }
            public Int32 PingDelay { get; set; }
            public String Tag { get; set; }

            // Constructor
            /// <summary>
            /// Constructor
            /// Creates a ServerInfo object
            /// </summary>
            /// <param name="ep"></param>
            public ServerInfo(IPEndPoint ep)
            {
                Endpoint = ep;
                Update();
            }

            // Methods
            /// <summary>
            /// Updates server ping delay.
            /// </summary>
            public virtual void Update()
            {
                // Update Ping Time
                PingReply reply = (new Ping()).Send(Endpoint.Address, ServerResourceManager._pingTimeout);
                if (reply.Status != IPStatus.Success)
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Expected, "ServerInfo.Update: Ping Failed (Status = {0}): {1}", reply.Status.ToString(), Endpoint);
                    PingDelay = Int32.MaxValue;
                    IsResponsive = false;
                }
                else
                {
                    PingDelay = (int)reply.RoundtripTime;
                    IsResponsive = true;
                }
            }
        }
        /// <summary>
        /// Represents a RoutingAI.Slave server managed by ServerResourceManager
        /// </summary>
        private class SlaveServerInfo : ServerInfo
        {
            // Fields
            private IRoutingAiSlaveService proxy = null;

            // Properties
            public Int32 RemainingCapacity { get; set; }

            // COnstructor
            public SlaveServerInfo(IPEndPoint ep)
                : base(ep)
            { }

            public override void Update()
            {
                base.Update();

                // if the server is not responding, no need to check further
                if (!IsResponsive) return;

                // if proxy is not initialized, initialize it
                if (proxy == null)
                    proxy = ServiceProxyHelper.GetSlaveProxy(Endpoint);

                // Try get server load status
                try
                {
                    Pair<Int32, Int32> loadInfo = proxy.GetServerCapacityInfo();
                    RemainingCapacity = loadInfo.Second - loadInfo.First;
                }
                catch (EndpointNotFoundException ex)
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Expected,
                        "SlaveServerInfo.Update: RoutingAI.Slave service not found: {0}", Endpoint);
                    IsResponsive = false;
                }

            }
        }

        private const String TAG = "SrvMgr";
        private const Int32 MAINTAIN_INTERVAL = 5000;  // server info updating interval

        #region Singleton

        private static ServerResourceManager _instance = null;
        /// <summary>
        /// Gets the global instance of ServerResourceManager
        /// </summary>
        public static ServerResourceManager Instance
        {
            get
            {
                if (_instance == null) _instance = new ServerResourceManager();
                return _instance;
            }
        }
        /// <summary>
        /// Initializes global instance of ServerResourceManager
        /// from a configuration file
        /// </summary>
        /// <param name="file">Path to configuration file</param>
        public static void InitializeFromConfig(String file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException("Configuration file not found: " + file);

            if (_instance != null)
                throw new InvalidOperationException("Global ServerResourceManager already initialized!");

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(file);

            _instance = new ServerResourceManager(xdoc);
        }

        
        #endregion

        private static Int32 _pingTimeout = 5000;  // ping timeout

        // Servers sorted by ping values
        private List<SlaveServerInfo> _slaveServers;
        private List<ServerInfo> _librarianServers;

        // Servers sorted by region
        private List<ServerInfo> _osrmServers;
        private List<ServerInfo> _redisServers;

        

        /// <summary>
        /// Constructor.
        /// Creates an empty instance.
        /// </summary>
        protected ServerResourceManager()
        {
            _slaveServers = new List<SlaveServerInfo>();
            _librarianServers = new List<ServerInfo>();

            _osrmServers = new List<ServerInfo>();
            _redisServers = new List<ServerInfo>();
        }

        /// <summary>
        /// Constructor.
        /// Creates an instance and populates it with servers from
        /// configuration file.
        /// </summary>
        /// <param name="config"></param>
        protected ServerResourceManager(XmlDocument config)
            : this()
        {
            // Add Slave Servers
            GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "Adding Slave Servers from Configuration File...");
            foreach (XmlNode node in config.SelectNodes("RoutingAiConfig/SlaveServers/Endpoint"))
            {
                IPEndPoint ep = ParseIPEndPoint(node.InnerText);
                AddSlaveServer(ep);
            }

            // Add OSRM Servers
            GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "Adding OSRM Servers from Configuration File...");
            foreach (XmlNode node in config.SelectNodes("RoutingAiConfig/OSRMServers/Endpoint"))
            {
                String region = node.Attributes["region"].InnerText;
                IPEndPoint ep = ParseIPEndPoint(node.InnerText);
                AddOsrmServer(region, ep);
            }

            // Add Redis Servers
            GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "Adding Redis Servers from Configuration File...");
            foreach (XmlNode node in config.SelectNodes("RoutingAiConfig/RedisServers/Endpoint"))
            {
                String region = node.Attributes["region"].InnerText;
                IPEndPoint ep = ParseIPEndPoint(node.InnerText);
                AddRedisServer(region, ep);
            }

            // Librarian servers not implemented


            // Start Maintaining Server List
            _maintain = new Thread(new ThreadStart(MaintainServerResources));
            _maintain.Start();
        }

        #region Add/Remove Servers


        /// <summary>
        /// Adds a slave server to the manager.
        /// Note: depending on network status, you may expect this method to block executing thread
        /// for up to _pingDelay milliseconds.
        /// </summary>
        /// <param name="ep">IP EndPoint of the server</param>
        public void AddSlaveServer(IPEndPoint ep)
        {
            try
            {
                SlaveServerInfo info = new SlaveServerInfo(ep);

                lock (_slaveServers)
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddSlaveServer: Mutex Lock Acquired: {0}", ep);
                    _slaveServers.Add(info);
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "AddSlaveServer: Success: {0}", ep);
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddSlaveServer: Releasing Mutex Lock: {0}", ep);
                }
            }
            catch (Exception ex)
            {
                // Unexpected exception
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Unexpected,
                    "AddSlaveServer: Unexpected Exception: {{Endpoint = {0}; ExceptionType = {1}; Message = {2}; Stack = {3}", ep, ex.GetType().FullName, ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Removes a slave server from the manager.
        /// Note: removing a server means it will not be assigned to any job,
        /// but doesn't mean that the slave is released by jobs it's already
        /// assigned to.
        /// </summary>
        /// <param name="ep">Server Endpoint</param>
        public void RemoveSlaveServer(IPEndPoint ep)
        {
            lock (_slaveServers)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveSlaveServer: Mutex Lock Acquired: {0}", ep);
                Int32 n = _slaveServers.RemoveAll(new Predicate<SlaveServerInfo>((SlaveServerInfo p) => p.Endpoint.Equals(ep)));
                if (n == 0) // no items removed.. it's not supposed to be like that
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "RemoveSlaveServer: EndPoint not found: {0}", ep);
                    return;
                }
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RemoveSlaveServer: Success: {0}", ep);
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveSlaveServer: Releasing Mutex Lock: {0}", ep);
            }
        }

        // Librarian servers are not implemented at the moment
        public void AddLibrarianServer(IPEndPoint ep)
        {
            //throw new NotImplementedException();
        }

        public void RemoveLibrarianServer(IPEndPoint ep)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a OSRM server to the manager.
        /// Note: depending on network status, you may expect this method to block executing thread
        /// for up to _pingDelay milliseconds.
        /// </summary>
        /// <param name="region">Region code of the server</param>
        /// <param name="ep">IP EndPoint of the server</param>
        public void AddOsrmServer(String region, IPEndPoint ep)
        {
            try
            {
                // Ping Osrm
                ServerInfo info = new ServerInfo(ep);
                info.Tag = region;

                lock (_osrmServers)
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddOsrmServer: Mutex Lock Acquired: {0}", ep);
                    _osrmServers.Add(info);
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "AddOsrmServer: Success: {0}", ep);
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddOsrmServer: Releasing Mutex Lock: {0}", ep);
                }
                
            }
            catch (Exception ex)
            {
                // Unexpected exception
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Unexpected,
                    "AddOsrmServer: Unexpected Exception: {{Endpoint = {0}; ExceptionType = {1}; Message = {2}; Stack = {3}", ep, ex.GetType().FullName, ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Removes a OSRM server from the manager.
        /// Note: removing a server means it will not be assigned to any job,
        /// but doesn't mean that the slave is released by jobs it's already
        /// assigned to.
        /// </summary>
        /// <param name="ep">Server Endpoint</param>
        public void RemoveOsrmServer(IPEndPoint ep)
        {
            lock (_osrmServers)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveOsrmServer: Mutex Lock Acquired: {0}", ep);

                Int32 n = _osrmServers.RemoveAll(new Predicate<ServerInfo>((ServerInfo p) => p.Endpoint.Equals(ep)));
                if (n == 0) // no items removed.. it's not supposed to be like that
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "RemoveOsrmServer: EndPoint not found: {0}", ep);
                    return;
                }

                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RemoveOsrmServer: Success. {1} occurences removed: {0}", ep, n);
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveOsrmServer: Releasing Mutex Lock: {0}", ep);
            }
        }

        /// <summary>
        /// Adds a Redis server to the manager.
        /// Note: depending on network status, you may expect this method to block executing thread
        /// for up to _pingDelay milliseconds.
        /// </summary>
        /// <param name="region">Region code of the server</param>
        /// <param name="ep">IP EndPoint of the server</param>
        public void AddRedisServer(String region, IPEndPoint ep)
        {
            try
            {
                ServerInfo info = new ServerInfo(ep);
                info.Tag = region;

                lock (_redisServers)
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddRedisServer: Mutex Lock Acquired: {0}", ep);
                    _redisServers.Add(info);
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "AddRedisServer: Success: {0}", ep);
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddRedisServer: Releasing Mutex Lock: {0}", ep);
                }
            }
            catch (Exception ex)
            {
                // Unexpected exception
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Unexpected,
                    "AddRedisServer: Unexpected Exception: {{Endpoint = {0}; ExceptionType = {1}; Message = {2}; Stack = {3}", ep, ex.GetType().FullName, ex.Message, ex.StackTrace);
            }

        }

        /// <summary>
        /// Removes a Redis server from the manager.
        /// Note: removing a server means it will not be assigned to any job,
        /// but doesn't mean that the slave is released by jobs it's already
        /// assigned to.
        /// </summary>
        /// <param name="ep">Server Endpoint</param>
        public void RemoveRedisServer(IPEndPoint ep)
        {
            lock (_redisServers)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveRedisServer: Mutex Lock Acquired: {0}", ep);

                Int32 n = _redisServers.RemoveAll(new Predicate<ServerInfo>((ServerInfo p) => p.Endpoint.Equals(ep)));
                if (n == 0) // no items removed.. it's not supposed to be like that
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "RemoveRedisServer: EndPoint not found: {0}", ep);
                    return;
                }

                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RemoveRedisServer: Success. {1} occurences removed: {0}", ep, n);
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveRedisServer: Releasing Mutex Lock: {0}", ep);

            }
        }

        #endregion

        #region Maintain Server Resources

        Thread _maintain;

        private void MaintainServerResources()
        {
            try
            {
                while (true)
                {
                    // Update Slave Servers
                    lock (_slaveServers)
                    {
                        foreach (SlaveServerInfo slave in _slaveServers)
                            slave.Update();
                    }

                    // Update OSRM Servers
                    lock (_osrmServers)
                    {
                        foreach (ServerInfo srv in _osrmServers)
                            srv.Update();
                    }

                    // Update Redis Servers
                    lock (_redisServers)
                    {
                        foreach (ServerInfo srv in _redisServers)
                            srv.Update();
                    }

                    Thread.Sleep(MAINTAIN_INTERVAL);
                }
            }
            catch (ThreadAbortException ex)
            {
                // do nothing
            }
        }

        #endregion

        #region 



        #endregion

        // Destructor
        ~ServerResourceManager()
        {
            _maintain.Abort();
        }

        // Static utility methods
        private static IPEndPoint ParseIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) 
                throw new FormatException("Invalid endpoint format");

            IPAddress ip = null;

            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                    throw new FormatException("Invalid ip-adress");
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                    throw new FormatException("Invalid ip-adress");
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
                throw new FormatException("Invalid port");
             
            return new IPEndPoint(ip, port);
        }
    }
}
