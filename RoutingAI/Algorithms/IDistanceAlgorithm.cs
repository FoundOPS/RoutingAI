using RoutingAI.API.OSRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms
{
    public interface IDistanceAlgorithm
    {
        Int32 GetDistance(Coordinate start, Coordinate end);
    }
}
