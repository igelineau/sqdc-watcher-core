using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CannaWatch.Markets.CannaFarms.HttpClient;
using CannaWatch.Markets.CannaFarms.Models;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Data.Entities;
using XFactory.SqdcWatcher.Data.Entities.Markets;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace CannaWatch.Markets.CannaFarms.Implementation
{
    public class CannaFarmsScanService : IMarketScanService, IMarketFacade
    {
        private readonly CannaFarmsRestClient restClient;
        private Dictionary<string, CannaFarmProduct> productsList;

        public MarketIdentity MarketIdentity => new MarketIdentity("CannaFarms");

        public CannaFarmsScanService(CannaFarmsRestClient restClient)
        {
            this.restClient = restClient;
        }

        public async IAsyncEnumerable<ProductDto> GetAllProductsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            productsList = (await restClient.GetPurchasableProducts()).ToDictionary(p => p.Id);
            foreach (CannaFarmProduct product in productsList.Values)
            {
                yield return product.MapToDto();
            }
        }

        public Task PrepareToFetchProductsVariants(IEnumerable<string> allProductsIds, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task UpdateProductVariants(Product product, CancellationToken cancelToken)
        {
            if(!productsList.TryGetValue(product.Id, out CannaFarmProduct ourProduct))
            {
                return Task.CompletedTask;
            }

            foreach (CannaFarmProductVariant sku in ourProduct.Skus)
            {
                ProductVariant mappedVariant = sku.MapToEntity(product.GetVariantById(sku.Id));
                product.AddVariant(mappedVariant);
            }

            return Task.CompletedTask;
        }

        public Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken)
        {
            IEnumerable<long> skuInStock = productsList.Values
                .SelectMany(p => p.Skus)
                .Where(sku => sku.InStock)
                .Select(sku => sku.Id);
            return Task.FromResult(skuInStock.ToList());
        }

        public Task UpdateVariantSpecifications(ProductVariant variant, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}