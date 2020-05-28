using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface IMarketFacade
    {
        MarketIdentity MarketIdentity { get; }
        
        IAsyncEnumerable<ProductDto> GetAllItemsAsync(CancellationToken cancellationToken);
        Task<VariantsPricesResponse> GetVariantsPrices(IEnumerable<string> productIds, CancellationToken cancelToken);
        Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken);
        Task<SpecificationsResponse> GetSpecifications(string productId, string variantId, CancellationToken cancellationToken);
    }
}