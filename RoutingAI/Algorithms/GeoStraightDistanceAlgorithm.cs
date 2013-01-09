using RoutingAI.API.OSRM;
using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoutingAI.Algorithms
{
    public class GeoStraightDistanceAlgorithm : IDistanceAlgorithm<Task>
    {
        public int GetDistance(Task start, Task end)
        {
            return Utilities.GeoTools.StraightDistance(start.Coordinates.latRad, start.Coordinates.lonRad, end.Coordinates.latRad, end.Coordinates.lonRad);
        }
    }
}
