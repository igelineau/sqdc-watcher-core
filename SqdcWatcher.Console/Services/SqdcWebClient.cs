#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using RestSharp;
using SqdcWatcher.RestApiModels;

#endregion

namespace SqdcWatcher.Services
{
    public class SqdcWebClient : SqdcHttpClientBase
    {
        private readonly IBrowsingContext htmlContext;
        private readonly ILogger<SqdcWebClient> logger;

        public SqdcWebClient(ILogger<SqdcWebClient> logger) : base($"{BASE_DOMAIN}/{DEFAULT_LOCALE}")
        {
            client.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            this.logger = logger;
            IConfiguration htmlParserConfig = Configuration.Default;
            htmlContext = BrowsingContext.New(htmlParserConfig);
        }

        public async Task<Dictionary<string, ProductDto>> GetProductSummaries(CancellationToken cancelToken)
        {
            var completeList = new Dictionary<string, ProductDto>();
            int currentPage = 1;
            bool hasReachedEnd = false;
            Stopwatch sw = Stopwatch.StartNew();

            do
            {
                ProductPageResult pageResult = await GetProductSummariesPage(currentPage, cancelToken);
                foreach (ProductDto productDto in pageResult.Products)
                {
                    if (!completeList.ContainsKey(productDto.Id))
                    {
                        completeList.Add(productDto.Id, productDto);
                    }
                }

                if (pageResult.Products.Any())
                {
                    currentPage++;
                }
                else
                {
                    hasReachedEnd = true;
                }
            } while (!hasReachedEnd);

            logger.LogInformation($"{currentPage - 1} pages fetched from SQDC HTML website in {Math.Round(sw.Elapsed.TotalSeconds)}s");
            return completeList;
        }

        public async IAsyncEnumerable<ProductDto> GetProductSummariesAsync()
        {
            var completeList = new Dictionary<string, ProductDto>();
            int currentPage = 1;
            bool hasReachedEnd = false;
            Stopwatch sw = Stopwatch.StartNew();

            do
            {
                ProductPageResult pageResult = await GetProductSummariesPage(currentPage);
                foreach (ProductDto productDto in pageResult.Products)
                {
                    if (!completeList.ContainsKey(productDto.Id))
                    {
                        completeList.Add(productDto.Id, productDto);
                        yield return productDto;
                    }
                }

                if (pageResult.Products.Any())
                {
                    currentPage++;
                }
                else
                {
                    hasReachedEnd = true;
                }
            } while (!hasReachedEnd);

            logger.LogInformation($"{currentPage - 1} pages fetched from SQDC HTML website in {Math.Round(sw.Elapsed.TotalSeconds)}s");
        }

        public async Task<ProductPageResult> GetProductSummariesPage(int pageNumber)
        {
            return await GetProductSummariesPage(pageNumber, CancellationToken.None);
        }

        public async Task<ProductPageResult> GetProductSummariesPage(int pageNumber, CancellationToken cancelToken)
        {
            var request = new RestRequest("Search");
            request.AddQueryParameter("SortDirection", "asc");
            request.AddQueryParameter("page", pageNumber.ToString());
            request.AddQueryParameter("keywords", "*");

            Stopwatch sw = Stopwatch.StartNew();
            IRestResponse response = await client.ExecuteAsync(request, cancelToken);
            logger.LogDebug($"Loaded SQDC products page {pageNumber} in {sw.ElapsedMilliseconds}ms");
            CheckResponseSuccess(response);

            var pageResult = new ProductPageResult(pageNumber);
            using (IDocument htmlDoc = await htmlContext.OpenAsync(req => req.Content(response.Content), cancelToken))
            {
                IHtmlCollection<IElement> productsElements = htmlDoc.DocumentElement.QuerySelectorAll("div.product-tile");
                foreach (IElement productElement in productsElements)
                {
                    IElement titleAnchor = productElement.QuerySelector("a[data-qa=\"search-product-title\"]");
                    string title = titleAnchor.TextContent;

                    IElement brandElement = productElement.QuerySelector("div[class=\"js-equalized-brand\"]");
                    string brand = brandElement.TextContent;

                    string url = BASE_DOMAIN + titleAnchor.GetAttribute("href");
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
            }

            return pageResult;
        }

        private void CheckResponseSuccess(IRestResponse response) => CheckResponseSuccess(response, "error while fetching an HTTP resource");

        private void CheckResponseSuccess(IRestResponse response, string exceptionMessage)
        {
            if (!response.IsSuccessful)
            {
                throw new SqdcHttpClientException(exceptionMessage, response.ErrorException);
            }
        }
    }
}