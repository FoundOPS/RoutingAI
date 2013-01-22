using RoutingAI.API.OSRM;
using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libWyvernzora;

namespace RoutingAI.Algorithms
{
    /// <summary>
    /// Distance algorithm based in geological straight distance
    /// </summary>
    public class GeoStraightDistanceAlgorithm : IDistanceAlgorithm<Task>
    {
        public int GetDistance(Task start, Task end)
        {
            return GetDistanceTime(start, end).First;
        }


        public int GetTime(Task start, Task end)
        {
            return GetDistanceTime(start, end).Second;
        }


        public Pair<int, int> GetDistanceTime(Task start, Task end)
        {
            Int32 distance = Utilities.GeoTools.StraightDistance(start.Coordinates.latRad, start.Coordinates.lonRad, end.Coordinates.latRad, end.Coordinates.lonRad);
            Int32 time = distance / 3; // default speed of 10km/h
            return new Pair<int, int>(distance, time);
        }
    }
}
