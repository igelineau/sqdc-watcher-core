namespace XFactory.SqdcWatcher.Core.MappingFilters
{
    public interface IMappingFilter<TSource, TDest>
    {
        public void Apply(TSource source, TDest dest);
    }
}