using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Data.Entities.Markets;
using XFactory.SqdcWatcher.Data.Entities.Products;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    /// <summary>
    /// Market-specific data retrieval behaviors
    /// </summary>
    public interface IMarketScanService
    {
        string GetMarketId();
        
        IAsyncEnumerable<ProductDto> GetAllProductsAsync(Market market, CancellationToken cancellationToken);
        
        Task<List<long>> GetInventoryItems(IEnumerable<long> variantsIds, CancellationToken cancelToken);
        
        Task PrepareToFetchProductsVariants(IEnumerable<string> allProductsIds, CancellationToken cancellationToken);
        public Task UpdateProductVariants(Product product, CancellationToken cancelToken);
        
        Task UpdateVariantSpecifications(ProductVariant variant, CancellationToken cancellationToken);
    }
}