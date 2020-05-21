using System.Linq;

namespace XFactory.SqdcWatcher.Data.Entities
{
    public static class ProductExtensions
    {
        public static ProductVariant GetVariantById(this Product product, long variantId)
        {
            return product.Variants.FirstOrDefault(v => v.Id == variantId);
        }
    }
}