namespace SqdcWatcher.Mappers
{
    public interface IMappingFilter<TSource, TDest>
    {
        public void Apply(TSource source, TDest dest);
    }
}