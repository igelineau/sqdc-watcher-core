namespace SqdcWatcher.MappingFilters
{
    public abstract class MappingFilterBase<TSource, TDest>
        where TSource: class
        where TDest: class
    {
        public abstract void Apply(TSource source, TDest destination);
    }
}