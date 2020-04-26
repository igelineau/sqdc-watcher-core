using System;
using System.Collections.Generic;
using System.Text;
using XFactory.SqdcWatcher.Data.Entities;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    internal class SqdcFormatter
    {
        internal static bool FormatProducts(List<Product> products, ProductFormatStyle table)
        {
            throw new NotImplementedException();
        }

        internal static string FormatProductsSummaries(List<Product> products, ProductFormatStyle table)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Product product in products)
            {
                sb.AppendLine($"id= {product.Id}, title= {product.Title}, url= {product.Url}");
            }

            return sb.ToString();
        }
    }
}