#region

using System.Collections.Generic;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.RestApiModels
{
    public class SpecificationsResponse : BaseResponse
    {
        public string ProductId { get; set; }
        public string VariantId { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Global
        public List<SpecificationGroup> Groups { get; set; }
    }
}