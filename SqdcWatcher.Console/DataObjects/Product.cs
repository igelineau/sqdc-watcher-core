using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using Dapper.Contrib.Extensions;
using SqdcWatcher.DataAccess;
using dapperContrib = Dapper.Contrib.Extensions;
using ssAnnotations = ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    [Table("Product")]
    [TableObject]
    [DebuggerDisplay("{Title} from {ProducerName} ({LevelTwoCategory})")]
    public class Product
    {
        [Required]
        [ssAnnotations.PrimaryKeyAttribute]
        [ExplicitKey]
        public string Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Url { get; set; }
        
        [ssAnnotations.ReferenceAttribute]
        [Computed]
        public List<ProductVariant> Variants { get; set; }

        public string ProducerName { get; set; }

        public string LevelTwoCategory { get; set; }
        
        public string CannabisType { get; set; }

        public string Strain { get; set; }

        public string Quality { get; set; }
        
        public string TerpeneDetailed { get; set; }
        public string Brand { get; set; }
        
        [ssAnnotations.IgnoreAttribute]
        [Computed]
        public bool IsNew { get; set; }

        public Product()
        {
            Variants = new List<ProductVariant>();
        }

        public List<ProductVariant> GetAvailableVariants()
        {
            return Variants.Where(v => v.InStock).ToList();
        }

        public ProductVariant AddOrGetVariant(ProductVariant variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant));
            }
            
            ProductVariant existingVariant = Variants.FirstOrDefault(v => v.Id == variant.Id);
            if (existingVariant == null)
            {
                variant.SetProduct(this);
            }

            return existingVariant ?? variant;
        }

        public ProductVariant GetVariantById(long variantId)
        {
            return Variants.FirstOrDefault(v => v.Id == variantId);
        }

        public bool IsInStock()
        {
            return Variants.Any(v => v.InStock);
        }
        
        
        protected bool Equals(Product other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Product) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}