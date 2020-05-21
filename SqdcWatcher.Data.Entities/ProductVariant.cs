using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace XFactory.SqdcWatcher.Data.Entities
{
    public sealed class ProductVariant
    {
        public ProductVariant(long id)
        {
            Id = id;
            Specifications = new List<SpecificationAttribute>();
        }

        public long Id { get; set; }

        public ProductVariantMetaData MetaData { get; private set; } = new ProductVariantMetaData {WasFetched = false};

        public bool HasMetaData => MetaData != null;

        public Product Product { get; set; }

        [Required] public string ProductId { get; set; }

        public bool InStock { get; set; }

        public double GramEquivalent { get; set; }

        public List<SpecificationAttribute> Specifications { get; set; }

        public DateTime? LastInStockTimestamp { get; set; }

        public double PricePerGram { get; set; }
        public double DisplayPrice { get; set; }
        public double ListPrice { get; set; }

        public string GetDisplayQuantity()
        {
            return Math.Abs(GramEquivalent) > 0.0001 ? $"{GramEquivalent}g" : "";
        }

        public void SetProduct(Product product)
        {
            Product = product;
            product.Variants.Add(this);
        }

        private bool Equals(ProductVariant other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProductVariant) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        internal bool HasSpecifications()
        {
            return Specifications.Any();
        }

        public void SetMetaData(ProductVariantMetaData metaData)
        {
            MetaData = metaData;
        }
    }
}