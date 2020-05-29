using System.Collections.Generic;

namespace SqdcWatcher.DataTransferObjects.RestApiModels
{
    public class ProductDto
    {
        public ProductDto(string id, string url)
        {
            Id = id;
            Url = url;
            Variants = new List<ProductVariantDto>();
        }

        public string Id { get; private set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public string Brand { get; set; }

        public List<ProductVariantDto> Variants { get; set; }
        public string Strain { get; set; }
        public string CannabisType { get; set; }
        public string ProducerName { get; set; }
        public string LevelTwoCategory { get; set; }

        public override string ToString() => $"Product Id= {Id}, Title={Title}, Brand={Brand}";
    }
}