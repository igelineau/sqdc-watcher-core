

using System.Collections.Generic;



namespace XFactory.SqdcWatcher.Core.RestApiModels
{
    public class ProductPrice
    {
        public string ProductId { get; set; }
        public bool IsPriceDiscounted { get; set; }
        public string DefaultListPrice { get; set; }
        public string ListPrice { get; set; }
        public List<ProductVariantPrice> VariantPrices { get; set; }
        public object DisplayPrice { get; set; }
        public object PricePerGram { get; set; }
    }
}