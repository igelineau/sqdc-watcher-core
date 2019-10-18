using System.Linq;
using ServiceStack.OrmLite;
using SqdcWatcher.DataObjects;
using SqdcWatcher.RestApiModels.cs;

namespace SqdcWatcher.Services
{
    public static class DtoMapper
    {
        public static Product MergeProductDto(this Product target, ProductDto source)
        {
            target.Id = source.Id;
            target.Title = source.Title;
            target.Url = source.Url;

            foreach (ProductVariantDto pv in source.Variants)
            {
                ProductVariant destVariant = target.GetVariantById(pv.Id);
                if (destVariant == null)
                {
                    destVariant = new ProductVariant();
                    target.Variants.Add(destVariant);
                }

                destVariant.MergeProductVariantDto(pv);
            }

            return target;
        }

        private static ProductVariant MergeProductVariantDto(this ProductVariant destination, ProductVariantDto source)
        {
            destination.Id = source.Id;
            destination.InStock = source.InStock;
            destination.ProductId = source.Product.Id;
            destination.PriceInfo = source.PriceInfo;
            
            return destination;
        }
    }
}