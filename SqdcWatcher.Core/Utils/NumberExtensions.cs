using System;

namespace XFactory.SqdcWatcher.Core.Utils
{
    public static class NumberExtensions
    {
        public static bool Equals(this double source, double other, int significantDigits = 2)
        {
            return Math.Abs(source - other) < Math.Pow(10, -significantDigits);
        }
    }
}