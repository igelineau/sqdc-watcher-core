using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using SqdcWatcher.DapperExtensions;
using SqdcWatcher.DataObjects;
using SqdcWatcher.Utils;

namespace SqdcWatcher.DataAccess
{
    public class DapperDataAccess
    {
        private readonly IDbConnectionFactory dbConnectionFactory;
        private readonly ILogger<DapperDataAccess> logger;

        public DapperDataAccess(IDbConnectionFactory dbConnectionFactory, ILogger<DapperDataAccess> logger)
        {
            this.dbConnectionFactory = dbConnectionFactory;
            this.logger = logger;
        }

        public async Task<Dictionary<string, Product>> GetProductsSummaryAsync(IDbConnection existingConnection = null)
        {
            using DisposeIfNullWrapper<IDbConnection> cnnWrapper =
                ConditionalDisposeWrapper.Create(existingConnection, dbConnectionFactory.OpenDbConnection);
            Dictionary<string, Product> products = (await cnnWrapper.Object.GetAllAsync<Product>()).ToDictionary(p => p.Id);
            List<ProductVariant> productVariants = (await cnnWrapper.Object.GetAllAsync<ProductVariant>()).ToList();

            foreach (ProductVariant pv in productVariants)
            {
                if (products.TryGetValue(pv.ProductId, out Product product))
                {
                    pv.SetProduct(product);
                }
                else
                {
                    logger.LogWarning($"Variant {pv.Id} is orphaned. Related product {pv.ProductId} not found");
                }
            }
            
            return products;
        }

        public async Task<Dictionary<string, Product>> GetProductsAsync(IDbConnection existingConnection = null)
        {
            using DisposeIfNullWrapper<IDbConnection> cnnWrapper =
                ConditionalDisposeWrapper.Create(existingConnection, dbConnectionFactory.OpenDbConnection);
            Dictionary<string, Product> products = (await cnnWrapper.Object.GetAllAsync<Product>()).ToDictionary(p => p.Id);
            Dictionary<long, ProductVariant> productVariants = (await cnnWrapper.Object.GetAllAsync<ProductVariant>()).ToDictionary(p => p.Id);
            Dictionary<int, SpecificationAttribute> specifications = (await cnnWrapper.Object.GetAllAsync<SpecificationAttribute>()).ToDictionary(sa => sa.Id);
            foreach (SpecificationAttribute spec in specifications.Values)
            {
                if (productVariants.TryGetValue(spec.ProductVariantId, out ProductVariant variant))
                {
                    spec.SetProductVariant(variant);    
                }
                else
                {
                    logger.LogWarning($"Specification {spec.Id} is orphaned. Related variant ({spec.ProductVariantId}) not found.");
                }
            }
            foreach (ProductVariant variant in productVariants.Values)
            {
                if (products.TryGetValue(variant.ProductId, out Product product))
                {
                    variant.SetProduct(product);
                }
                else
                {
                    logger.LogWarning($"Variant {variant.Id} is orphaned. Related product ({variant.ProductId}) not found.");
                }
            }

            return products;
            
            var finalResults = new Dictionary<string, Product>();
            await cnnWrapper.Object.QueryAsync<Product, ProductVariant, SpecificationAttribute, Product>(
                @"SELECT * FROM ProductVariant pv
                      INNER JOIN SpecificationAttribute sa ON pv.Id = sa.ProductVariantId",
                (dbProduct, dbVariant, spec) =>
                {
                    if (!finalResults.TryGetValue(dbProduct.Id, out Product product))
                    {
                        finalResults.Add(dbProduct.Id, dbProduct);
                        product = dbProduct;
                    }

                    ProductVariant variant = product.AddOrGetVariant(dbVariant);
                    if (spec != null && spec.Id > 0)
                    {
                        spec.SetProductVariant(variant);
                    }
                    
                    return product;
                },
                splitOn: "sa.Id"
            );
            
            return finalResults;
        }

        public async Task SaveProductsAsync(IEnumerable<Product> products)
        {
            using var ts = new TransactionScope();
            using IDbConnection connection = dbConnectionFactory.OpenDbConnection();
            Dictionary<string, Product> currentProducts = await GetProductsAsync(connection);
            Dictionary<long, ProductVariant> existingProductVariants = currentProducts.Values
                    .SelectMany(p => p.Variants)
                    .ToDictionary(p => p.Id);
            Dictionary<(long productVariantId, string name), SpecificationAttribute> existingSpecifications =
                existingProductVariants.Values
                    .SelectMany(pv => pv.Specifications).
                    ToDictionary(s => (s.ProductVariantId, s.PropertyName));
            
            foreach (Product p in products)
            {
                if (p.Id == null) throw new DataIntegrityException($"Trying to save Product {p.Title} with Id=0");
                
                await connection.InsertOrUpdate(p, currentProducts.ContainsKey(p.Id));
                foreach (ProductVariant variant in p.Variants)
                {
                    await connection.InsertOrUpdate(variant, existingProductVariants.ContainsKey(variant.Id));
                    foreach (SpecificationAttribute spec in variant.Specifications)
                    {
                        spec.ProductVariantId = variant.Id;
                        await connection.InsertOrUpdate(
                            spec,
                            existingSpecifications.ContainsKey((spec.ProductVariantId, spec.PropertyName)));
                    }
                }
            }
            
            ts.Complete();
        }
    }
}