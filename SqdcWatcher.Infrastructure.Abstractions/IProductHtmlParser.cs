using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataTransferObjects.RestApiModels;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface IProductHtmlParser
    {
        Task<ProductPageResult> ParseProductsPage(string rawHtml, CancellationToken cancelToken);
    }
}