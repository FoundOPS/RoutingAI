using RoutingAI.API.OSRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms
{
    public class StraightDistanceAlgorithm : IDistanceAlgorithm<Coordinate>
    {
        public int GetDistance(API.OSRM.Coordinate start, API.OSRM.Coordinate end)
        {
            return (Int32)Math.Sqrt(Math.Pow(start.lon - end.lon, 2) + Math.Pow(start.lat - end.lat, 2));
        }
    }
}
