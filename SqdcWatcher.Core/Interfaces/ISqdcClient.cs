

using System.Collections.Generic;
using System.Threading.Tasks;
using XFactory.SqdcWatcher.Core.RestApiModels;



namespace XFactory.SqdcWatcher.Core.Interfaces
{
    public interface ISqdcClient
    {
        Task<Dictionary<string, ProductDto>> GetProductSummaries();
    }
}