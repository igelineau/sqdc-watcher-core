#region

using System;
using System.Reflection;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Utils
{
    public static class AssemblyExtensions
    {
        public static string GetContainingDirectory(this Assembly assembly)
        {
            return new Uri(assembly.GetName().CodeBase).LocalPath;
        }
    }
}