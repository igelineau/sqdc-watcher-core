using System.Collections.Generic;
using System.Threading.Tasks;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher
{
    public interface ISqdcClient
    {
        Task<List<ProductDto>> GetProductSummaries();
    }
}