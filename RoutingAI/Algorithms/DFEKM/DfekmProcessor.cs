using RoutingAI.API.OSRM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingAI.ServiceContracts;

namespace RoutingAI.Algorithms.DFEKM
{
    /// <summary>
    /// One single thread processing K-Means clustering
    /// </summary>
    public class DfekmProcessor<T>
    {
        Int32 _myId;
        CaTable _caTable;
        ILibrarianService _librarian;
        

        Coordinate[] _problemData;
        ArraySegment<Coordinate> _processorData;
        Dictionary<Coordinate, BitArray> _boundaryIndex = new Dictionary<Coordinate, BitArray>();

        IDistanceAlgorithm<T> _distanceAlg;
    }
}
