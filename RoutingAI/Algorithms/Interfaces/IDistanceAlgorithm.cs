using RoutingAI.API.OSRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms
{
    public interface IDistanceAlgorithm<T>
    {
        Int32 GetDistance(T start, T end);
    }
}
