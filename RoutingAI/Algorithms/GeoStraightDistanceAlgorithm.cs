using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms
{
    public class GeoStraightDistanceAlgorithm : IDistanceAlgorithm
    {
        public int GetDistance(API.OSRM.Coordinate start, API.OSRM.Coordinate end)
        {
            return Utilities.GeoTools.StraightDistance(start.latRad, start.lonRad, end.latRad, end.lonRad);
        }
    }
}
