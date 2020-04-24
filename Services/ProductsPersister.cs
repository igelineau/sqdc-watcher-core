using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using AngleSharp.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ServiceStack.OrmLite;
using SqdcWatcher.DataAccess;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.MappingFilters;
using SqdcWatcher.RestApiModels;
using SqdcWatcher.Utils;
using SqdcWatcher.Visitors;

namespace SqdcWatcher.Services
{
    public class ProductsPersister
    {
        private readonly ILogger<ProductsPersister> logger;
        private readonly SqdcDataAccess sqdcDataAccess;
        private readonly MapperConfiguration mapperConfiguration;
        private readonly BecameInStockTriggerPolicy becameInStockTriggerPolicy;
        private readonly IEnumerable<VisitorBase<ProductVariant>> variantVisitors;
        private readonly IEnumerable<MappingFilterBase<ProductVariantDto, ProductVariant>> variantMappingFilters;
        private readonly StockHistoryPersister stockHistoryPersister;
        private readonly IEnumerable<VisitorBase<Product>> productVisitors;

        public ProductsPersister(
            ILogger<ProductsPersister> logger,
            SqdcDataAccess sqdcDataAccess,
            MapperConfiguration mapperConfiguration,
            BecameInStockTriggerPolicy becameInStockTriggerPolicy,
            IEnumerable<VisitorBase<Product>> productVisitors,
            IEnumerable<VisitorBase<ProductVariant>> variantVisitors,
            IEnumerable<MappingFilterBase<ProductVariantDto, ProductVariant>> variantMappingFilters,
            StockHistoryPersister stockHistoryPersister)
        {
            this.logger = logger;
            this.sqdcDataAccess = sqdcDataAccess;
            this.mapperConfiguration = mapperConfiguration;
            this.becameInStockTriggerPolicy = becameInStockTriggerPolicy;
            this.variantVisitors = variantVisitors;
            this.variantMappingFilters = variantMappingFilters;
            this.stockHistoryPersister = stockHistoryPersister;
            this.productVisitors = productVisitors.ToList();
        }

        public PersistProductsResult PersistMergeProducts(List<ProductDto> products, Dictionary<string, Product> dbProductsMap)
        {
            return PersistProducts(products, dbProductsMap);
        }
        
        private PersistProductsResult PersistProducts(List<ProductDto> freshProducts, Dictionary<string, Product> dbProductsMap)
        {
            var productsToUpdate = new List<Product>();
            var persistResult = new PersistProductsResult
            {
                AllProducts = productsToUpdate
            };
            foreach (ProductDto freshProduct in freshProducts)
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

            ApplyVisitors(productVisitors, productsToUpdate);
            ApplyVisitors(variantVisitors, productsToUpdate.SelectMany(p => p.Variants).ToList());

            Stopwatch sw = Stopwatch.StartNew();
            logger.Log(LogLevel.Information, "Saving all products to DB...");
            sqdcDataAccess.SaveProducts(productsToUpdate);
            logger.Log(LogLevel.Information, $"Persisted {productsToUpdate.Count} products to DB in {sw.ElapsedMilliseconds}ms");
            
            return persistResult;
        }

        private void ApplyVisitors<T>(IEnumerable<VisitorBase<T>> visitors, IList<T> itemsToApplyTo)
        {
            foreach (VisitorBase<T> productVisitor in visitors)
            {
                logger.Log(LogLevel.Debug, $"Invoking visitor {productVisitor.GetType().Name} on {itemsToApplyTo.Count} products");
                productVisitor.VisitAll(itemsToApplyTo);
            }
        }

        private ProductMapResult MapProductDto(ProductDto source, Product destination)
        {
            var mapResult = new ProductMapResult
            {
                MappedProduct = destination
            };
            
            destination.ProductId = source.Id;
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
            destination.ProductVariantId = source.Id;
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