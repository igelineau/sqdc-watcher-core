#region

using System.Diagnostics;
using SqdcWatcher.RestApiModels;

#endregion

namespace SqdcWatcher.Services
{
    [DebuggerDisplay("{PropertyName} = {Value}")]
    public class SpecificationAttributeDto
    {
        public ProductVariantDto ProductVariant { get; set; }
        public string Title { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }
}