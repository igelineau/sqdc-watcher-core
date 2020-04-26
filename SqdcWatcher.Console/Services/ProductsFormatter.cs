#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alba.CsConsoleFormat;
using Models.EntityFramework;

#endregion

namespace SqdcWatcher.Services
{
    public class ProductsFormatter
    {
        private const int TITLE_MAX_WIDTH = 20;
        private const int STRAIN_MAX_WIDTH = 25;
        private const int SINGLE_BRANDING_MAX_WIDTH = 25;
        private const int DUAL_BRANDING_MAX_COMP_WIDTH = 12;

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
                                new Cell(FormatConsoleProductPrefix(p) + p.Title),
                                new Cell(FormatBrandAndSupplier(p)),
                                new Cell(FormatVariantsAvailable(p)),
                                new Cell(p.Url),
                            })
                        }
                    }));
        }

        private static string FormatConsoleProductPrefix(Product p)
        {
            if (p.IsNew)
            {
                return p.IsInStock() ? "[NEW] " : "[UPCOMING] ";
            }

            return "";
        }

        private static string FormatSlackProductPrefix(Product p)
        {
            if (p.IsNew)
            {
                return p.IsInStock() ? ":weed: NEW :weed: " : ":star: UPCOMING :star: ";
            }

            return "";
        }

        public static string FormatForSlackTable(IEnumerable<Product> products)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Product product in products)
            {
                string variantsAvailable = FormatVariantsAvailable(product);
                builder.AppendLine($"*{FormatSlackProductPrefix(product)}{FormatNameWithType(product)}* / {product.Brand} {variantsAvailable}{product.Url}");
            }

            return builder.ToString();
        }

        private static string FormatNameWithType(Product product)
        {
            string name = FormatName(product);
            if (!string.IsNullOrWhiteSpace(product.CannabisType))
            {
                return $"{name}, {product.CannabisType}";
            }

            return name;
        }

        private static string FormatName(Product product)
        {
            string name = product.Title;
            string strain = LimitLength(product.Strain, STRAIN_MAX_WIDTH);
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

            string displayString = components[0];
            if (components.Count > 1)
            {
                displayString += $" ({LimitLength(components[1], 12)})";
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