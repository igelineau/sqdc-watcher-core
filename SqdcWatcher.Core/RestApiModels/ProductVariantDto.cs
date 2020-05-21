

using System.Collections.Generic;
using XFactory.SqdcWatcher.Core.Services;



namespace XFactory.SqdcWatcher.Core.RestApiModels
{
    public class ProductVariantDto
    {
        public long Id { get; set; }
        public List<SpecificationAttributeDto> Specifications { get; set; }
        public ProductDto Product { get; set; }
        public bool InStock { get; set; }
        public ProductVariantPrice PriceInfo { get; set; }

        public override string ToString() => $"{Product} / VariantId={Id}";
    }
}