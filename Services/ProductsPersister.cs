using System;
using System.Collections.Generic;
using System.Linq;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Services
{
    public class ProductsPersister
    {
        private readonly DataAccess dataAccess;

        public ProductsPersister(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public void PersistMergeProducts(List<Product> products)
        {
            PersistProducts(products);
        }

        private void PersistProducts(List<Product> freshProducts)
        {
            List<Product> dbProducts = dataAccess.GetProducts();
            var dbProductsMap = dbProducts.ToDictionary(p => p.Id);
            int newProducts = 0;
            int updatedProducts = 0;
            foreach(Product freshProduct in freshProducts)
            {
                Product dbProduct;
                if(!dbProductsMap.TryGetValue(freshProduct.Id, out dbProduct))
                {
                    dbProduct = freshProduct;
                    newProducts++;
                }
                else
                {
                    // we don't need to actually copy.. only detect changes. fix that later when we genericize this code.
                    bool hasChanged = ProductCopier.CopyAllSimpleProperties(new Product(), freshProduct);
                    if(hasChanged)
                    {
                        updatedProducts++;
                    }
                }
            }

            dataAccess.SaveProducts(freshProducts);

            Console.WriteLine(
                $"Online search found products {freshProducts.Count} ({newProducts} new, {updatedProducts} changed).");
        }


    }
}