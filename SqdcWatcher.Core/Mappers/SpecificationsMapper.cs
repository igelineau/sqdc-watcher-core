using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using XFactory.SqdcWatcher.Core.MappingFilters;
using XFactory.SqdcWatcher.Core.RestApiModels;
using XFactory.SqdcWatcher.Core.Services;
using XFactory.SqdcWatcher.Data.Entities;

namespace XFactory.SqdcWatcher.Core.Mappers 
{
    [UsedImplicitly]
    public class SpecificationsMapper : MapperBase<SpecificationsResponse, List<SpecificationAttribute>>
    {
        public SpecificationsMapper(IEnumerable<IMappingFilter<SpecificationsResponse, List<SpecificationAttribute>>> mappingFilters) : base(mappingFilters)
        {
        }

        protected override List<SpecificationAttribute> PerformMapping(SpecificationsResponse source, List<SpecificationAttribute> dest)
        {
            List<SpecificationAttributeDto> attributes = source.Groups.SelectMany(g => g.Attributes).ToList();
            dest.MergeListById(
                attributes,
                x => x.PropertyName,
                x => x.PropertyName,
                spec => spec.ProductVariantId = long.Parse(source.VariantId));
            return dest;
        }
    }
}