using System.Threading;

namespace SqdcWatcher.Services
{
    public interface ISqdcWatcher
    {
        void Start(CancellationToken cancelToken);
        void RequestRefresh();
        WatcherState State { get; }
    }
}