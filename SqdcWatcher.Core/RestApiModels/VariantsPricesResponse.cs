#region

using System.Collections.Generic;

#endregion

namespace XFactory.SqdcWatcher.Core.RestApiModels
{
    public class VariantsPricesResponse : BaseResponse
    {
        public List<ProductPrice> ProductPrices { get; set; }
        public Currency Currency { get; set; }
    }
}