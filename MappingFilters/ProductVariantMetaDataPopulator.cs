using SqdcWatcher.DataObjects;
using SqdcWatcher.Dto;
using SqdcWatcher.RestApiModels;

namespace SqdcWatcher.MappingFilters
{
    public class ProductVariantMetaDataPopulator : MappingFilterBase<ProductVariantDto, ProductVariant>
    {
        public override void Apply(ProductVariantDto source, ProductVariant destination)
        {
            var metaData = new ProductVariantMetaData
            {
                HasBecomeInStock = !destination.InStock && source.InStock,
                HasBecomeOutOfStock = destination.InStock && !source.InStock
            };
            destination.SetMetaData(metaData);
        }
    }
}