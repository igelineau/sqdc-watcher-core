#region

using System.Collections.Generic;
using Models.EntityFramework;

#endregion

namespace SqdcWatcher.Dto
{
    public class ProductMapResult
    {
        public Product MappedProduct { get; set; }
        public List<ProductVariant> NewVariants { get; set; } = new List<ProductVariant>();
    }
}