using System;
using JetBrains.Annotations;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace CannaWatch.Markets.CannaFarms.Models
{
    [PublicAPI]
    public class CannaFarmProductVariant
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        public string ProductId { get; set; }
        public double UnitGrams { get; set; }
        public double UnitPrice { get; set; }
        public bool InStock { get; set; }

        public ProductVariant MapToEntity(ProductVariant dbVariant)
        {
            ProductVariant variantToReturn = dbVariant ?? new ProductVariant(Id);
            
            variantToReturn.GramEquivalent = Math.Round(UnitGrams, 3);
            variantToReturn.DisplayPrice = Math.Round(UnitPrice / 100, 2);
            variantToReturn.GramEquivalent = UnitGrams;
            variantToReturn.ProductId = ProductId;
            variantToReturn.SetStockStatus(InStock);
            
            return variantToReturn;
        }
    }
}