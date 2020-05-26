using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using RestSharp;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Core.HttpClient;
using XFactory.SqdcWatcher.Core.Services;

namespace XFactory.SqdcWatcher.Core.SiteCrawling
{
    public class SqdcProductsFetcher : SqdcHttpClientBase, IRemoteStore<ProductDto>
    {
        private readonly IBrowsingContext htmlContext;
        private readonly ILogger<SqdcProductsFetcher> logger;

        public SqdcProductsFetcher(ILogger<SqdcProductsFetcher> logger) : base($"{BaseDomain}/{DefaultLocale}")
        {
            Client.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            this.logger = logger;

            IConfiguration htmlParserConfig = AngleSharp.Configuration.Default;
            htmlContext = BrowsingContext.New(htmlParserConfig);
        }

        public async IAsyncEnumerable<ProductDto> GetAllItemsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var completeList = new Dictionary<string, ProductDto>();
            int currentPage = 1;
            bool hasReachedEnd = false;
            var sw = Stopwatch.StartNew();

            do
            {
                ProductPageResult pageResult = await GetProductSummariesPage(currentPage, cancellationToken);
                foreach (ProductDto productDto in pageResult.Products)
                    if (!completeList.ContainsKey(productDto.Id))
                    {
                        completeList.Add(productDto.Id, productDto);
                        yield return productDto;
                    }

                if (pageResult.Products.Any())
                    currentPage++;
                else
                    hasReachedEnd = true;
            } while (!hasReachedEnd);

            logger.LogInformation($"{completeList.Count} products discovered from the SQDC website in {Math.Round(sw.Elapsed.TotalSeconds)}s");
        }

        private async Task<ProductPageResult> GetProductSummariesPage(int pageNumber, CancellationToken cancelToken)
        {
            var request = new RestRequest("Search");
            request.AddQueryParameter("SortDirection", "asc");
            request.AddQueryParameter("page", pageNumber.ToString());
            request.AddQueryParameter("keywords", "*");

            var sw = Stopwatch.StartNew();
            IRestResponse response = await Client.ExecuteAsync(request, cancelToken);
            logger.LogDebug($"Loaded SQDC products page {pageNumber} in {sw.ElapsedMilliseconds}ms");
            EnsureResponseSuccess(response);

            var pageResult = new ProductPageResult(pageNumber);
            using IDocument htmlDoc = await htmlContext.OpenAsync(req => req.Content(response.Content), cancelToken);
            IHtmlCollection<IElement> productsElements = htmlDoc.DocumentElement.QuerySelectorAll("div.product-tile");
            foreach (IElement productElement in productsElements)
            {
                IElement titleAnchor = productElement.QuerySelector("a[data-qa=\"search-product-title\"]");
                string title = titleAnchor.TextContent;

                IElement brandElement = productElement.QuerySelector("div[class=\"js-equalized-brand\"]");
                string brand = brandElement.TextContent;

                string url = BaseDomain + titleAnchor.GetAttribute("href");
                string id = titleAnchor.GetAttribute("data-productid");

                var productSummary = new ProductDto
                {
                    Id = id,
                    Title = title,
                    Url = url,
                    Brand = brand
                };
                pageResult.Products.Add(productSummary);
            }

            return pageResult;
        }

        private static void EnsureResponseSuccess(IRestResponse response) =>
            EnsureResponseSuccess(response, "error while fetching an HTTP resource");

        private static void EnsureResponseSuccess(IRestResponse response, string exceptionMessage)
        {
            if (!response.IsSuccessful) throw new SqdcHttpClientException(exceptionMessage, response.ErrorException);
        }
    }
}