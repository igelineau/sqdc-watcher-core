using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Services
{
    public class DataAccess
    {
        private IDbConnection db;

        public DataAccess(IDbConnectionFactory connectionFactory)
        {
            db = connectionFactory.OpenDbConnection();
            db.CreateTableIfNotExists<Product>();
            db.CreateTableIfNotExists<ProductVariant>();
        }

        public List<Product> GetProducts()
        {
            return db.Select<Product>();
        }

        public void AddProduct(Product product)
        {
            db.Insert(product);
        }

        internal void SaveProducts(IEnumerable<Product> products)
        {
            db.SaveAll(products);
        }
    }
}