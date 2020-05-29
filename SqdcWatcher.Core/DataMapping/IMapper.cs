namespace XFactory.SqdcWatcher.Core.DataMapping
{
    public interface IMapper<in TSource, TDest>
    {
        TDest MapCreateOrUpdate(TSource source, TDest existingDest);
    }
}