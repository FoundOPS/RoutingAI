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
    /// Since this class detects problems in the entire server infrastructure,
    /// its log entries are most likely marked with Routine, Critical or Fatal.
    /// </remarks>
    public class ServerResourceManager
    {
        private const String TAG = "SrvMgr";

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
        private readonly Random _rand = new Random();   // Random Number Generator

        // Servers sorted by ping values
        private List<SlaveServerInfo> _slaveServers;    // list of slave servers
        private List<ServerInfo> _librarianServers;     // list of librarian servers

        // Servers sorted by region
        private List<ServerInfo> _osrmServers;      // list of OSRM servers
        private List<ServerInfo> _redisServers;     // list of Redis servers
            

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
            GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "Adding Librarian Servers from Configuration File...");
            foreach (XmlNode node in config.SelectNodes("RoutingAiConfig/LibrarianServers/Endpoint"))
            {
                IPEndPoint ep = ParseIPEndPoint(node.InnerText);
                AddLibrarianServer(ep);
            }
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

        /// <summary>
        /// Adds a Librarian server to the manager.
        /// Note: depending on network status, you may expect this method to block executing thread
        /// for up to _pingDelay milliseconds.
        /// </summary>
        /// <param name="region">Region code of the server</param>
        /// <param name="ep">IP EndPoint of the server</param>
        public void AddLibrarianServer(IPEndPoint ep)
        {
            try
            {
                ServerInfo info = new ServerInfo(ep);

                lock (_librarianServers)
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddLibrarianServer: Mutex Lock Acquired: {0}", ep);

                    _librarianServers.Add(info);

                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "AddLibrarianServer: Success: {0}", ep);
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "AddLibrarianServer: Releasing Mutex Lock: {0}", ep);
                }
            }
            catch (Exception ex)
            {
                // Unexpected exception
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Critical | MessageFlags.Unexpected,
                    "AddLibrarianServer: Unexpected Exception: {{Endpoint = {0}; ExceptionType = {1}; Message = {2}; Stack = {3}", ep, ex.GetType().FullName, ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Removes a Librarian server from the manager.
        /// Note: removing a server means it will not be assigned to any job,
        /// but doesn't mean that the slave is released by jobs it's already
        /// assigned to.
        /// </summary>
        /// <param name="ep">Server Endpoint</param>
        public void RemoveLibrarianServer(IPEndPoint ep)
        {
            lock (_slaveServers)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveLibrarianServer: Mutex Lock Acquired: {0}", ep);
                Int32 n = _librarianServers.RemoveAll(new Predicate<ServerInfo>((ServerInfo p) => p.Endpoint.Equals(ep)));
                if (n == 0) // no items removed.. it's not supposed to be like that
                {
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Error | MessageFlags.Expected, "RemoveLibrarianServer: EndPoint not found: {0}", ep);
                    return;
                }
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Routine, "RemoveLibrarianServer: Success: {0}", ep);
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Verbose, "RemoveLibrarianServer: Releasing Mutex Lock: {0}", ep);
            }
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

        /// <summary>
        /// Updates ping delay and capacity information
        /// of all slave servers, then sorts them
        /// </summary>
        public void UpdateSlaveServers()
        {
            lock (_slaveServers)
            {
                foreach (SlaveServerInfo slave in _slaveServers)
                    slave.Update(_pingTimeout);
                _slaveServers.Sort();
            }
        }

        /// <summary>
        /// Updates ping delay of all OSRM servers, then sorts them
        /// </summary>
        public void UpdateOsrmServers()
        {
            lock (_osrmServers)
            {
                foreach (ServerInfo srv in _osrmServers)
                    srv.Update(_pingTimeout);
                _osrmServers.Sort();
            }
        }

        /// <summary>
        /// Updates ping delay of all Redis servers, then sorts them
        /// </summary>
        public void UpdateRedisServers()
        {
            lock (_redisServers)
            {
                foreach (ServerInfo srv in _redisServers)
                    srv.Update(_pingTimeout);
                _redisServers.Sort();
            }
        }

        /// <summary>
        /// Updates ping delay of all Librarian servers, then sorts them
        /// </summary>
        public void UpdateLibrarianServers()
        {
            lock (_librarianServers)
            {
                foreach (ServerInfo libr in _librarianServers)
                    libr.Update(_pingTimeout);
                _librarianServers.Sort();
            }
        }

        /// <summary>
        /// Updates ping delay of all servers, then sorts them
        /// </summary>
        public void UpdateAllServers()
        {
            UpdateSlaveServers();
            UpdateLibrarianServers();
            UpdateOsrmServers();
            UpdateRedisServers();
        }

        #endregion

        #region Getting Servers

        /// <summary>
        /// Gets the specified number of endpoints representing slave servers.
        /// Slave servers with more remaining capacity are selected first.
        /// </summary>
        /// <param name="count">Number of servers to get</param>
        /// <returns>Array of IP endpoints</returns>
        public IPEndPoint[] GetSlaveServers(Int32 count)
        {
            UpdateSlaveServers();
            IEnumerable<IPEndPoint> servers = (from srv in _slaveServers
                                              where srv.IsResponsive
                                              orderby srv.RemainingCapacity
                                              select srv.Endpoint);
            IPEndPoint[] eps = servers.ToArray();

            if (eps.Length > count)
                return eps.Take(count).ToArray();
            else
                return eps;
        }

        /// <summary>
        /// Gets a random Librarian server
        /// </summary>
        /// <returns>IP Endpoint representing the Librarian Server; null if no librarian found</returns>
        public IPEndPoint GetLibrarianServer()
        {
            UpdateLibrarianServers();
            IPEndPoint[] eps = (from srv in _librarianServers
                                where srv.IsResponsive
                                select srv.Endpoint).ToArray();
            Int32 index = _rand.Next(0, eps.Length);
            return eps[index];
        }

        /// <summary>
        /// Gets all responding OSRM servers for the specified region
        /// </summary>
        /// <param name="region">Region code</param>
        /// <returns>Array of IP Endpoints representing servers</returns>
        public IPEndPoint[] GetOsrmServers(String region)
        {
            UpdateOsrmServers();
            return (from srv in _osrmServers where srv.IsResponsive && region.Equals(srv.Tag) select srv.Endpoint).ToArray();
        }

        /// <summary>
        /// Gets all responding Redis servers for the specified region
        /// </summary>
        /// <param name="region">Region code</param>
        /// <returns>Array of IP Endpoints representing servers</returns>
        public IPEndPoint[] GetRedisServers(String region)
        {
            UpdateRedisServers();
            return (from srv in _redisServers where srv.IsResponsive && region.Equals(srv.Tag) select srv.Endpoint).ToArray();
        }

        #endregion

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
