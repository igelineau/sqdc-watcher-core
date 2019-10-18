using System.Collections.Generic;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.RestApiModels.cs
{
    public class VariantsPricesResponse : BaseResponse
    {
        public List<ProductPrice> ProductPrices { get; set; }
        public Currency Currency { get; set; }
    }
}