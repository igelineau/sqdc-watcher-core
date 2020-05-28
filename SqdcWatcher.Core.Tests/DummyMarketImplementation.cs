using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;

namespace SqdcWatcher.Core.Tests
{
    public class DummyMarketImplementation : IMarketDataFetcher
    {
        public Task<VariantsPricesResponse> GetVariantsPrices(IEnumerable<string> productIds, CancellationToken cancelToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<SpecificationsResponse> GetSpecifications(string productId, string variantId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}