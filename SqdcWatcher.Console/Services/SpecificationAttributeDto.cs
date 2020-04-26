#region

using System.Diagnostics;
using XFactory.SqdcWatcher.ConsoleApp.RestApiModels;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Services
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