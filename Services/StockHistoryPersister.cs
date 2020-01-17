using System;
using SqdcWatcher.DataAccess;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Services
{
    public class StockHistoryPersister
    {
        private readonly SqdcDataAccess dataAccess;

        public StockHistoryPersister(SqdcDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void AddHistoryEntry(long variantId, string eventName)
        {
            var stockHistory = new StockHistory
            {
                ProductVariantId = variantId,
                Event = eventName,
                Timestamp = DateTime.Now
            };
            dataAccess.InsertHistoryEntry(stockHistory);
        }
    }
}