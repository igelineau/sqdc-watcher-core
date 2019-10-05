using System.Collections.Generic;

namespace SqdcWatcher.DataObjects
{
    public class SpecificationsResponse
    {
        public string ProductId { get; set; }
        public string VariantId { get; set; }
        public List<SpecificationGroup> Groups { get; set; }
        public string JsonContext { get; set; }
    }
}