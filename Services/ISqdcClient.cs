using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqdcWatcher
{
    public interface ISqdcClient
    {
        Task<List<ProductSummary>> GetProductSummaries();
    }
}