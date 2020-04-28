namespace XFactory.SqdcWatcher.Core.Mappers
{
    public interface IMapper<TSource, TDest>
    {
        TDest Map(TSource source, TDest existingDest);
    }
}