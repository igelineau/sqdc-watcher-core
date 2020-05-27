using System;
using System.Collections.Generic;

namespace SqdcWatcher.DataTransferObjects.RestApiModels
{
    public class ProductPageResult
    {
        public ProductPageResult(List<ProductDto> products, List<Exception> exceptions)
        {
            Products = products;
            Exceptions = exceptions;
        }
        
        public List<ProductDto> Products { get; }
        public List<Exception> Exceptions { get; }
    }
}