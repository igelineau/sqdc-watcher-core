using System.Text.RegularExpressions;
using JetBrains.Annotations;
using XFactory.SqdcWatcher.Data.Entities.Common;

namespace XFactory.SqdcWatcher.Data.Entities.Products
{
    [PublicAPI]
    public class ProductId : ValueObject<ProductId>
    {
        private ProductId()
        {
        }

        private ProductId(string id)
        {
            Validate(id);
            Id = id;
        }

        public string Id { get; private set; }

        private static void Validate(string id)
        {
            if (id == null)
            {
                throw new InvalidProductIdException("Id cannot be null");
            }

            if (!Regex.IsMatch(id, "^\\d+-P$"))
            {
                throw new InvalidProductIdException(id);
            }
        }

        public static ProductId Create(string id)
        {
            return new ProductId(id);
        }

        protected override bool EqualsCore(ProductId other)
        {
            return other.Id == Id;
        }

        protected override int GetHashCodeCore()
        {
            return Id.GetHashCode();
        }

        public static implicit operator string(ProductId productId)
        {
            return productId.Id;
        }

        public static implicit operator ProductId(string id)
        {
            return new ProductId
            {
                Id = id
            };
        }
    }
}