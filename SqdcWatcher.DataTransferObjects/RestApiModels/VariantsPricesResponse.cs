using System.Collections.Generic;

namespace SqdcWatcher.DataTransferObjects.RestApiModels
{
    public class VariantsPricesResponse : BaseResponse
    {
        public List<ProductPrice> ProductPrices { get; set; }
        public Currency Currency { get; set; }
    }
}