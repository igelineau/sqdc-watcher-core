#region

using System.Collections.Generic;
using XFactory.SqdcWatcher.ConsoleApp.RestApiModels;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Dto
{
    public class ProductsListResult
    {
        public Dictionary<string, ProductDto> Products { get; set; }

        public bool RemoteFetchPerformed { get; set; }
    }
}