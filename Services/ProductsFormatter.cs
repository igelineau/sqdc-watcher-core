using System;
using System.Collections.Generic;
using System.Linq;
using Alba.CsConsoleFormat;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.Services
{
    public class ProductsFormatter
    {
        public static void WriteProductsTableToConsole(IEnumerable<Product> products)
        {
            var headerThickness = new LineThickness(LineWidth.Double, LineWidth.Single);

            ConsoleRenderer.RenderDocument(
                new Document(
                    new Grid
                    {
                        Background = ConsoleColor.DarkGray,
                        Color = ConsoleColor.Gray,
                        Columns = { GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto },
                        Children =
                        {
                            new Cell("Title") { Stroke = headerThickness },
                            new Cell("Brand") { Stroke = headerThickness },
                            new Cell("Quantities") { Stroke = headerThickness },
                            new Cell("Url") { Stroke = headerThickness },
                            products.Select(p => new[]
                            {
                                new Cell(p.Title),
                                new Cell(FormatBrandAndSupplier(p)),
                                CreateVariantsAvailableCell(p),
                                new Cell(p.Url),
                            })
                        }
                    }));
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

        private static Cell CreateVariantsAvailableCell(Product p)
        {
            return new Cell(string.Join(", ", p.GetAvailableVariants().Select(v => v.GetDisplayQuantity())));
        }
    }
}