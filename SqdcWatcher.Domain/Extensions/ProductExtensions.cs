using System.Collections.Generic;
using System.Linq;
using XFactory.SqdcWatcher.Data.Entities.Products;

namespace XFactory.SqdcWatcher.Data.Entities
{
    public static class ProductExtensions
    {
        public static ProductVariant.ProductVariant GetVariantById(this Product product, long variantId)
        {
            return product.Variants.FirstOrDefault(v => v.Id == variantId);
        }

        public static IEnumerable<ProductVariant.ProductVariant> GetNewVariants(this Product product)
        {
            return product.Variants.Where(v => v.IsNew());
        }
    }
}