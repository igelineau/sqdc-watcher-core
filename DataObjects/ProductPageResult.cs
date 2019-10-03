using System.Collections.Generic;

namespace SqdcWatcher
{
    public class ProductPageResult
    {
        public int PageNumber { get; }
        public List<ProductSummary> Products { get; }
        public bool HasNextPage { get; set; }

        public ProductPageResult(int pageNumber)
        {
            PageNumber = pageNumber;
            Products = new List<ProductSummary>();
        }
    }
}