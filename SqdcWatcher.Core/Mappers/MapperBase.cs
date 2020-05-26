using System.Collections.Generic;
using System.Linq;
using XFactory.SqdcWatcher.Core.MappingFilters;

namespace XFactory.SqdcWatcher.Core.Mappers
{
    public abstract class MapperBase<TSource, TDest> : IMapper<TSource, TDest>
    {
        private readonly IReadOnlyCollection<IMappingFilter<TSource, TDest>> mappingFilters;

        protected MapperBase(IEnumerable<IMappingFilter<TSource, TDest>> mappingFilters)
        {
            this.mappingFilters = mappingFilters.ToList().AsReadOnly();
        }

        public TDest Map(TSource source, TDest existingDest)
        {
            existingDest ??= CreateDestinationInstance(source);

            foreach (IMappingFilter<TSource, TDest> filter in mappingFilters)
            {
                filter.Apply(source, existingDest);
            }

            return PerformMapping(source, existingDest);
        }

        protected abstract TDest PerformMapping(TSource source, TDest destination);

        protected abstract TDest CreateDestinationInstance(TSource source);
    }
}