using System.Collections.Generic;
using System.Threading;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface IRemoteProductsStore<TService>
        where TService : IRemoteProductsStore<TService>
    {
        IAsyncEnumerable<ProductDto> GetAllProductsAsync(Market market, CancellationToken cancellationToken);
    }
}