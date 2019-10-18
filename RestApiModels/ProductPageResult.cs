using System.Collections.Generic;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.RestApiModels.cs
{
    public class ProductPageResult
    {
        public int PageNumber { get; }
        public List<ProductDto> Products { get; }
        public bool HasNextPage { get; set; }

        public ProductPageResult(int pageNumber)
        {
            PageNumber = pageNumber;
            Products = new List<ProductDto>();
        }
    }
}