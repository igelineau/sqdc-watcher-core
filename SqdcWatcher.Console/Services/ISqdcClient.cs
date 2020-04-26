#region

using System.Collections.Generic;
using System.Threading.Tasks;
using SqdcWatcher.RestApiModels;

#endregion

namespace SqdcWatcher
{
    public interface ISqdcClient
    {
        Task<Dictionary<string, ProductDto>> GetProductSummaries();
    }
}