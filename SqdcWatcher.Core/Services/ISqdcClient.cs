#region

using System.Collections.Generic;
using System.Threading.Tasks;
using XFactory.SqdcWatcher.Core.RestApiModels;

#endregion

namespace XFactory.SqdcWatcher.Core.Services
{
    public interface ISqdcClient
    {
        Task<Dictionary<string, ProductDto>> GetProductSummaries();
    }
}