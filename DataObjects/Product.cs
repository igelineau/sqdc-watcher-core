using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    [DebuggerDisplay("{Title} from {ProducerName} ({LevelTwoCategory})")]
    public class Product
    {
        [Required]
        public string Id { get; set; }
        public string Title { get; set; }
        [Required]
        public string Url { get; set; }

        [Reference]
        public List<ProductVariant> Variants { get; set; }

        public string ProducerName { get; set; }

        public string LevelTwoCategory { get; set; }
        
        public string CannabisType { get; set; }

        public string Strain { get; set; }

        public string Quality { get; set; }
        
        public string TerpeneDetailed { get; set; }
        public string Brand { get; set; }
        
        [Ignore]
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
    }
}