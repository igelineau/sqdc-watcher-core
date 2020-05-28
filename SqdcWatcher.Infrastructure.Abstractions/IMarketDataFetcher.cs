using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataTransferObjects.RestApiModels;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface IMarketDataFetcher
    {
        Task<VariantsPricesResponse> GetVariantsPrices(IEnumerable<string> productIds, CancellationToken cancelToken);
        Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken);
        Task<SpecificationsResponse> GetSpecifications(string productId, string variantId, CancellationToken cancellationToken);
    }
}