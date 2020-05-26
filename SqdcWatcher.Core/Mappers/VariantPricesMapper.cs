using System.Collections.Generic;
using JetBrains.Annotations;
using SqdcWatcher.DataTransferObjects.DomainDto;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Core.Utils;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace XFactory.SqdcWatcher.Core.Mappers
{
    [UsedImplicitly]
    public class VariantPricesMapper : MapperBase<ProductVariantPrice, ProductVariant>
    {
        public VariantPricesMapper(IEnumerable<IMappingFilter<ProductVariantPrice, ProductVariant>> mappingFilters) : base(mappingFilters)
        {
        }

        protected override ProductVariant PerformMapping(ProductVariantPrice source, ProductVariant destination)
        {
            ParsedPriceInfo parsedPrices = PriceParser.ParseVariantPrice(source);
            if (parsedPrices.ListPrice != null)
            {
                destination.ListPrice = parsedPrices.ListPrice.Value;
            }

            if (parsedPrices.DisplayPrice != null)
            {
                destination.DisplayPrice = parsedPrices.DisplayPrice.Value;
            }

            if (parsedPrices.PricePerGram != null)
            {
                destination.PricePerGram = parsedPrices.PricePerGram.Value;
            }

            return destination;
        }

        protected override ProductVariant CreateDestinationInstance(ProductVariantPrice source)
        {
            return new ProductVariant(long.Parse(source.VariantId));
        }
    }
}