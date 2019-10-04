using RestSharp;
using System;
using System.Collections.Generic;
using AngleSharp;
using System.Threading.Tasks;
using AngleSharp.Dom;
using System.Linq;

namespace SqdcWatcher
{
    public class SqdcWebClient : ISqdcClient
    {
        private const string BASE_DOMAIN = "https://sqdc.ca";
        private const string DEFAULT_LOCALE = "en-CA";

        private readonly RestClient client;
        private readonly IBrowsingContext htmlContext;

        public SqdcWebClient()
        {
            
            client = new RestClient();
            client.BaseUrl = new Uri($"{BASE_DOMAIN}/{DEFAULT_LOCALE}");
            client.AddDefaultHeader("User-Agent", "Sqdc Watcher");
            client.AddDefaultHeader("Accept", "application/json, text/javascript, */*; q=0.01");

            var htmlParserConfig = Configuration.Default;
            htmlContext = BrowsingContext.New(htmlParserConfig);
        }

        public async Task<List<ProductSummary>> GetProductSummaries()
        {
            var completeList = new List<ProductSummary>();
            ProductPageResult pageResult;
            int currentPage = 1;
            bool hasReachedEnd = false;

            do
            {
                pageResult = await GetProductSummariesPage(currentPage);
                if(pageResult.Products.Any())
                {
                    completeList.AddRange(pageResult.Products);
                    currentPage++;
                }
                else
                {
                    hasReachedEnd = true;
                }
            }
            while (!hasReachedEnd);

            return completeList;
        }

        public async Task<ProductPageResult> GetProductSummariesPage(int pageNumber)
        {
            var request = new RestRequest("Search");
            request.AddQueryParameter("SortDirection", "asc");
            request.AddQueryParameter("page", pageNumber.ToString());
            request.AddQueryParameter("keywords", "*");
            
            IRestResponse response = await client.ExecuteTaskAsync(request);
            CheckResponseSuccess(response);

            var pageResult = new ProductPageResult(pageNumber);
            using (IDocument htmlDoc = await htmlContext.OpenAsync(req => req.Content(response.Content)))
            {
                IHtmlCollection<IElement> productsElements = htmlDoc.DocumentElement.QuerySelectorAll("div.product-tile");
                foreach(IElement productElement in productsElements)
                {
                    IElement titleAnchor = productElement.QuerySelector("a[data-qa=\"search-product-title\"]");
                    string title = titleAnchor.TextContent;
                    string url = BASE_DOMAIN + titleAnchor.GetAttribute("href");
                    string id = titleAnchor.GetAttribute("data-productid");

                    var productSummary = new ProductSummary
                    {
                        Id = id,
                        Title = title,
                        Url = url
                    };
                    pageResult.Products.Add(productSummary);
                }
            }

            return pageResult;
        }

        private void CheckResponseSuccess(IRestResponse response) => CheckResponseSuccess(response, "error while fetching an HTTP resource");

        private void CheckResponseSuccess(IRestResponse response, string exceptionMessage)
        {
            if(!response.IsSuccessful)
            {
                throw new SqdcHttpClientException(exceptionMessage, response.ErrorException);
            }
        }
    }
}