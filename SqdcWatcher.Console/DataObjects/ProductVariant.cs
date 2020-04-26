using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Dapper.Contrib.Extensions;
using Models.EntityFramework;
using SqdcWatcher.DataAccess;
using dapperExtensions = Dapper.Contrib.Extensions;
using ssAnnotations = ServiceStack.DataAnnotations;

namespace SqdcWatcher.DataObjects
{
    [Table("ProductVariant")]
    [TableObject]
    public class ProductVariant
    {
        [Required]
        [ssAnnotations.PrimaryKeyAttribute]
        [dapperExtensions.Key]
        public long Id { get; set; }

        [ssAnnotations.ReferenceAttribute]
        [Computed]
        public ProductVariantMetaData MetaData { get; private set; } = new ProductVariantMetaData { WasFetched = false };

        [ssAnnotations.IgnoreAttribute]
        [Computed]
        public bool HasMetaData => MetaData != null;

        [ssAnnotations.Reference]
        [Computed]
        public Product Product { get; set; }
        
        [Required]
        public string ProductId { get; set; }
        
        public bool InStock { get; set; }

        public double GramEquivalent { get; set; }

        [ssAnnotations.ReferenceAttribute]
        [Computed]
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