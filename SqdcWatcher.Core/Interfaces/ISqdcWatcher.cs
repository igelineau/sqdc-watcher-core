using System.Threading;
using System.Threading.Tasks;
using XFactory.SqdcWatcher.Core.Services;

namespace XFactory.SqdcWatcher.Core.Interfaces
{
    public interface ISqdcWatcher
    {
        WatcherState State { get; }
        Task Start(CancellationToken cancellationToken);
        void RequestRefresh(bool forceFullRefresh = false);
    }
}