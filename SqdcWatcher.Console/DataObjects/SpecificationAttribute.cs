using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using SqdcWatcher.DataAccess;
using Dapper.Contrib.Extensions;
using dapperExtensions = Dapper.Contrib.Extensions;
using ssAnnotations = ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    [Table("SpecificationAttribute")]
    [TableObject]
    [DebuggerDisplay("{PropertyName} = {Value}")]
    public class SpecificationAttribute
    {
        [Required]
        [ssAnnotations.PrimaryKeyAttribute]
        [dapperExtensions.Key]
        public int Id { get; set; }
        
        [ssAnnotations.Reference]
        [Computed]
        public ProductVariant ProductVariant { get; set; }
        
        public long ProductVariantId { get; set; }
        
        [Required]
        public string PropertyName { get; set; }
        
        public string Title { get; set; }
        
        [Required]
        public string Value { get; set; }

        public void SetProductVariant(ProductVariant productVariant)
        {
            ProductVariant = productVariant;
            productVariant.Specifications.Add(this);
        }
    }
}