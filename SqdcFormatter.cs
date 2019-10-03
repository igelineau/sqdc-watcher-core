using System;
using System.Collections.Generic;
using System.Text;

namespace SqdcWatcher
{
    internal class SqdcFormatter
    {
        internal static bool FormatProducts(List<SqdcProduct> products, ProductFormatStyle table)
        {
            throw new NotImplementedException();
        }

        internal static string FormatProductsSummaries(List<ProductSummary> products, ProductFormatStyle table)
        {
            StringBuilder sb = new StringBuilder();
            foreach(ProductSummary product in products)
            {
                sb.AppendLine($"id= {product.Id}, title= {product.Title}, url= {product.Url}");
            }

            return sb.ToString();
        }
    }
}