using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoutingAI.Algorithms
{
    public interface ICostFunction
    {
        Cost GetCost(Resource worker, Task from, Task to);
    }
}
