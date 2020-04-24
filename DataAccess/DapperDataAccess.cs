using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;
using Dapper.Contrib.Extensions;
using ServiceStack.Data;
using ServiceStack.OrmLite.Dapper;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.DataAccess
{
    public class DapperDataAccess
    {
        private readonly IDbConnectionFactory dbConnectionFactory;

        public DapperDataAccess(IDbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Dictionary<string, Product>> GetProductsSummaryAsync()
        {
            var finalResults = new Dictionary<string, Product>();

            using var connection = dbConnectionFactory.OpenDbConnection();
            await connection.QueryAsync<Product, ProductVariant, Product>(
                @"SELECT * FROM Product p
                      LEFT JOIN ProductVariant pv ON p.ProductId = pv.ProductId",
                (dbProduct, dbVariant) =>
                {
                    if (!finalResults.TryGetValue(dbProduct.ProductId, out Product product))
                    {
                        finalResults.Add(dbProduct.ProductId, dbProduct);
                        product = dbProduct;
                    }

                    if (dbVariant != null)
                    {
                        product.AddOrGetVariant(dbVariant);
                    }

                    return product;
                },
                splitOn: "ProductVariantId"
            );

            return finalResults;
        }
        
        public async Task<Dictionary<string, Product>> GetProductsAsync()
        {
            var finalResults = new Dictionary<string, Product>();
            using var connection = dbConnectionFactory.OpenDbConnection();
            await connection.QueryAsync<Product, ProductVariant, SpecificationAttribute, Product>(
                @"SELECT * FROM Product p
                     INNER JOIN ProductVariant pv ON p.ProductId = pv.ProductId
                     INNER JOIN SpecificationAttribute sa ON pv.ProductVariantId = sa.ProductVariantId",
                (dbProduct, dbVariant, spec) =>
                {
                    if (!finalResults.TryGetValue(dbProduct.ProductId, out Product product))
                    {
                        finalResults.Add(dbProduct.ProductId, dbProduct);
                        product = dbProduct;
                    }

                    ProductVariant variant = product.AddOrGetVariant(dbVariant);
                    if (spec != null && spec.SpecificationAttributeId > 0)
                    {
                        spec.SetProductVariant(variant);
                    }
                    
                    return product;
                },
                splitOn: "ProductId, ProductVariantId, SpecificationAttributeId"
            );
            
            return finalResults;
        }

        public void SaveProducts(IEnumerable<Product> products)
        {
            using var ts = new TransactionScope();
            using var connection = dbConnectionFactory.OpenDbConnection();

            foreach (Product p in products)
            {
                connection.UpdateAsync(p);
                foreach (ProductVariant variant in p.Variants)
                {
                    connection.UpdateAsync(variant);
                    
                    foreach (var spec in variant.Specifications)
                    {
                        connection.InsertAsync(spec);
                    }
                }
            }
        }
    }
}