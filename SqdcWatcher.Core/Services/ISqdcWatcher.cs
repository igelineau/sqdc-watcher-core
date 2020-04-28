#region

using System.Threading;

#endregion

namespace XFactory.SqdcWatcher.Core.Services
{
    public interface ISqdcWatcher
    {
        WatcherState State { get; }
        void Start(CancellationToken cancelToken);
        void RequestRefresh(bool forceFullRefresh = false);
    }
}