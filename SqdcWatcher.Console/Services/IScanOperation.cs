#region

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Services
{
    public interface IScanOperation
    {
        Task Execute(bool forceProductsRefresh, CancellationToken cancellationToken);
    }
}