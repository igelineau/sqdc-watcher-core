using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XFactory.SqdcWatcher.Data.Entities.Products
{
    [DebuggerDisplay("{Title} from {ProducerName} ({LevelTwoCategory})")]
    public sealed class Product
    {
        private readonly List<ProductVariant> variants;
        public ProductId Id { get; }

        public string Title { get; set; }

        public string Url { get; set; }

        public IEnumerable<ProductVariant> Variants => variants;

        public string ProducerName { get; set; }

        public string LevelTwoCategory { get; set; }

        public string CannabisType { get; set; }

        public string Strain { get; set; }

        public string Quality { get; set; }

        public string TerpeneDetailed { get; set; }
        public string Brand { get; set; }
        public bool IsNew { get; set; }

        private Product()
        {
            variants = new List<ProductVariant>();
        }

        public Product(string id) : this()
        {
            Id = ProductId.Create(id);
            IsNew = true;
        }

        public IEnumerable<ProductVariant> GetAvailableVariants() => Variants.Where(v => v.InStock);

        public bool IsInStock() => Variants.Any(v => v.InStock);

        public void AddVariant(ProductVariant productVariant)
        {
            variants.Add(productVariant);
        }

        private bool Equals(Product other)
        {
            return Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj?.GetType() != GetType()) return false;
            return Equals((Product) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}