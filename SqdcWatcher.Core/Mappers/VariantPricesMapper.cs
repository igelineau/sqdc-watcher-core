using System.Collections.Generic;
using XFactory.SqdcWatcher.Core.Dto;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Core.RestApiModels;
using XFactory.SqdcWatcher.Core.Utils;
using XFactory.SqdcWatcher.Data.Entities;

namespace XFactory.SqdcWatcher.Core.Mappers
{
    public class VariantPricesMapper : MapperBase<ProductVariantPrice, ProductVariant>
    {
        public VariantPricesMapper(IEnumerable<IMappingFilter<ProductVariantPrice, ProductVariant>> mappingFilters) : base(mappingFilters)
        {
        }

        protected override ProductVariant PerformMapping(ProductVariantPrice sourcePriceInfo, ProductVariant dest)
        {
            dest ??= CreateDestinationInstance();
            if (dest.Id == 0)
            {
                dest.Id = long.Parse(sourcePriceInfo.VariantId);
            }

            ParsedPriceInfo parsedPrices = PriceParser.ParseVariantPrice(sourcePriceInfo);
            if (parsedPrices.ListPrice != null)
            {
                dest.ListPrice = parsedPrices.ListPrice.Value;
            }

            if (parsedPrices.DisplayPrice != null)
            {
                dest.DisplayPrice = parsedPrices.DisplayPrice.Value;
            }

            if (parsedPrices.PricePerGram != null)
            {
                dest.PricePerGram = parsedPrices.PricePerGram.Value;
            }

            return dest;
        }
    }
}