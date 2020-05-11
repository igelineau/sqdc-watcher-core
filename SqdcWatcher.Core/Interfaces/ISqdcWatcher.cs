#region

using System.Threading;
using System.Threading.Tasks;
using XFactory.SqdcWatcher.Core.Services;

#endregion

namespace XFactory.SqdcWatcher.Core.Interfaces
{
    public interface ISqdcWatcher
    {
        WatcherState State { get; }
        Task Start(CancellationToken cancelToken);
        void RequestRefresh(bool forceFullRefresh = false);
    }
}