using libWyvernzora;
using libWyvernzora.Logging;
using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Controller
{
    /// <summary>
    /// Represents a server managed by ServerResourceManager
    /// </summary>
    public class ServerInfo : IComparable<ServerInfo>
    {
        /// <summary>
        /// Log message tag for this and deriving classes
        /// </summary>
        protected const String TAG = "SrvInfo";

        // Properties
        /// <summary>
        /// Gets the ddress of the server represented by 
        /// this ServerInfo object
        /// </summary>
        public IPEndPoint Endpoint { get; protected set; }
        /// <summary>
        /// Gets a value indicating whether the server
        /// responded during last update
        /// </summary>
        public Boolean IsResponsive { get; protected set; }
        /// <summary>
        /// Gets the ping delay of the server
        /// </summary>
        public Int32 PingDelay { get; protected set; }
        /// <summary>
        /// Gets or sets a string containing additional information
        /// about the server represented by this ServerInfo object
        /// </summary>
        public String Tag { get; set; }

        // Constructors
        /// <summary>
        /// Constructor
        /// Creates a ServerInfo object from an IP EndPoint
        /// </summary>
        /// <param name="ep">IP endpoint of the server</param>
        public ServerInfo(IPEndPoint ep)
        {
            Endpoint = ep;
        }

        // Methods
        /// <summary>
        /// Updates server ping delay.
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait for response before timing-out</param>
        public virtual void Update(Int32 timeout)
        {
            // Update Ping Time
            PingReply reply = (new Ping()).Send(Endpoint.Address, timeout);
            if (reply.Status != IPStatus.Success)
            {
                // Ping did not succeed.. log the event and set this server as non-responsive
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "ServerInfo.Update: Ping Failed (Status = {0}): {1}", reply.Status.ToString(), Endpoint);
                PingDelay = Int32.MaxValue;
                IsResponsive = false;
            }
            else
            {
                // Ping was successful, update server info
                PingDelay = (int)reply.RoundtripTime;
                IsResponsive = true;
            }
        }


        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another object 
        /// of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or 
        /// occurs in the same position in the sort order as the 
        /// other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public virtual int CompareTo(ServerInfo other)
        {
            if (!this.IsResponsive)
                return 0;
            else if (!other.IsResponsive)
                return -1;

            return this.PingDelay.CompareTo(other.PingDelay);
        }

        #endregion
    }

    /// <summary>
    /// Represents a RoutingAI.Slave server managed by ServerResourceManager
    /// </summary>
    public class SlaveServerInfo : ServerInfo
    {
        // Fields
        private IRoutingAiSlaveService proxy = null;    // WCF Service Proxy

        // Properties
        /// <summary>
        /// Gets the remaining number of threads that can be 
        /// created on the slave server
        /// </summary>
        public Int32 RemainingCapacity { get; private set; }
        /// <summary>
        /// Gets the total number of threads that can be created
        /// on the slave server
        /// </summary>
        public Int32 TotalCapacity { get; private set; }
        /// <summary>
        /// Gets WCF Service Proxy associated with this server
        /// </summary>
        public IRoutingAiSlaveService Proxy
        { get { return proxy; } }

        // COnstructor
        /// <summary>
        /// Constructor
        /// Creates a SlaveServerInfo instance from an IP endpoint
        /// </summary>
        /// <param name="ep">IP endpoint of the server</param>
        public SlaveServerInfo(IPEndPoint ep)
            : base(ep)
        { }

        /// <summary>
        /// Updates ping delay and remaining capacity of the server
        /// </summary>
        /// <param name="timeout">Number of milliseconds to wait for response before timing-out</param>
        public override void Update(Int32 timeout)
        {
            base.Update(timeout);

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
                TotalCapacity = loadInfo.Second;
            }
            catch (EndpointNotFoundException)
            {
                GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected,
                    "SlaveServerInfo.Update: RoutingAI.Slave service not found: {0}", Endpoint);
                IsResponsive = false;
            }

        }

        #region IComparable Members

        /// <summary>
        /// Compares the current instance with another object 
        /// of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or 
        /// occurs in the same position in the sort order as the 
        /// other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public override int CompareTo(ServerInfo other)
        {
            SlaveServerInfo slave = other as SlaveServerInfo;

            if (!this.IsResponsive)
                return 0;
            else if (!other.IsResponsive)
                return -1;

            if (slave != null && slave.RemainingCapacity != this.RemainingCapacity)
                return slave.RemainingCapacity.CompareTo(this.RemainingCapacity); // Remaining capacity order is reversed

            return this.PingDelay.CompareTo(other.PingDelay);
        }

        #endregion
    }
}
