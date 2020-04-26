#region

using System.Collections.Generic;
using System.Threading.Tasks;
using Models.EntityFramework;

#endregion

namespace SqdcWatcher.DataAccess.EntityFramework
{
    public interface ISqdcDataAccess
    {
        Task<IEnumerable<Product>> GetProductsWithRelationsAsync(bool asTracking = false);
        Task<IEnumerable<Product>> GetProductsSummary();
        Task SaveProducts(List<Product> products);
        Task<AppState> GetAppStateAsync();
        Task UpdateAppStateAsync(AppState state);
        void InsertPriceHistoryEntry(PriceHistory priceHistoryEntry);
    }
}