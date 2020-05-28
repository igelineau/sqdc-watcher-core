using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using CannaWatch.Markets.Sqdc.HttpClient;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using SqdcWatcher.Infrastructure.Abstractions;

namespace CannaWatch.Markets.Sqdc.Implementation
{
    public class SqdcHtmlParser : IProductHtmlParser
    {
        private static readonly IConfiguration HtmlParserConfig = AngleSharp.Configuration.Default;

        public async Task<ProductPageResult> ParseProductsPage(string rawHtml, CancellationToken cancelToken)
        {
            var parsedProducts = new List<ProductDto>();
            var exceptions = new List<Exception>();
            using IBrowsingContext htmlContext = BrowsingContext.New(HtmlParserConfig);
            using IDocument htmlDoc = await htmlContext.OpenAsync(req => req.Content(rawHtml), cancelToken);
            IHtmlCollection<IElement> productsElements = htmlDoc.DocumentElement.QuerySelectorAll("div.product-tile");
            foreach (IElement productElement in productsElements)
            {
                try
                {
                    ProductDto productSummary = ParseProductElement(productElement);
                    parsedProducts.Add(productSummary);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
            
            return new ProductPageResult(parsedProducts, exceptions);
        }

        private ProductDto ParseProductElement(IElement productElement)
        {
            IElement titleAnchor = productElement.QuerySelector("a[data-qa=\"search-product-title\"]");
            string title = titleAnchor.TextContent;

            IElement brandElement = productElement.QuerySelector("div[class=\"js-equalized-brand\"]");
            string brand = brandElement.TextContent;

            string url = SqdcHttpClientBase.BaseDomain + titleAnchor.GetAttribute("href");
            string id = titleAnchor.GetAttribute("data-productid");

            var productSummary = new ProductDto
            {
                Id = id,
                Title = title,
                Url = url,
                Brand = brand
            };
            return productSummary;
        }
    }
}