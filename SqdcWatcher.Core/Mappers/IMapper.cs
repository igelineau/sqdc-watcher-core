namespace XFactory.SqdcWatcher.Core.Mappers
{
    public interface IMapper<in TSource, TDest>
    {
        TDest Map(TSource source, TDest existingDest);
    }
}