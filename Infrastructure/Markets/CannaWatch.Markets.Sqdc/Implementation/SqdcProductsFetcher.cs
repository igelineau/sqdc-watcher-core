using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CannaWatch.Markets.Sqdc.HttpClient;
using Microsoft.Extensions.Logging;
using RestSharp;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;

namespace CannaWatch.Markets.Sqdc.Implementation
{
    public class SqdcProductsFetcher : SqdcHttpClientBase, IRemoteStore<SqdcMarketFacade, ProductDto>
    {
        private readonly ILogger<SqdcProductsFetcher> logger;
        private readonly SqdcHtmlParser sqdcHtmlParser;

        public SqdcProductsFetcher(SqdcHtmlParser productHtmlParser, ILogger<SqdcProductsFetcher> logger)
            : base($"{BaseDomain}/{DefaultLocale}")
        {
            Client.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

            this.logger = logger;
            sqdcHtmlParser = productHtmlParser;
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
            logger.LogDebug($"Loaded products page {pageNumber} in {sw.ElapsedMilliseconds}ms");
            EnsureResponseSuccess(response);
            
            return await sqdcHtmlParser.ParseProductsPage(response.Content, cancelToken);
        }

        private static void EnsureResponseSuccess(IRestResponse response) =>
            EnsureResponseSuccess(response, "error while fetching an HTTP resource");

        private static void EnsureResponseSuccess(IRestResponse response, string exceptionMessage)
        {
            if (!response.IsSuccessful)
            {
                throw new SqdcHttpClientException(exceptionMessage, response.ErrorException);
            }
        }
    }
}