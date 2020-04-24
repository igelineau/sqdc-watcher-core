﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ServiceStack.DataAnnotations;
using SqdcWatcher.DataAccess;

namespace SqdcWatcher.DataObjects
{
    [TableObject]
    [DebuggerDisplay("{Title} from {ProducerName} ({LevelTwoCategory})")]
    public class Product
    {
        [Ignore]
        public string Id => ProductId;
    
        [PrimaryKey]
        [Required]
        public string ProductId { get; set; }
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
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant));
            }
            
            ProductVariant existingVariant = Variants.FirstOrDefault(v => v.ProductVariantId == variant.ProductVariantId);
            if (existingVariant == null)
            {
                variant.SetProduct(this);
            }

            return existingVariant ?? variant;
        }

        public ProductVariant GetVariantById(long variantId)
        {
            return Variants.FirstOrDefault(v => v.ProductVariantId == variantId);
        }

        public bool IsInStock()
        {
            return Variants.Any(v => v.InStock);
        }
        
        
        protected bool Equals(Product other)
        {
            return ProductId == other.ProductId;
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
            return ProductId.GetHashCode();
        }
    }
}