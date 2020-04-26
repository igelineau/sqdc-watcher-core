#region

using System.Collections.Generic;
using XFactory.SqdcWatcher.Data.Entities;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Dto
{
    public class ProductMapResult
    {
        public Product MappedProduct { get; set; }
        public List<ProductVariant> NewVariants { get; set; } = new List<ProductVariant>();
    }
}