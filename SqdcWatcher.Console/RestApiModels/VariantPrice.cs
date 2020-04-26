namespace SqdcWatcher.DataObjects
{
    public class ProductVariantPrice
    {
        public string VariantId { get; set; }
        public bool IsPriceDiscounted { get; set; }
        public string DefaultListPrice { get; set; }
        public string ListPrice { get; set; }
        public string DisplayPrice { get; set; }
        public string PricePerGram { get; set; }
    }
}