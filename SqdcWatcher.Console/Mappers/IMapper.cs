namespace SqdcWatcher.Mappers
{
    public interface IMapper<TSource, TDest>
    {
        public TDest Map(TSource source, TDest existingDest);
    }
}