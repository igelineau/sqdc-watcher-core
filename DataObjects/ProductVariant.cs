using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    public class ProductVariant
    {
        [Ignore]
        public ProductVariantMetaData MetaData { get; private set; } = new ProductVariantMetaData { WasFetched = false };

        [Ignore]
        public bool HasMetaData => MetaData != null;

        public long Id { get; set; }

        [Reference]
        public Product Product { get; set; }

        public string ProductId { get; set; }

        public bool InStock { get; set; }

        public double GramEquivalent { get; set; }

        [Reference]
        public List<SpecificationAttribute> Specifications { get; set; }
        
        public DateTime LastInStockTimestamp { get; set; }

        public double PricePerGram { get; set; }
        public double DisplayPrice { get; set; }
        public double ListPrice { get; set; }
        
        public ProductVariant()
        {
            Specifications = new List<SpecificationAttribute>();
        }

        public string GetDisplayQuantity()
        {
            return Math.Abs(GramEquivalent) > 0.0001 ? $"{GramEquivalent}g" : "";
        }

        private string GetSpecification(string propertyName)
        {
            SpecificationAttribute attribute = Specifications.FirstOrDefault(s => s.PropertyName == propertyName);
            return attribute?.Value ?? "";
        }

        public void SetProduct(Product product)
        {
            Product = product;
            product.Variants.Add(this);
        }

        protected bool Equals(ProductVariant other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
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