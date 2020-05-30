using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero.");
            }
            Id = id;
        }

        public long Id { get; private set; }

        /// <summary>
        /// Indicates if this entity was created from code (IsNew == true) or not. IsNew == false means it was created using the default, private constructor.
        /// </summary>
        public bool IsNew { get; }

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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ProductVariant) obj);
        }

        public StockStatusChangeResult SetStockStatus(bool isInStock)
        {
            bool hasChangedToInStock = !InStock && isInStock;
            bool hasChangedToNotInStock = InStock && !isInStock;
            if (hasChangedToInStock)
            {
                LastInStockTimestamp = DateTime.Now;
            }

            InStock = isInStock;

            if (hasChangedToInStock)
            {
                return StockStatusChangeResult.BecameInStock;
            }

            if (hasChangedToNotInStock)
            {
                return StockStatusChangeResult.BecameOutOfStock;
            }

            return StockStatusChangeResult.NotChanged;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}