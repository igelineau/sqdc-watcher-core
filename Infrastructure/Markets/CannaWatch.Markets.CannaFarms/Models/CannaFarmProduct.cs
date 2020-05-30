using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Data.Entities.Markets;

namespace CannaWatch.Markets.CannaFarms.Models
{
    [PublicAPI]
    public class CannaFarmProduct
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsOrganic { get; set; }
        public string ProductStrainName { get; set; }
        public string ProductTypeName { get; set; }
        public string ProductTypeSubclass { get; set; }

        public List<string> ProductTagNames { get; set; }
        
        public List<CannaFarmProductVariant> Skus { get; set; }

        public ProductDto MapToDto(Market market)
        {
            string url = $"https://shop.cannafarms.ca/#/shop/product/{Id}";
            return new ProductDto(Id, url)
            {
                Title = Name,
                Brand = ProductTagNames?.FirstOrDefault(),
                ProducerName = "CannaFarms",
                Strain = ProductStrainName,
                LevelTwoCategory = ProductTypeName,
                MarketId = market.Id
            };
        }
    }
}