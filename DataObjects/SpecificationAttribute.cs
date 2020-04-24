using System.Diagnostics;
using ServiceStack.DataAnnotations;
using SqdcWatcher.DataAccess;

namespace SqdcWatcher.DataObjects
{
    [TableObject]
    [DebuggerDisplay("{PropertyName} = {Value}")]
    public class SpecificationAttribute
    {
        [Ignore]
        public int Id => SpecificationAttributeId;
        
        [PrimaryKey]
        [AutoIncrement]
        public int SpecificationAttributeId { get; set; }
        
        [Reference]
        public ProductVariant ProductVariant { get; set; }
        
        [Index]
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