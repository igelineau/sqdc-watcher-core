using System.Collections.Generic;
using ServiceStack.DataAnnotations;
using SqdcWatcher.DataObjects;

namespace SqdcWatcher.RestApiModels
{
    public class SpecificationsResponse : BaseResponse
    {
        public string ProductId { get; set; }
        public string VariantId { get; set; }
        
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<SpecificationGroup> Groups { get; set; }
    }
}