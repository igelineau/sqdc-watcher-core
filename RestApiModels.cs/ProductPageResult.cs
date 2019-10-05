using System.Collections.Generic;

namespace SqdcWatcher.DataObjects
{
    public class ProductPageResult
    {
        public int PageNumber { get; }
        public List<Product> Products { get; }
        public bool HasNextPage { get; set; }

        public ProductPageResult(int pageNumber)
        {
            PageNumber = pageNumber;
            Products = new List<Product>();
        }
    }
}