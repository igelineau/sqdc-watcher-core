using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SqdcWatcher.DataTransferObjects.RestApiModels;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Core.Services;
using XFactory.SqdcWatcher.Data.Entities.ProductVariant;

namespace XFactory.SqdcWatcher.Core.Mappers
{
    [UsedImplicitly]
    public class SpecificationsMapper : MapperBase<SpecificationsResponse, List<SpecificationAttribute>>
    {
        public SpecificationsMapper(IEnumerable<IMappingFilter<SpecificationsResponse, List<SpecificationAttribute>>> mappingFilters) : base(mappingFilters)
        {
        }

        protected override List<SpecificationAttribute> PerformMapping(SpecificationsResponse source, List<SpecificationAttribute> destination)
        {
            List<SpecificationAttributeDto> attributes = source.Groups.SelectMany(g => g.Attributes).ToList();
            destination.MergeListById(
                attributes,
                x => x.PropertyName,
                x => x.PropertyName,
                spec => spec.ProductVariantId = long.Parse(source.VariantId));
            return destination;
        }

        protected override List<SpecificationAttribute> CreateDestinationInstance(SpecificationsResponse source)
        {
            return new List<SpecificationAttribute>();
        }
    }
}