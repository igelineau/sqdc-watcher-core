using System;
using System.Reflection;

namespace SqdcWatcher.Utils
{
    public static class AssemblyExtensions
    {
        public static string GetContainingDirectory(this Assembly assembly)
        {
            return new Uri(assembly.GetName().CodeBase).LocalPath;
        }
    }
}