using System.Collections.Generic;
using System.Threading.Tasks;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels;

namespace SqdcWatcher
{
    public interface ISqdcClient
    {
        Task<Dictionary<string, ProductDto>> GetProductSummaries();
    }
}