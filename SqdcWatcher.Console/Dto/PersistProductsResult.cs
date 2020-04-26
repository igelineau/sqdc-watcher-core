#region

using System.Collections.Generic;
using XFactory.SqdcWatcher.Data.Entities;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Dto
{
    public class PersistProductsResult
    {
        public List<Product> AllProducts { get; set; }

        public List<Product> NewProducts { get; set; } = new List<Product>();

        public List<Product> ProductsInStock { get; set; } = new List<Product>();

        /// <summary>
        ///     Variants that just became in stock and for which we should send a notification.
        /// </summary>
        public Dictionary<Product, List<ProductVariant>> NewVariantsInStock { get; set; } = new Dictionary<Product, List<ProductVariant>>();
    }
}