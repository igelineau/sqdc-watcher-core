using System.Collections.Generic;
using System.Linq;

namespace SqdcWatcher.RestApiModels.cs
{
    public class ProductDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        
        public string Brand { get; set; }

        public List<ProductVariantDto> Variants { get; set; }
        
        public ProductDto()
        {
            Variants = new List<ProductVariantDto>();
        }
    }
}