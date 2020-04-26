using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;

namespace XFactory.SqdcWatcher.Data.Entities
{
    [DebuggerDisplay("{Title} from {ProducerName} ({LevelTwoCategory})")]
    public class Product
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public List<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        public string ProducerName { get; set; }

        public string LevelTwoCategory { get; set; }

        public string CannabisType { get; set; }

        public string Strain { get; set; }

        public string Quality { get; set; }

        public string TerpeneDetailed { get; set; }
        public string Brand { get; set; }

        [NotMapped] public bool IsNew { get; set; }

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
            if (obj.GetType() != GetType()) return false;
            return Equals((Product) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}