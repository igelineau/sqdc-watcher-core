using System.Collections.Generic;
using System.Threading.Tasks;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher
{
    public interface ISqdcClient
    {
        Task<List<Product>> GetProductSummaries();
    }
}