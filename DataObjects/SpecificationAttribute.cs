using System.Diagnostics;
using ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    [DebuggerDisplay("{PropertyName} = {Value}")]
    public class SpecificationAttribute
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        [Reference]
        public ProductVariant ProductVariant { get; set; }
        
        public long ProductVariantId { get; set; }
        public string PropertyName { get; set; }
        
        public string Title { get; set; }
        public string Value { get; set; }

        public void SetProductVariant(ProductVariant productVariant)
        {
            ProductVariant = productVariant;
            productVariant.Specifications.Add(this);
        }
    }
}