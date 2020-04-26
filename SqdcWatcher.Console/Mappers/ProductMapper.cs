#region

using System.Collections.Generic;
using XFactory.SqdcWatcher.ConsoleApp.MappingFilters;
using XFactory.SqdcWatcher.ConsoleApp.RestApiModels;
using XFactory.SqdcWatcher.Data.Entities;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Mappers
{
    public class ProductMapper : MapperBase<ProductDto, Product>
    {
        public ProductMapper(IEnumerable<IMappingFilter<ProductDto, Product>> mappingFilters) : base(mappingFilters)
        {
        }

        protected override Product PerformMapping(ProductDto source, Product destination)
        {
            if (destination == null)
            {
                destination = new Product();
            }

            destination.Id = source.Id;
            destination.Title = source.Title;
            destination.Url = source.Url;
            destination.Brand = source.Brand;
            return destination;
        }

        protected override Product CreateDestinationInstance() => new Product {IsNew = true};
    }
}