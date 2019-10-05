using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher.Services
{
    public class SqdcProductsFetcher
    {
        private readonly SqdcWebClient htmlClient;
        private readonly SqdcRestApiClient restClient;
        private readonly DataAccess dataAccess;

        public SqdcProductsFetcher(SqdcWebClient htmlClient, SqdcRestApiClient restClient, DataAccess dataAccess)
        {
            this.htmlClient = htmlClient;
            this.restClient = restClient;
            this.dataAccess = dataAccess;
        }

        public async Task<List<Product>> GetProducts()
        {
            Dictionary<string, Product> productSummaries = (await htmlClient.GetProductSummaries()).ToDictionary(p => p.Id);
            VariantsPricesResponse response = await restClient.GetVariantsPrices(productSummaries.Keys);
            MergeVariantsIntoProducts(productSummaries, response.ProductPrices);
            
            return productSummaries.Values.ToList();
        }

        private void MergeVariantsIntoProducts(Dictionary<string, Product> productsTarget, List<ProductPrice> productPrices)
        {
            foreach(ProductPrice productPrice in productPrices)
            {
                Product product = productsTarget[productPrice.ProductId];
                AddVariantsToProduct(product, productPrice.VariantPrices);
            }
        }

        private void AddVariantsToProduct(Product product, List<ProductVariantPrice> variantPrices)
        {
            product.Variants.AddRange(variantPrices.Select(
                vp => new ProductVariant
                {
                    Id = long.Parse(vp.VariantId),
                    Product = product,
                    PriceInfo = vp
                })
            );
        }
    }
}