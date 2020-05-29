using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CannaWatch.Markets.Sqdc.Mapping;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;
using XFactory.SqdcWatcher.Data.Entities;
using XFactory.SqdcWatcher.Data.Entities.Markets;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace CannaWatch.Markets.Sqdc.Implementation
{
    public class SqdcMarketFacade : IMarketScanService, IMarketFacade
    {
        private readonly IRemoteStore<SqdcMarketFacade, ProductDto> productsFetcher;
        private readonly SqdcRestApiClient restApiClient;
        private readonly VariantPricesMapper variantPricesMapper;
        private readonly SpecificationsMapper specificationsMapper;
        private ILookup<string, ProductPrice> variantsByProduct;

        public MarketIdentity MarketIdentity => new MarketIdentity("Sqdc");

        public SqdcMarketFacade(
            IRemoteStore<SqdcMarketFacade, ProductDto> productsFetcher,
            SqdcRestApiClient restApiClient,
            VariantPricesMapper variantPricesMapper,
            SpecificationsMapper specificationsMapper)
        {
            this.productsFetcher = productsFetcher;
            this.restApiClient = restApiClient;
            this.variantPricesMapper = variantPricesMapper;
            this.specificationsMapper = specificationsMapper;
        }

        public Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken)
        {
            return restApiClient.GetInventoryItems(variantsIds, cancelToken);
        }

        MarketIdentity IMarketScanService.MarketIdentity => new MarketIdentity("Sqdc");

        public IAsyncEnumerable<ProductDto> GetAllProductsAsync(CancellationToken cancellationToken)
        {
            return productsFetcher.GetAllItemsAsync(cancellationToken);
        }

        public async Task PrepareToFetchProductsVariants(IEnumerable<string> allProductsIds, CancellationToken cancellationToken)
        {
            VariantsPricesResponse response = await restApiClient.GetVariantsPrices(allProductsIds, cancellationToken);
            variantsByProduct = response.ProductPrices.ToLookup(pp => pp.ProductId);
        }

        public Task UpdateProductVariants(Product product, CancellationToken cancelToken)
        {
            IEnumerable<ProductPrice> variantsPrices = variantsByProduct[product.Id];
            foreach (ProductPrice productPrice in variantsPrices)
            {
                AddVariantsToProduct(product, productPrice.VariantPrices);
            }

            return Task.CompletedTask;
        }

        private void AddVariantsToProduct(Product product, List<ProductVariantPrice> variantPrices)
        {
            foreach (ProductVariantPrice variantPrice in variantPrices)
            {
                long variantId = long.Parse(variantPrice.VariantId);
                ProductVariant dbVariant = product.GetVariantById(variantId);
                dbVariant = variantPricesMapper.MapCreateOrUpdate(variantPrice, dbVariant);
                dbVariant.ProductId ??= product.Id;
            }
        }

        public async Task UpdateVariantSpecifications(ProductVariant variant, CancellationToken cancellationToken)
        {
            SpecificationsResponse response = await restApiClient.GetSpecifications(variant.ProductId, variant.Id.ToString(), cancellationToken);
            specificationsMapper.MapCreateOrUpdate(response, variant.Specifications);
        }
    }
}