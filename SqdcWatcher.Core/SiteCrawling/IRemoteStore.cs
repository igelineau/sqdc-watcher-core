using System.Collections.Generic;
using System.Threading;

namespace XFactory.SqdcWatcher.Core.SiteCrawling
{
    public interface IRemoteStore<out T> where T : class
    {
        IAsyncEnumerable<T> GetAllItemsAsync(CancellationToken cancellationToken);
    }
}