using System.Threading;

namespace SqdcWatcher
{
    public interface ISqdcWatcher
    {
        void start(CancellationToken cancelToken);
    }
}