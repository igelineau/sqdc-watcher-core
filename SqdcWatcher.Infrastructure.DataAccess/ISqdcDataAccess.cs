using System.Threading.Tasks;
using XFactory.SqdcWatcher.Data.Entities.History;

namespace XFactory.SqdcWatcher.DataAccess
{
    public interface ISqdcDataAccess
    {
        Task InsertPriceHistoryEntry(PriceHistory priceHistoryEntry);
    }
}