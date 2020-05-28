using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace CannaWatch.Markets.Sqdc.Implementation
{
    public class SqdcMarketFacade : IMarketFacade
    {
        private readonly IRemoteStore<SqdcMarketFacade, ProductDto> productsFetcher;
        private readonly SqdcRestApiClient restApiClient;

        public MarketIdentity MarketIdentity => new MarketIdentity("Sqdc");

        public SqdcMarketFacade(IRemoteStore<SqdcMarketFacade, ProductDto> productsFetcher, SqdcRestApiClient restApiClient)
        {
            this.productsFetcher = productsFetcher;
            this.restApiClient = restApiClient;
        }
        
        public IAsyncEnumerable<ProductDto> GetAllItemsAsync(CancellationToken cancellationToken)
        {
            return productsFetcher.GetAllItemsAsync(cancellationToken);
        }

        public Task<VariantsPricesResponse> GetVariantsPrices(IEnumerable<string> productIds, CancellationToken cancelToken)
        {
            return restApiClient.GetVariantsPrices(productIds, cancelToken);
        }

        public Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken)
        {
            return restApiClient.GetInventoryItems(variantsIds, cancelToken);
        }

        public Task<SpecificationsResponse> GetSpecifications(string productId, string variantId, CancellationToken cancellationToken)
        {
            return restApiClient.GetSpecifications(productId, variantId, cancellationToken);
        }
    }
}