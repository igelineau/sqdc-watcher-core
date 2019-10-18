using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using AngleSharp.Common;
using AutoMapper;
using ServiceStack.OrmLite;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher.Services
{
    public class ProductsPersister
    {
        private readonly DataAccess dataAccess;
        private readonly MapperConfiguration mapperConfiguration;
        private readonly BecameInStockTriggerPolicy becameInStockTriggerPolicy;

        public ProductsPersister(DataAccess dataAccess, MapperConfiguration mapperConfiguration, BecameInStockTriggerPolicy becameInStockTriggerPolicy)
        {
            this.dataAccess = dataAccess;
            this.mapperConfiguration = mapperConfiguration;
            this.becameInStockTriggerPolicy = becameInStockTriggerPolicy;
        }

        public PersistProductsResult PersistMergeProducts(List<ProductDto> products, Dictionary<string, Product> dbProductsMap)
        {
            return PersistProducts(products, dbProductsMap);
        }
        
        private PersistProductsResult PersistProducts(List<ProductDto> freshProducts, Dictionary<string, Product> dbProductsMap)
        {
            int newProducts = 0;
            int updatedProducts = 0;
            var productsToUpdate = new List<Product>();
            var persistResult = new PersistProductsResult
            {
                AllProducts = productsToUpdate
            };
            foreach (ProductDto freshProduct in freshProducts)
            {
                if (!dbProductsMap.TryGetValue(freshProduct.Id, out Product dbProduct))
                {
                    dbProduct = new Product();
                    newProducts++;
                    persistResult.NewProducts.Add(dbProduct);
                }
                else
                {
                    updatedProducts++;
                }

                ProductMapResult mapResult = MapProductDto(freshProduct, dbProduct);

                if(mapResult.NewVariantsInStock.Any())
                {
                    persistResult.NewVariantsInStock.Add(mapResult.MappedProduct, mapResult.NewVariantsInStock);
                }
                if (mapResult.MappedProduct.Variants.Any(v => v.InStock))
                {
                    persistResult.ProductsInStock.Add(mapResult.MappedProduct);
                }
                
                productsToUpdate.Add(mapResult.MappedProduct);
            }

            MoveKnownSpecsToParentObjects(productsToUpdate);
            dataAccess.SaveProducts(productsToUpdate);

            Console.WriteLine(
                $"Online search found products {freshProducts.Count} ({newProducts} new, {updatedProducts} changed).");

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
                
                ProductVariantMapResult variantMapResult = MapProductVariantDto(pv, destVariant);
                if (variantMapResult.HasBecomeInStock)
                {
                    mapResult.NewVariantsInStock.Add(variantMapResult.MappedVariant);
                }
            }

            return mapResult;
        }

        private ProductVariantMapResult MapProductVariantDto(ProductVariantDto source, ProductVariant destination)
        {
            var mapResult = new ProductVariantMapResult
            {
                MappedVariant = destination,
                HasBecomeInStock = !destination.InStock && source.InStock && !becameInStockTriggerPolicy.ShouldIgnoreInStockChange(destination)
            };

            destination.Id = source.Id;
            destination.InStock = source.InStock;
            destination.ProductId = source.Product.Id;
            destination.PriceInfo = source.PriceInfo;

            if (source.Specifications != null && source.Specifications.Any())
            {
                destination.Specifications.MergeListById(source.Specifications, target => target.PropertyName);
            }

            return mapResult;
        }
        
        private static void MoveKnownSpecsToParentObjects(List<Product> products)
        {
            foreach (Product p in products)
            {
                bool hasAssignedProduct = false;
                foreach(ProductVariant variant in p.Variants)
                {
                    List<string> specsNamesToRemove = new List<string>();
                    if(!hasAssignedProduct)
                    {
                        specsNamesToRemove = SpecificationCopier.CopySpecificationsToObject(p, variant.Specifications);
                    }

                    IEnumerable<string> allSpecsToRemove =
                            specsNamesToRemove.Union(SpecificationCopier.CopySpecificationsToObject(variant, variant.Specifications));
                    variant.Specifications.RemoveAll(spec => allSpecsToRemove.Contains(spec.PropertyName));
                }
            }
        }

    }
}