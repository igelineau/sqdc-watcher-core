#region

using System.Threading;

#endregion

namespace SqdcWatcher.Services
{
    public interface ISqdcWatcher
    {
        WatcherState State { get; }
        void Start(CancellationToken cancelToken);
        void RequestRefresh(bool forceFullRefresh = false);
    }
}