using System.Collections.Generic;
using System.Threading;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface IRemoteStore<out T> where T : class
    {
        IAsyncEnumerable<T> GetAllItemsAsync(CancellationToken cancellationToken);
    }
}