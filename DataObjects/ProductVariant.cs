using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ServiceStack.DataAnnotations;
using SqdcWatcher.DataAccess;
using SqdcWatcher.Dto;

namespace SqdcWatcher.DataObjects
{
    [TableObject]
    public class ProductVariant
    {
        [Ignore]
        public long Id => ProductVariantId;
        
        [Ignore]
        public ProductVariantMetaData MetaData { get; private set; } = new ProductVariantMetaData { WasFetched = false };

        [Ignore]
        public bool HasMetaData => MetaData != null;

        [PrimaryKey]
        public long ProductVariantId { get; set; }

        [Reference]
        public Product Product { get; set; }

        [Index]
        public string ProductId { get; set; }

        public bool InStock { get; set; }

        public double GramEquivalent { get; set; }

        [Reference]
        public List<SpecificationAttribute> Specifications { get; set; }
        
        public DateTime? LastInStockTimestamp { get; set; }

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
            return ProductVariantId == other.ProductVariantId;
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
            return ProductVariantId.GetHashCode();
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