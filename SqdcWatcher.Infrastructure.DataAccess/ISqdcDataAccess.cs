using System.Collections.Generic;
using System.Threading.Tasks;
using XFactory.SqdcWatcher.Data.Entities.Common;
using XFactory.SqdcWatcher.Data.Entities.History;
using XFactory.SqdcWatcher.Data.Entities.Products;

namespace XFactory.SqdcWatcher.DataAccess
{
    public interface ISqdcDataAccess
    {
        Task<IEnumerable<Product>> GetProductsWithRelationsAsync(bool asTracking = false);
        Task<IEnumerable<Product>> GetProductsSummary();
        Task SaveProducts(List<Product> products);
        Task<AppState> GetAppStateAsync();
        Task UpdateAppStateAsync(AppState state);
        Task InsertPriceHistoryEntry(PriceHistory priceHistoryEntry);
    }
}