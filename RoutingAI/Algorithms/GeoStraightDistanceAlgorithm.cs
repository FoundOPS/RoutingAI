using RoutingAI.API.OSRM;
using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.Utilities;
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
            Int32 distance = (Int32)GeoTools.OrthodomicDistance(start.Coordinates, end.Coordinates, GeoTools.FormulaType.VincentyFormula);
            Int32 time = distance / 14; // default speed of 50km/h
            return new Pair<int, int>(distance, time);
        }
    }
}
