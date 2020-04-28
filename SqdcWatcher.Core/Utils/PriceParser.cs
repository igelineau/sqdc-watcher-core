#region

using XFactory.SqdcWatcher.Core.Dto;
using XFactory.SqdcWatcher.Core.RestApiModels;

#endregion

namespace XFactory.SqdcWatcher.Core.Utils
{
    public class PriceParser
    {
        public static ParsedPriceInfo ParseVariantPrice(ProductVariantPrice rawPrices)
        {
            return new ParsedPriceInfo
            {
                DisplayPrice = rawPrices.DisplayPrice == null ? new double?() : ParsePrice(rawPrices.DisplayPrice),
                ListPrice = rawPrices.ListPrice == null ? new double?() : ParsePrice(rawPrices.ListPrice),
                PricePerGram = rawPrices.PricePerGram == null ? new double?() : ParsePrice(rawPrices.PricePerGram)
            };
        }

        private static double ParsePrice(string price)
        {
            return double.Parse(price.Trim('$', ' '));
        }
    }
}