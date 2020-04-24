using System;
using JetBrains.Annotations;
using SqdcWatcher.DataAccess;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels;
using SqdcWatcher.Utils;

namespace SqdcWatcher.MappingFilters
{
    [UsedImplicitly]
    public class PriceHistoryFilter : MappingFilterBase<ProductVariantDto, ProductVariant>
    {
        private readonly SqdcDataAccess dataAccess;

        public PriceHistoryFilter(SqdcDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }
        
        public override void Apply(ProductVariantDto source, ProductVariant destination)
        {
            if (source.PriceInfo == null)
            {
                return;
            }
            
            var parsedSourcePrices = PriceParser.ParseVariantPrice(source.PriceInfo);
            double? newDisplayPrice = null;
            double? newListPrice = null;
            if (destination.DisplayPrice > 0 &&
                parsedSourcePrices.DisplayPrice.HasValue &&
                !parsedSourcePrices.DisplayPrice.Value.Equals(destination.DisplayPrice, 2))
            {
                newDisplayPrice = parsedSourcePrices.DisplayPrice;
            }

            if (destination.ListPrice > 0 &&
                parsedSourcePrices.ListPrice.HasValue &&
                !parsedSourcePrices.ListPrice.Value.Equals(destination.ListPrice, 2))
            {
                newListPrice = parsedSourcePrices.ListPrice;
            }

            if (newDisplayPrice != null || newListPrice != null)
            {
                var priceHistoryEntry = new PriceHistory
                {
                    Timestamp = DateTime.Now,
                    VariantId = destination.ProductVariantId,
                    OldDisplayPrice = newDisplayPrice == null ? new double?() : destination.DisplayPrice,
                    NewDisplayPrice = newDisplayPrice,
                    OldListPrice = newListPrice == null ? new double?() : destination.ListPrice,
                    NewListPrice = newListPrice
                };
                dataAccess.InsertPriceHistoryEntry(priceHistoryEntry);
            }
        }
    }
}