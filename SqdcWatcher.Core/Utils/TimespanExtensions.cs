using System;
using Humanizer;

namespace XFactory.SqdcWatcher.Core.Utils
{
    public static class TimespanExtensions
    {
        public static string ToSmartFormat(this TimeSpan timeSpan)
        {
            return timeSpan.Humanize();
        }
    }
}