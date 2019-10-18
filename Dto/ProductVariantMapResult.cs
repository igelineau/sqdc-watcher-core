using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Dto
{
    public class ProductVariantMapResult
    {
        public ProductVariant MappedVariant { get; set; }
        public bool HasBecomeInStock { get; set; }
    }
}