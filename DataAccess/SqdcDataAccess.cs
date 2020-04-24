using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.DataAccess
{
    public class SqdcDataAccess
    {
        private readonly ILogger<SqdcDataAccess> logger;
        private const string DEFAULT_APPSTATE_KEY = "default";
        const bool DROP_TABLES = false;
        
        private IDbConnection db;

        public SqdcDataAccess(IDbConnectionFactory connectionFactory, ILogger<SqdcDataAccess> logger)
        {
            this.logger = logger;

            db = connectionFactory.OpenDbConnection();

            string dataObjectsNamespace = typeof(Product).Namespace ?? throw new InvalidOperationException("Product has no namespace.");
            var tablesTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.CustomAttributes.Any(ca => ca.AttributeType == typeof(TableObjectAttribute)))
                .ToArray();
            db.CreateTables(overwrite: DROP_TABLES, tablesTypes);
        }
        
        [Obsolete]
        public List<Product> GetProductsSummary()
        {
            SqlExpression<Product> q = db
                .From<Product>()
                .LeftJoin<ProductVariant>();
            List<Tuple<Product, ProductVariant>> result = db.SelectMulti<Product, ProductVariant>(q);
            var finalResults = new Dictionary<string, Product>();
            foreach ((Product p, ProductVariant pv) in result)
            {
                if (!finalResults.TryGetValue(p.ProductId, out Product product))
                {
                    finalResults.Add(p.ProductId, p);
                    product = p;
                }

                if(pv.ProductVariantId > 0)
                {
                    product.AddOrGetVariant(pv);
                }                
            }

            return finalResults.Values.ToList();
        }

        public Dictionary<string, Product> GetProducts()
        {
            Stopwatch sw = Stopwatch.StartNew();
            SqlExpression<Product> q = db
                .From<Product>()
                .LeftJoin<Product, ProductVariant>()
                .LeftJoin<ProductVariant, SpecificationAttribute>();

            List<Tuple<Product, ProductVariant, SpecificationAttribute>> results =
                db.SelectMulti<Product, ProductVariant, SpecificationAttribute>(q);

            var finalResults = new Dictionary<string, Product>();
            
            foreach ((Product p, ProductVariant pv, SpecificationAttribute spec) in results)
            {
                if (!finalResults.TryGetValue(p.ProductId, out Product product))
                {
                    finalResults.Add(p.ProductId, p);
                    product = p;
                }
                ProductVariant variant = product.AddOrGetVariant(pv);
                if(spec.SpecificationAttributeId > 0)
                {
                    spec.SetProductVariant(variant);
                }
            }

            logger.Log(LogLevel.Information, $"Retrieved {finalResults.Count} products from the DB store in {sw.ElapsedMilliseconds}ms");
            return finalResults;
        }

        public void AddProduct(Product product)
        {
            db.Insert(product);
        }

        internal void SaveProducts(List<Product> products)
        {
            db.SaveAll(products);
            
            foreach (ProductVariant variant in products.SelectMany(p => p.Variants))
            {
                db.Save(variant, references: true);
            }
        }

        public DateTime? GetLastProductsListUpdateTimestamp()
        {
            var q = db.From<AppState>().Limit(1).Select<AppState>(s => s.LastProductsListRefresh);
            var result = db.Select(q).FirstOrDefault();
            return result?.LastProductsListRefresh;
        }

        public void SetLastProductsListUpdateTimestamp(DateTime value)
        {
            var dataToUpdate = new {LastProductsListRefresh = value};
            int affected = db.Update<AppState>(dataToUpdate, appState => appState.Key == DEFAULT_APPSTATE_KEY);
            if (affected == 0)
            {
                db.Insert(new AppState {Key = DEFAULT_APPSTATE_KEY, LastProductsListRefresh = value});
            }
        }

        public void InsertHistoryEntry(StockHistory stockHistory)
        {
            db.Insert(stockHistory);
        }

        public void InsertPriceHistoryEntry(PriceHistory priceHistory)
        {
            db.Insert(priceHistory);
        }
    }
}