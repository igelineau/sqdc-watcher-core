#region

using System;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Utils
{
    public static class NumberExtensions
    {
        public static bool Equals(this double source, double other, int toleranceDigits = 2)
        {
            return Math.Abs(source - other) < Math.Pow(10, -toleranceDigits);
        }
    }
}