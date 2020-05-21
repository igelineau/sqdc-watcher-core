

using System;
using XFactory.SqdcWatcher.Data.Entities;



namespace XFactory.SqdcWatcher.Core.Services
{
    public class VariantStockStatusUpdater
    {
        public StockStatusChangeResult SetStockStatus(ProductVariant variant, bool isInStock)
        {
            bool hasChangedToInStock = !variant.InStock && isInStock;
            bool hasChangedToNotInStock = variant.InStock && !isInStock;
            if (hasChangedToInStock)
            {
                variant.LastInStockTimestamp = DateTime.Now;
            }

            variant.InStock = isInStock;

            if (hasChangedToInStock)
            {
                return StockStatusChangeResult.BecameInStock;
            }

            if (hasChangedToNotInStock)
            {
                return StockStatusChangeResult.BecameOutOfStock;
            }

            return StockStatusChangeResult.NotChanged;
        }
    }
}