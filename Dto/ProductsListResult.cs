using System.Collections.Generic;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher.Dto
{
    public class ProductsListResult
    {
        public Dictionary<string, ProductDto> Products { get; set; }
        
        public bool RemoteFetchPerformed { get; set; }
    }
}