

using System.Collections.Generic;



namespace XFactory.SqdcWatcher.Core.RestApiModels
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