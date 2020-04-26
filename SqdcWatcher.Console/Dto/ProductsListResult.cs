#region

using System.Collections.Generic;
using SqdcWatcher.RestApiModels;

#endregion

namespace SqdcWatcher.Dto
{
    public class ProductsListResult
    {
        public Dictionary<string, ProductDto> Products { get; set; }

        public bool RemoteFetchPerformed { get; set; }
    }
}