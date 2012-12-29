using RoutingAI.API.OSRM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms.DFEKM
{
    /// <summary>
    /// One single thread processing K-Means clustering
    /// </summary>
    public class DfekmProcessor
    {
        CaTable _caTable;
        Coordinate[] _problemData;
        ArraySegment<Coordinate> _processorData;
        Dictionary<Coordinate, BitArray> _boundaryIndex = new Dictionary<Coordinate, BitArray>();

        IDistanceAlgorithm _distanceAlg = new GeoStraightDistanceAlgorithm();

    }
}
