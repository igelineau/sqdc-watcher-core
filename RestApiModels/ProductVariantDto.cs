using System.Collections.Generic;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Services;

namespace SqdcWatcher.RestApiModels.cs
{
    public class ProductVariantDto
    {
        public long Id { get; set; }
        public List<SpecificationAttributeDto> Specifications { get; set; }
        public ProductDto Product { get; set; }
        public bool InStock { get; set; }
        public ProductVariantPrice PriceInfo { get; set; }
    }
}