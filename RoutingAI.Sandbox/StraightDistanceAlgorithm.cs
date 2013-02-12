using RoutingAI.API.OSRM;
using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libWyvernzora;

namespace RoutingAI.Algorithms
{
    public class StraightDistanceAlgorithm : IDistanceAlgorithm<Coordinate>
    {

        public int GetDistance(Coordinate start, Coordinate end)
        {
            return GetDistanceTime(start, end).First;
        }

        public int GetTime(Coordinate start, Coordinate end)
        {
            return GetDistanceTime(start, end).Second;
        }

        public Pair<int, int> GetDistanceTime(Coordinate start, Coordinate end)
        {
            Int32 distance = (Int32)
                       Math.Sqrt(Math.Pow(end.Latitude - start.Latitude, 2) +
                                 Math.Pow(end.Longitude - start.Longitude, 2));
            Int32 time = distance / 3; // default speed of around 10km/h
            return new Pair<int, int>(distance, time);
        }
    }
}
