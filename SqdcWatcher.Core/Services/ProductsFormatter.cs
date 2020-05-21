

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alba.CsConsoleFormat;
using XFactory.SqdcWatcher.Data.Entities;



namespace XFactory.SqdcWatcher.Core.Services
{
    public static class ProductsFormatter
    {
        private const int StrainMaxWidth = 25;

        public static void WriteProductsTableToConsole(IEnumerable<Product> products)
        {
            var headerThickness = new LineThickness(LineWidth.Double, LineWidth.Single);

            ConsoleRenderer.RenderDocument(
                new Document(
                    new Grid
                    {
                        Background = ConsoleColor.DarkGray,
                        Color = ConsoleColor.Gray,
                        Columns = {GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto},
                        Children =
                        {
                            new Cell("Title") {Stroke = headerThickness},
                            new Cell("Brand") {Stroke = headerThickness},
                            new Cell("Quantities") {Stroke = headerThickness},
                            new Cell("Url") {Stroke = headerThickness},
                            products.Select(p => new[]
                            {
                                new Cell(p.Title),
                                new Cell(FormatBrandAndSupplier(p)),
                                new Cell(FormatVariantsAvailable(p)),
                                new Cell(p.Url),
                            })
                        }
                    }));
        }

        private static string FormatNewProductPrefix(Product p)
        {
            string result = "";
            if (p.IsNew)
            {
                result = p.IsInStock() ? "[NEW] " : "[UPCOMING] ";
            }
            
            return result;
        }

        public static string FormatForSlackTable(IEnumerable<Product> products)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Product product in products)
            {
                string variantsAvailable = FormatVariantsAvailable(product);
                string prefix = FormatNewProductPrefix(product);
                string urlDisplayText = $"*{product.Brand} {product.Title} ({product.LevelTwoCategory})*";
                builder.AppendLine($"{prefix} <{product.Url}|{urlDisplayText}> {variantsAvailable}".Trim());
            }

            return builder.ToString();
        }

        private static string FormatName(Product product)
        {
            string name = product.Title;
            string strain = LimitLength(product.Strain, StrainMaxWidth);
            string finalName;
            if (string.IsNullOrEmpty(strain) || strain.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                finalName = name;
            }
            else
            {
                finalName = strain.IndexOf(',') > -1 ? $"{name} ({strain})" : $"{strain} ({name})";
            }

            return finalName;
        }

        private static string FormatBrandAndSupplier(Product product)
        {
            var components = new List<string>();
            string producerName = product.ProducerName;
            string brand = product.Brand;

            if (ShouldDisplayBrand(brand))
            {
                components.Add(brand);
            }

            if (ShouldDisplaySupplier(producerName) && !producerName.Equals(brand, StringComparison.OrdinalIgnoreCase))
            {
                components.Add(producerName);
            }

            if (components.Count == 0)
            {
                components.Add(brand);
            }

            string displayString = components.First();
            if (components.Count > 1)
            {
                displayString += $" ({LimitLength(components.ElementAt(1), 12)})";
            }
            else
            {
                displayString = LimitLength(displayString, 25);
            }

            return displayString;
        }

        private static bool ShouldDisplayBrand(string brand) => brand != "Plain Packaging";

        private static bool ShouldDisplaySupplier(string supplier) => supplier != "Aurora Cannabis";

        private static string LimitLength(string str, int maxLength) => str.Length <= maxLength ? str : str.Substring(0, maxLength);

        private static string FormatVariantsAvailable(Product p)
        {
            string formattedQuantities = string.Join(", ", p.GetAvailableVariants().Select(v => v.GetDisplayQuantity() + $" {v.DisplayPrice}$"));
            return formattedQuantities == "" ? "" : $"({formattedQuantities}) ";
        }
    }
}