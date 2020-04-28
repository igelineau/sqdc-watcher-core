#region

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace XFactory.SqdcWatcher.Core.Services
{
    public interface IScanOperation
    {
        Task Execute(bool forceProductsRefresh, CancellationToken cancellationToken);
    }
}