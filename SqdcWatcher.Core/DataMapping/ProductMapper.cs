using System.Collections.Generic;
using JetBrains.Annotations;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Data.Entities.Products;

namespace XFactory.SqdcWatcher.Core.DataMapping
{
    [UsedImplicitly]
    public class ProductMapper : MapperBase<ProductDto, Product>
    {
        public ProductMapper(IEnumerable<IMappingFilter<ProductDto, Product>> mappingFilters) : base(mappingFilters)
        {
        }

        protected override Product PerformMapping(ProductDto source, Product destination)
        {
            destination.Title = source.Title;
            destination.Url = source.Url;
            destination.Brand = source.Brand;
            return destination;
        }

        protected override Product CreateDestinationInstance(ProductDto source) => new Product(source.Id);
    }
}