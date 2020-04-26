#region

using System.Collections.Generic;
using SqdcWatcher.DataObjects;

#endregion

namespace SqdcWatcher.RestApiModels
{
    public class VariantsPricesResponse : BaseResponse
    {
        public List<ProductPrice> ProductPrices { get; set; }
        public Currency Currency { get; set; }
    }
}