using System.Collections.Generic;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Dto
{
    public class ProductMapResult
    {
        public Product MappedProduct { get; set; }
        public List<ProductVariant> NewVariants { get; set; } = new List<ProductVariant>();

        public List<ProductVariant> NewVariantsInStock = new List<ProductVariant>();
    }
}