using System.Threading;
using System.Threading.Tasks;
using SqdcWatcher.DataTransferObjects.RestApiModels;

namespace CannaWatch.Markets.Sqdc.Abstractions
{
    public interface IProductHtmlParser
    {
        Task<ProductPageResult> ParseProductsPage(string rawHtml, CancellationToken cancelToken);
    }
}