#region

using System.Collections.Generic;
using System.Linq;
using XFactory.SqdcWatcher.ConsoleApp.MappingFilters;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Mappers
{
    public abstract class MapperBase<TSource, TDest> : IMapper<TSource, TDest> where TDest : new()
    {
        protected readonly IReadOnlyCollection<IMappingFilter<TSource, TDest>> MappingFilters;

        protected MapperBase(IEnumerable<IMappingFilter<TSource, TDest>> mappingFilters)
        {
            MappingFilters = mappingFilters.ToList().AsReadOnly();
        }

        public TDest Map(TSource source, TDest dest)
        {
            if (dest == null)
            {
                dest = CreateDestinationInstance();
            }

            foreach (IMappingFilter<TSource, TDest> filter in MappingFilters)
            {
                filter.Apply(source, dest);
            }

            return PerformMapping(source, dest);
        }

        protected abstract TDest PerformMapping(TSource source, TDest dest);

        protected virtual TDest CreateDestinationInstance() => new TDest();
    }
}