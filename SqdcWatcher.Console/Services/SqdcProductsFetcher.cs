#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Models.EntityFramework;
using SqdcWatcher.DataAccess.EntityFramework;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.RestApiModels;

#endregion

namespace SqdcWatcher.Services
{
    public class SqdcProductsFetcher
    {
        private readonly SqdcWebClient htmlClient;
        private readonly ILogger<SqdcProductsFetcher> logger;
        private readonly SqdcRestApiClient restClient;
        private readonly ISqdcDataAccess sqdcDataAccess;

        public SqdcProductsFetcher(
            SqdcWebClient htmlClient,
            SqdcRestApiClient restClient,
            ISqdcDataAccess sqdcDataAccess,
            ILogger<SqdcProductsFetcher> logger)
        {
            this.htmlClient = htmlClient;
            this.restClient = restClient;
            this.sqdcDataAccess = sqdcDataAccess;
            this.logger = logger;
        }

        public async Task<ProductsListResult> FetchProductsFromApi(
            FetchProductsOptions infoDto,
            CancellationToken cancelToken,
            bool forceFullRefresh)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Dictionary<string, ProductDto> products;

            if (true)
            {
                logger.LogInformation("Updating Products List from SQDC Website...");
                products = await htmlClient.GetProductSummaries(cancelToken);

                cancelToken.ThrowIfCancellationRequested();
                VariantsPricesResponse pricesResponse = await restClient.GetVariantsPrices(products.Keys, cancelToken);
                MergeVariantsIntoProducts(products, pricesResponse.ProductPrices);
            }
            else
            {
                logger.LogInformation("Using Product List cached from the local DB...");
                products = await LoadProductsListFromCachedDatabase();
            }

            Dictionary<long, ProductVariantDto> variantsMap =
                products.Values.SelectMany(p => p.Variants).ToDictionary(v => v.Id);

            cancelToken.ThrowIfCancellationRequested();
            await UpdateInStockStatuses(variantsMap, cancelToken);

            List<ProductVariantDto> variantsToUpdateSpecs = variantsMap.Values.Where(v => !infoDto.VariantsWithUpToDateSpecs.Contains(v.Id)).ToList();
            await FetchVariantsSpecifications(variantsToUpdateSpecs, cancelToken);

            logger.LogInformation($"Products fetched from the API in {Math.Round(sw.Elapsed.TotalSeconds, 1)}s");
            return new ProductsListResult
            {
                Products = products,
                RemoteFetchPerformed = true
            };
        }

        private async Task<Dictionary<string, ProductDto>> LoadProductsListFromCachedDatabase()
        {
            Dictionary<string, Product> allDbProducts = (await sqdcDataAccess.GetProductsSummary())
                .ToDictionary(p => p.Id);
            return allDbProducts.Values.Select(dbProd =>
            {
                var prodDto = new ProductDto
                {
                    Id = dbProd.Id,
                    Title = dbProd.Title,
                    Brand = dbProd.Brand,
                    Url = dbProd.Url
                };
                prodDto.Variants.MergeListById(
                    dbProd.Variants,
                    o => o.Id,
                    o => o.Id);
                foreach (ProductVariantDto pv in prodDto.Variants)
                {
                    pv.Product = prodDto;
                }

                return prodDto;
            }).ToDictionary(p => p.Id);
        }

        private async Task FetchVariantsSpecifications(List<ProductVariantDto> variants, CancellationToken cancelToken)
        {
            foreach (ProductVariantDto variant in variants)
            {
                logger.LogInformation($"Fetching specifications for productId={variant.Product.Id}, variantId={variant.Id}");
                try
                {
                    SpecificationsResponse response =
                        await restClient.GetSpecifications(variant.Product.Id, variant.Id.ToString(), cancelToken);
                    List<SpecificationAttributeDto> attributes = response.Groups.SelectMany(g => g.Attributes).ToList();
                    attributes.ForEach(a => a.ProductVariant = variant);
                    variant.Specifications = attributes;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error fetching or parsing the specifications of {variant}");
                }
            }
        }

        private async Task UpdateInStockStatuses(Dictionary<long, ProductVariantDto> variants, CancellationToken cancelToken)
        {
            List<long> variantsInStock = await restClient.GetInventoryItems(variants.Keys, cancelToken);
            variantsInStock.ForEach(vId => variants[vId].InStock = true);
        }

        private void MergeVariantsIntoProducts(Dictionary<string, ProductDto> productsTarget, List<ProductPrice> productPrices)
        {
            foreach (ProductPrice productPrice in productPrices)
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