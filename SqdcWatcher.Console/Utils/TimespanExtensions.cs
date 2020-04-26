using System;
using Humanizer;

namespace SqdcWatcher.Utils
{
    public static class TimespanExtensions
    {
        public static string ToSmartFormat(this TimeSpan timeSpan)
        {
            return timeSpan.Humanize();
        }
    }
}