using System.Collections.Generic;
using System.Threading;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface IRemoteStore<TMarketFacade, out TData>
        where TMarketFacade : IMarketFacade
        where TData : class
    {
        IAsyncEnumerable<TData> GetAllItemsAsync(CancellationToken cancellationToken);
    }
}