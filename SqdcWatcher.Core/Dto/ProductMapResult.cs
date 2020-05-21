

using System.Collections.Generic;
using XFactory.SqdcWatcher.Data.Entities;



namespace XFactory.SqdcWatcher.Core.Dto
{
    public class ProductMapResult
    {
        public Product MappedProduct { get; set; }
        public List<ProductVariant> NewVariants { get; set; } = new List<ProductVariant>();
    }
}