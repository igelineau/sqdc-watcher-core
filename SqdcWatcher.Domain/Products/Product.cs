using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace XFactory.SqdcWatcher.Data.Entities.Products
{
    [PublicAPI]
    [DebuggerDisplay("{Title} from {ProducerName} ({LevelTwoCategory})")]
    public sealed class Product
    {
        private readonly List<ProductVariant.ProductVariant> variants;

        private Product()
        {
            variants = new List<ProductVariant.ProductVariant>();
        }

        public Product(string id, string marketId) : this()
        {
            Id = id;
            SetMarketId(marketId);
            IsNew = true;
        }

        public string Id { get; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string MarketId { get; private set; }

        public Market Market { get; private set; }

        public IEnumerable<ProductVariant.ProductVariant> Variants => variants;

        public string ProducerName { get; set; }

        public string LevelTwoCategory { get; set; }

        public string CannabisType { get; set; }

        public string Strain { get; set; }

        public string Quality { get; set; }

        public string TerpeneDetailed { get; set; }
        public string Brand { get; set; }
        public bool IsNew { get; set; }

        public IEnumerable<ProductVariant.ProductVariant> GetAvailableVariants() => Variants.Where(v => v.InStock);

        public bool IsInStock() => Variants.Any(v => v.InStock);

        public bool AddVariant(ProductVariant.ProductVariant variant)
        {
            bool wasAdded = false;
            if (!Variants.Contains(variant))
            {
                variants.Add(variant);
                wasAdded = true;
            }

            return wasAdded;
        }

        private bool Equals(Product other)
        {
            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj?.GetType() != GetType())
            {
                return false;
            }

            return Equals((Product) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void SetMarketId(in string marketId)
        {
            Guard.Against.NullOrWhiteSpace(marketId, nameof(marketId));

            MarketId = marketId;
        }
    }
}