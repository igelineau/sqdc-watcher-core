

using System.Collections.Generic;
using XFactory.SqdcWatcher.Core.RestApiModels;



namespace XFactory.SqdcWatcher.Core.Dto
{
    public class ProductsListResult
    {
        public Dictionary<string, ProductDto> Products { get; set; }

        public bool RemoteFetchPerformed { get; set; }
    }
}