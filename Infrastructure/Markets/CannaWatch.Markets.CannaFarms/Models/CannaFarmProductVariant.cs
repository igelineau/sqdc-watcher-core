using JetBrains.Annotations;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace CannaWatch.Markets.CannaFarms.Models
{
    [PublicAPI]
    public class CannaFarmProductVariant
    {
        public long Id { get; private set; }
        
        public string Name { get; private set; }
        public string ProductId { get; private set; }
        public double UnitGrams { get; private set; }
        public double UnitPrice { get; private set; }
        public bool InStock { get; private set; }

        public ProductVariant MapToEntity(ProductVariant dbVariant)
        {
            ProductVariant variantToReturn = dbVariant ?? new ProductVariant(Id);
            
            variantToReturn.GramEquivalent = UnitGrams;
            variantToReturn.DisplayPrice = UnitPrice / 100;
            variantToReturn.GramEquivalent = UnitGrams;
            variantToReturn.ProductId = ProductId;
            
            return variantToReturn;
        }
    }
}