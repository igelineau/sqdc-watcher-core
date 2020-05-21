namespace XFactory.SqdcWatcher.Core.MappingFilters
{
    public interface IMappingFilter<in TSource, in TDest>
    {
        public void Apply(TSource source, TDest destination);
    }
}