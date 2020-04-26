#region

using SqdcWatcher.Mappers;

#endregion

namespace SqdcWatcher.MappingFilters
{
    public abstract class MappingFilterBase<TSource, TDest> : IMappingFilter<TSource, TDest>
        where TDest : class
    {
        public abstract void Apply(TSource source, TDest destination);
    }
}