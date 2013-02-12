using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoutingAI.API.OSRM;

namespace RoutingAI.Utilities
{
    /// <summary>
    /// Contains various methods for calculating distances between geolocations
    /// </summary>
    public static class GeoTools
    {
        //Othodromic Distance from https://github.com/lmaslanka/Orthodromic-Distance-Calculator

        #region Distance Calculation

        /// <summary>
        /// Radius of Earth
        /// </summary>
        public const Double EARTH_RADIUS = 6371.01;

        /// <summary>
        /// Type of distance formula
        /// </summary>
        public enum FormulaType
        {
            SphericalLawOfCosinesFormula = 1,
            HaversineFormula = 2,
            VincentyFormula = 3
        }

        /// <summary>
        /// Calculates orthodomic distance between two geolocations
        /// </summary>
        /// <param name="a">Location A</param>
        /// <param name="b">Location B</param>
        /// <param name="formula">Distance formula to use</param>
        /// <returns>Distance between locations in meters</returns>
        public static Double OrthodomicDistance(Coordinate a, Coordinate b, FormulaType formula)
        {
            return EARTH_RADIUS * ArcLength(a, b, formula) * 1000;
        }

        private static double ArcLength(Coordinate locationA, Coordinate locationB, FormulaType formula)
        {
            switch (formula)
            {
                case FormulaType.SphericalLawOfCosinesFormula:
                    return ArcLengthSphericalLawOfCosines(locationA, locationB);

                case FormulaType.HaversineFormula:
                    return ArcLengthHaversineFormula(locationA, locationB);

                case FormulaType.VincentyFormula:
                    return ArcLengthVincentyFormula(locationA, locationB);

                default:
                    return 0;
            }
        }

        private static double ArcLengthSphericalLawOfCosines(Coordinate locationA, Coordinate locationB)
        {
            return Math.Acos((Math.Sin(locationA.LatitudeRad) * Math.Sin(locationB.LatitudeRad)) + (Math.Sin(locationA.LatitudeRad) * Math.Sin(locationB.LatitudeRad) * Math.Cos(Diff(locationA.LongitudeRad, locationB.LongitudeRad))));
        }

        private static double ArcLengthHaversineFormula(Coordinate locationA, Coordinate locationB)
        {
            return 2 * Math.Asin(Math.Sqrt((Sin2(Diff(locationA.LatitudeRad, locationB.LatitudeRad) / 2)) + (Math.Cos(locationA.LatitudeRad) * Math.Cos(locationB.LatitudeRad) * Sin2(Diff(locationA.LongitudeRad, locationB.LongitudeRad) / 2))));
        }

        private static double ArcLengthVincentyFormula(Coordinate locationA, Coordinate locationB)
        {
            return Math.Atan(Math.Sqrt(((Math.Pow(Math.Cos(locationB.LatitudeRad) * Math.Sin(Diff(locationA.LongitudeRad, locationB.LongitudeRad)), 2)) + (Math.Pow((Math.Cos(locationA.LatitudeRad) * Math.Sin(locationB.LatitudeRad)) - (Math.Sin(locationA.LatitudeRad) * Math.Cos(locationB.LatitudeRad) * Math.Cos(Diff(locationA.LongitudeRad, locationB.LongitudeRad))), 2))) / ((Math.Sin(locationA.LatitudeRad) * Math.Sin(locationB.LatitudeRad)) + (Math.Cos(locationA.LatitudeRad) * Math.Cos(locationB.LatitudeRad) * Math.Cos(Diff(locationA.LongitudeRad, locationB.LongitudeRad))))));
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts degree value to radian value
        /// </summary>
        /// <param name="angle">Angle to convert</param>
        /// <returns>Same angle in radians</returns>
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Converts a radian value into degree value
        /// </summary>
        /// <param name="angle">Angle to convert</param>
        /// <returns>Same angle in degrees</returns>
        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static double Sin2(double x)
        {
            return 0.5 - 0.5 * Math.Cos(2 * x);
        }

        public static double Diff(double a, double b)
        {
            return Math.Abs(b - a);
        }

        #endregion
    }
}
