using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Services
{
    public class DataAccess
    {
        private const string DEFAULT_APPSTATE_KEY = "default";
        const bool DROP_TABLES = false;
        
        private IDbConnection db;

        public DataAccess(IDbConnectionFactory connectionFactory)
        {
            db = connectionFactory.OpenDbConnection();

            var tablesTypes = new[]
            {
                typeof(Product),
                typeof(ProductVariant),
                typeof(SpecificationAttribute)
            };

            if (DROP_TABLES)
            {
                db.DropTables(tablesTypes);
            }
            db.CreateTables(overwrite: DROP_TABLES, tablesTypes);
        }

        public Dictionary<string, Product> GetProducts()
        {
            SqlExpression<Product> q = db
                .From<Product>()
                .LeftJoin<Product, ProductVariant>()
                .LeftJoin<ProductVariant, SpecificationAttribute>();
            
            List<Tuple<Product, ProductVariant, SpecificationAttribute>> results =
                db.SelectMulti<Product, ProductVariant, SpecificationAttribute>(q);
            
            var finalResults = new Dictionary<string, Product>();
            
            foreach ((Product p, ProductVariant pv, SpecificationAttribute spec) in results)
            {
                if (!finalResults.TryGetValue(p.Id, out Product product))
                {
                    finalResults.Add(p.Id, p);
                    product = p;
                }
                ProductVariant variant = product.AddOrGetVariant(pv);
                if(spec.Id > 0)
                {
                    spec.SetProductVariant(variant);
                }
                
                finalResults.TryAdd(product.Id, product);
            }

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

        public DateTime GetLastProductsListUpdateTimestamp()
        {
            var q = db.From<AppState>().Limit(1).Select<AppState>(s => s.LastProductsListRefresh);
            return db.Select<DateTime>(q).FirstOrDefault();
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

        public List<Product> GetProductsSummary()
        {
            return db.Select<Product>().ToList();
        }
    }
}