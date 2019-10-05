namespace SqdcWatcher.DataObjects
{
    public class ProductVariantPrice
    {
        public string VariantId { get; set; }
        public bool IsPriceDiscounted { get; set; }
        public string DefaultListPrice { get; set; }
        public string ListPrice { get; set; }
        public string JsonContext { get; set; }
        public string DisplayPrice { get; set; }
        public object PricePerGram { get; set; }
    }
}