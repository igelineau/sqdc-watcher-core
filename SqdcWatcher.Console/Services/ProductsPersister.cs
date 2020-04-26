#region

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Models.EntityFramework;
using SqdcWatcher.DataAccess.EntityFramework;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.MappingFilters;
using SqdcWatcher.RestApiModels;
using SqdcWatcher.Utils;
using SqdcWatcher.Visitors;

#endregion

namespace SqdcWatcher.Services
{
    public class ProductsPersister
    {
        private readonly ILogger<ProductsPersister> logger;
        private readonly IEnumerable<VisitorBase<Product>> productVisitors;
        private readonly ISqdcDataAccess sqdcDataAccess;
        private readonly IEnumerable<MappingFilterBase<ProductVariantDto, ProductVariant>> variantMappingFilters;
        private readonly IEnumerable<VisitorBase<ProductVariant>> variantVisitors;

        public ProductsPersister(
            ILogger<ProductsPersister> logger,
            ISqdcDataAccess sqdcDataAccess,
            IEnumerable<VisitorBase<Product>> productVisitors,
            IEnumerable<VisitorBase<ProductVariant>> variantVisitors,
            IEnumerable<MappingFilterBase<ProductVariantDto, ProductVariant>> variantMappingFilters)
        {
            this.logger = logger;
            this.sqdcDataAccess = sqdcDataAccess;
            this.variantVisitors = variantVisitors;
            this.variantMappingFilters = variantMappingFilters;
            this.productVisitors = productVisitors.ToList();
        }

        public async Task<PersistProductsResult> PersistMergeProducts(List<ProductDto> products, Dictionary<string, Product> dbProductsMap)
        {
            var productsToUpdate = new List<Product>();
            var persistResult = new PersistProductsResult
            {
                AllProducts = productsToUpdate
            };
            foreach (ProductDto freshProduct in products)
            {
                if (!dbProductsMap.TryGetValue(freshProduct.Id, out Product dbProduct))
                {
                    dbProduct = new Product {IsNew = true};
                    persistResult.NewProducts.Add(dbProduct);
                }

                ProductMapResult mapResult = MapProductDto(freshProduct, dbProduct);

                var variantsBecameInStockToNotify =
                    mapResult.MappedProduct.Variants.Where(v => v.MetaData.HasBecomeInStock && v.MetaData.ShouldSendNotification);
                foreach (ProductVariant variant in variantsBecameInStockToNotify)
                {
                    if (!persistResult.NewVariantsInStock.TryGetValue(mapResult.MappedProduct, out List<ProductVariant> variantsList))
                    {
                        variantsList = new List<ProductVariant>();
                        persistResult.NewVariantsInStock.Add(mapResult.MappedProduct, variantsList);
                    }

                    variantsList.Add(variant);
                }

                if (mapResult.MappedProduct.Variants.Any(v => v.InStock))
                {
                    persistResult.ProductsInStock.Add(mapResult.MappedProduct);
                }

                productsToUpdate.Add(mapResult.MappedProduct);
            }

            Stopwatch sw = Stopwatch.StartNew();
            logger.Log(LogLevel.Information, "Saving all products to DB...");
            await sqdcDataAccess.SaveProducts(productsToUpdate);
            logger.Log(LogLevel.Information, $"Persisted {productsToUpdate.Count} products to DB in {sw.ElapsedMilliseconds}ms");

            return persistResult;
        }

        private ProductMapResult MapProductDto(ProductDto source, Product destination)
        {
            var mapResult = new ProductMapResult
            {
                MappedProduct = destination
            };

            destination.Id = source.Id;
            destination.Title = source.Title;
            destination.Url = source.Url;
            destination.Brand = source.Brand;

            foreach (ProductVariantDto pv in source.Variants)
            {
                ProductVariant destVariant = destination.GetVariantById(pv.Id);
                if (destVariant == null)
                {
                    destVariant = new ProductVariant();
                    destination.Variants.Add(destVariant);
                    mapResult.NewVariants.Add(destVariant);
                }

                MergeProductVariantDto(pv, destVariant);
            }

            return mapResult;
        }

        private void MergeProductVariantDto(ProductVariantDto source, ProductVariant destination)
        {
            destination.Id = source.Id;
            destination.ProductId = source.Product.Id;

            foreach (MappingFilterBase<ProductVariantDto, ProductVariant> filter in variantMappingFilters)
            {
                filter.Apply(source, destination);
            }

            destination.InStock = source.InStock;
            MapPriceInfo(destination, source.PriceInfo);

            if (source.Specifications != null && source.Specifications.Any())
            {
                destination.Specifications.MergeListById(
                    source.Specifications,
                    s => s.PropertyName,
                    t => t.PropertyName);
            }
        }

        private static void MapPriceInfo(ProductVariant destination, ProductVariantPrice sourcePriceInfo)
        {
            if (sourcePriceInfo == null)
            {
                return;
            }

            var parsedPrices = PriceParser.ParseVariantPrice(sourcePriceInfo);
            if (parsedPrices.ListPrice != null)
            {
                destination.ListPrice = parsedPrices.ListPrice.Value;
            }

            if (parsedPrices.DisplayPrice != null)
            {
                destination.DisplayPrice = parsedPrices.DisplayPrice.Value;
            }

            if (parsedPrices.PricePerGram != null)
            {
                destination.PricePerGram = parsedPrices.PricePerGram.Value;
            }
        }
    }
}