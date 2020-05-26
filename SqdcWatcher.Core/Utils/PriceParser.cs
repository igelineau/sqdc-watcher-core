using SqdcWatcher.DataTransferObjects.DomainDto;
using SqdcWatcher.DataTransferObjects.RestApiModels;

namespace XFactory.SqdcWatcher.Core.Utils
{
    public static class PriceParser
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