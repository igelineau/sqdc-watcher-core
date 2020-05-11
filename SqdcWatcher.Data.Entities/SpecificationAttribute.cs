using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace XFactory.SqdcWatcher.Data.Entities
{
    [DebuggerDisplay("{PropertyName} = {Value}")]
    public class SpecificationAttribute
    {
        [Required] public int Id { get; set; }

        public ProductVariant ProductVariant { get; set; }

        public long ProductVariantId { get; set; }

        [Required] public string PropertyName { get; set; }

        public string Title { get; set; }

        [Required] public string Value { get; set; }

        public void SetProductVariant(ProductVariant productVariant)
        {
            ProductVariant = productVariant;
            productVariant.Specifications.Add(this);
        }
    }
}