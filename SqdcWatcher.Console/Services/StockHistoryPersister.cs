#region

using System;
using Models.EntityFramework;
using SqdcWatcher.DataAccess.EntityFramework;

#endregion

namespace SqdcWatcher.Services
{
    public class StockHistoryPersister
    {
        private readonly ISqdcDataAccess dataAccess;

        public StockHistoryPersister(ISqdcDataAccess dataAccess)
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
            //dataAccess.InsertHistoryEntry(stockHistory);
        }
    }
}