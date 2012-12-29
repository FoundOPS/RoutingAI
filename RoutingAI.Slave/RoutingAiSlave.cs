using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Slave
{
    class RoutingAiSlave : IRoutingAiSlaveService
    {
        public string Ping()
        {
            return "RoutingAiSlave.Ping(): Hello World!";
        }
    }
}
