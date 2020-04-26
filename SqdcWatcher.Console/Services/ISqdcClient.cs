#region

using System.Collections.Generic;
using System.Threading.Tasks;
using XFactory.SqdcWatcher.ConsoleApp.RestApiModels;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Services
{
    public interface ISqdcClient
    {
        Task<Dictionary<string, ProductDto>> GetProductSummaries();
    }
}