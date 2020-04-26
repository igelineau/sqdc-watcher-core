using System;
using Humanizer;

namespace XFactory.SqdcWatcher.ConsoleApp.Utils
{
    public static class TimespanExtensions
    {
        public static string ToSmartFormat(this TimeSpan timeSpan)
        {
            return timeSpan.Humanize();
        }
    }
}