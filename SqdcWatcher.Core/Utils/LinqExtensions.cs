using System.Collections.Generic;

namespace XFactory.SqdcWatcher.Core.Utils
{
    public static class LinqExtensions
    {
#pragma warning disable 1998
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
#pragma warning restore 1998
        {
            foreach(T item in source)
            {
                yield return item;
            }
        }
    }
}