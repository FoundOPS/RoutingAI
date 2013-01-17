using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Utilities
{
    /// <summary>
    /// Static class containing extension methods for DateTime and Window classes.
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// Determines whether the Window completely contains another Window.
        /// Start and/or End values can be equal.
        /// </summary>
        /// <param name="a">Parent window</param>
        /// <param name="w">Child window</param>
        /// <returns>True if parent window completely contains child window; false otherwise</returns>
        public static Boolean Contains(this Window a, Window w)
        {
            return (w.Start >= a.Start && w.End <= a.End);
        }

        /// <summary>
        /// Determines whether the Window intersects with another Window.
        /// Equal Start and/or End values do NOT count as intersection.
        /// </summary>
        /// <param name="a">First Window</param>
        /// <param name="w">Second Window</param>
        /// <returns>True if first window and second window intersect; false otherwise</returns>
        public static Boolean Intersects(this Window a, Window w)
        {
            if (w.End <= a.Start) return false;
            if (w.Start >= a.End) return false;
            return true;
        }
    
        /// <summary>
        /// Checks whether the Window ends before another window begins.
        /// If End equals Start of the next Window, it does not count as intersection.
        /// </summary>
        /// <param name="a">First Window</param>
        /// <param name="w">Second Window</param>
        /// <returns>True if first window ends before the second window starts; false otherwise</returns>
        public static Boolean IsBefore(this Window a, Window w)
        {
            if (a.Intersects(w)) return false;
            if (w.End <= a.Start) return true;
            return false;
        }

        /// <summary>
        /// Checks whether the Window starts efter another window begendsins.
        /// If End equals Start, it does not count as intersection.
        /// </summary>
        /// <param name="a">First Window</param>
        /// <param name="w">Second Window</param>
        /// <returns>True if first window starts after the second window ends; false otherwise</returns>
        public static Boolean IsAfter(this Window a, Window w)
        {
            if (a.Intersects(w)) return false;
            if (w.Start >= a.End) return true;
            return false;
        }
    }
}
