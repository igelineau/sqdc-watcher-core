namespace XFactory.SqdcWatcher.ConsoleApp.MappingFilters
{
    public interface IMappingFilter<TSource, TDest>
    {
        public void Apply(TSource source, TDest dest);
    }
}