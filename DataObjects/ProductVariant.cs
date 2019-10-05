using ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    public class ProductVariant
    {
        public long Id { get; set; }

        [Reference]
        public Product Product { get; set; }
        public int ProductId { get; set; }

        public ProductVariantPrice PriceInfo { get; set; }
    }
}