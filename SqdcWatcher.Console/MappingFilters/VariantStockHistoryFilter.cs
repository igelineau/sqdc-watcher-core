#region

using System;
using Models.EntityFramework;
using SqdcWatcher.Dto;
using SqdcWatcher.RestApiModels;
using SqdcWatcher.Services;

#endregion

namespace SqdcWatcher.MappingFilters
{
    public class VariantStockHistoryFilter : MappingFilterBase<ProductVariantDto, ProductVariant>
    {
        private readonly StockHistoryPersister stockHistoryPersister;

        public VariantStockHistoryFilter(StockHistoryPersister stockHistoryPersister)
        {
            this.stockHistoryPersister = stockHistoryPersister;
        }

        public override void Apply(ProductVariantDto source, ProductVariant destination)
        {
            bool hasChangedToInStock = !destination.InStock && source.InStock;
            bool hasChangedToNotInStock = destination.InStock && !source.InStock;
            if (hasChangedToInStock)
            {
                stockHistoryPersister.AddHistoryEntry(source.Id, StockEventNames.IN_STOCK);
                destination.LastInStockTimestamp = DateTime.Now;
            }
            else if (hasChangedToNotInStock)
            {
                stockHistoryPersister.AddHistoryEntry(source.Id, StockEventNames.OUT_OF_STOCK);
            }
        }
    }
}