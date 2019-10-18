using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher.Services
{
    public class SqdcProductsFetcher
    {
        private readonly TimeSpan refreshProductsListInterval = TimeSpan.FromMinutes(15);
        
        private readonly SqdcWebClient htmlClient;
        private readonly SqdcRestApiClient restClient;
        private readonly DataAccess dataAccess;

        public SqdcProductsFetcher(SqdcWebClient htmlClient, SqdcRestApiClient restClient, DataAccess dataAccess)
        {
            this.htmlClient = htmlClient;
            this.restClient = restClient;
            this.dataAccess = dataAccess;
        }

        public async Task<ProductsListResult> FetchProductsFromApi(GetProductsInfoDto infoDto)
        {
            Dictionary<string, ProductDto> products;

            DateTime lastProductsListScan = dataAccess.GetLastProductsListUpdateTimestamp();
            bool mustFetchProductsSummariesFromApi = DateTime.Now - lastProductsListScan > refreshProductsListInterval; 
            if (mustFetchProductsSummariesFromApi)
            {
                Console.WriteLine("Updating Products List from SQDC Website...");
                products = (await htmlClient.GetProductSummaries()).ToDictionary(p => p.Id);                
            }
            else
            {
                Console.WriteLine("Using Product List cached from the local DB...");
                products = LoadProductsListFromCachedDatabase();
            }
            
            VariantsPricesResponse pricesResponse = await restClient.GetVariantsPrices(products.Keys);
            MergeVariantsIntoProducts(products, pricesResponse.ProductPrices);

            Dictionary<string, ProductVariantDto> variantsMap =
                products.Values.SelectMany(p => p.Variants).ToDictionary(v => v.Id.ToString());
            
            await UpdateInStockStatuses(variantsMap);

            List<ProductVariantDto> variantsToUpdateSpecs = variantsMap.Values.Where(v => !infoDto.VariantsWithUpToDateSpecs.Contains(v.Id)).ToList();
            await FetchVariantsSpecifications(variantsToUpdateSpecs);

            return new ProductsListResult
            {
                Products = products,
                RemoteFetchPerformed = mustFetchProductsSummariesFromApi
            };
        }

        private Dictionary<string, ProductDto> LoadProductsListFromCachedDatabase()
        {
            List<Product> allDbProducts = dataAccess.GetProductsSummary();
            return allDbProducts.Select(dbProd => new ProductDto
            {
                Id = dbProd.Id,
                Title = dbProd.Title,
                Brand = dbProd.Brand,
                Url = dbProd.Url
            }).ToDictionary(p => p.Id);
        }

        private async Task FetchVariantsSpecifications(List<ProductVariantDto> variants)
        {
            foreach (ProductVariantDto variant in variants)
            {
                Console.WriteLine($"Fetching specifications for productId={variant.Product.Id}, variantId={variant.Id}");
                SpecificationsResponse response =
                    await restClient.GetSpecifications(variant.Product.Id, variant.Id.ToString());
                List<SpecificationAttributeDto> attributes = response.Groups.SelectMany(g => g.Attributes).ToList();
                attributes.ForEach(a => a.ProductVariant = variant);
                variant.Specifications = attributes;
            }
        }

        private async Task UpdateInStockStatuses(Dictionary<string, ProductVariantDto> variants)
        {
            List<string> variantsInStock = await restClient.GetInventoryItems(variants.Keys);
            variantsInStock.ForEach(vId => variants[vId].InStock = true);
        }

        private void MergeVariantsIntoProducts(Dictionary<string, ProductDto> productsTarget, List<ProductPrice> productPrices)
        {
            foreach(ProductPrice productPrice in productPrices)
            {
                ProductDto product = productsTarget[productPrice.ProductId];
                AddVariantsToProduct(product, productPrice.VariantPrices);
            }
        }

        private void AddVariantsToProduct(ProductDto product, List<ProductVariantPrice> variantPrices)
        {
            product.Variants.AddRange(variantPrices.Select(
                vp => new ProductVariantDto
                {
                    Id = long.Parse(vp.VariantId),
                    Product = product,
                    PriceInfo = vp
                })
            );
        }
    }
}