using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using JetBrains.Annotations;
using XFactory.SqdcWatcher.Data.Entities.History;

namespace XFactory.SqdcWatcher.Data.Entities.ProductVariant
{
    [PublicAPI]
    public sealed class ProductVariant
    {
        private ProductVariant()
        {
            Specifications = new List<SpecificationAttribute>();
        }

        public ProductVariant(long id) : this()
        {
            Id = id;
        }

        public long Id { get; private set; }

        public ProductVariantMetaData MetaData { get; private set; } = new ProductVariantMetaData {WasFetched = false};

        public bool HasMetaData => MetaData != null;

        [Required] public string ProductId { get; set; }

        public bool InStock { get; private set; }

        public double GramEquivalent { get; set; }

        public List<SpecificationAttribute> Specifications { get; private set; }

        public DateTime? LastInStockTimestamp { get; private set; }

        public double PricePerGram { get; set; }
        public double DisplayPrice { get; set; }
        public double ListPrice { get; set; }

        public string GetDisplayQuantity()
        {
            return Math.Abs(GramEquivalent) > 0.0001 ? $"{GramEquivalent}g" : "";
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

        public StockStatusChangeResult SetStockStatus(bool isInStock)
        {
            bool hasChangedToInStock = !InStock && isInStock;
            bool hasChangedToNotInStock = InStock && !isInStock;
            if (hasChangedToInStock) LastInStockTimestamp = DateTime.Now;

            InStock = isInStock;

            if (hasChangedToInStock) return StockStatusChangeResult.BecameInStock;

            if (hasChangedToNotInStock) return StockStatusChangeResult.BecameOutOfStock;

            return StockStatusChangeResult.NotChanged;
        }
    }
}