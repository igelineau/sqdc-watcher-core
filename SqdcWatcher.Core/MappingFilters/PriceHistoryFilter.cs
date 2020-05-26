using System;
using JetBrains.Annotations;
using SqdcWatcher.DataTransferObjects.DomainDto;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Core.Utils;
using XFactory.SqdcWatcher.Data.Entities.History;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;
using XFactory.SqdcWatcher.DataAccess;

namespace XFactory.SqdcWatcher.Core.MappingFilters
{
    [UsedImplicitly]
    public class PriceHistoryFilter : IMappingFilter<ProductVariantPrice, ProductVariant>
    {
        private readonly ISqdcDataAccess dataAccess;

        public PriceHistoryFilter(ISqdcDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void Apply(ProductVariantPrice source, ProductVariant destination)
        {
            ParsedPriceInfo parsedSourcePrices = PriceParser.ParseVariantPrice(source);
            double? newDisplayPrice = null;
            double? newListPrice = null;
            if (destination.DisplayPrice > 0 &&
                parsedSourcePrices.DisplayPrice.HasValue &&
                !parsedSourcePrices.DisplayPrice.Value.Equals(destination.DisplayPrice, 2))
                newDisplayPrice = parsedSourcePrices.DisplayPrice;

            if (destination.ListPrice > 0 &&
                parsedSourcePrices.ListPrice.HasValue &&
                !parsedSourcePrices.ListPrice.Value.Equals(destination.ListPrice, 2))
                newListPrice = parsedSourcePrices.ListPrice;

            if (newDisplayPrice != null || newListPrice != null)
            {
                var priceHistoryEntry = new PriceHistory
                {
                    Timestamp = DateTime.Now,
                    ProductVariantId = destination.Id,
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