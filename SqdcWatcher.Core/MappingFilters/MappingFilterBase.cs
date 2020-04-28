#region

#endregion

namespace XFactory.SqdcWatcher.Core.MappingFilters
{
    public abstract class MappingFilterBase<TSource, TDest> : IMappingFilter<TSource, TDest>
        where TDest : class
    {
        public abstract void Apply(TSource source, TDest destination);
    }
}