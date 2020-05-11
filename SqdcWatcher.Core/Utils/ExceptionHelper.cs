using System;
using System.Threading.Tasks;

namespace XFactory.SqdcWatcher.Core.Utils
{
    public static class TaskAwaiterHelper
    {
        public static async Task AwaitIgnoringExceptionAsync<T>(Task task) where T : Exception
        {
            try
            {
                await task;
            }
            catch (T)
            {
                // ignore this type of Exception when encountered
            }
        }
    }
}