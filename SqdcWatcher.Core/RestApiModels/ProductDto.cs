

using System.Collections.Generic;



namespace XFactory.SqdcWatcher.Core.RestApiModels
{
    public class ProductDto
    {
        public ProductDto()
        {
            Variants = new List<ProductVariantDto>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public string Brand { get; set; }

        public List<ProductVariantDto> Variants { get; set; }

        public override string ToString() => $"Product Id= {Id}, Title={Title}, Brand={Brand}";
    }
}