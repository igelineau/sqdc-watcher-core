#region

using System.Collections.Generic;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.RestApiModels
{
    public class ProductPageResult
    {
        public ProductPageResult(int pageNumber)
        {
            PageNumber = pageNumber;
            Products = new List<ProductDto>();
        }

        public int PageNumber { get; }
        public List<ProductDto> Products { get; }
        public bool HasNextPage { get; set; }
    }
}