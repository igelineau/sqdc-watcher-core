#region

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace SqdcWatcher.Services
{
    public interface IScanOperation
    {
        Task Execute(bool forceProductsRefresh, CancellationToken cancellationToken);
    }
}