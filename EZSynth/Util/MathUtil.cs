using System;

namespace EZSynth.Util
{
    public static class MathUtil
    {
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (min.CompareTo(max) > 0)
            {
                throw new ArgumentException("min should be less than or equal to max");
            }

            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }
        
        public static double Log2(double value)
        {
            // Log2 of a negative number is undefined
            if (value < 0) return double.NaN;
        
            // Directly return known values for special cases
            if (double.IsNaN(value) || double.IsInfinity(value)) return value;

            return Math.Log(value) / Math.Log(2);
        }
    }
}
