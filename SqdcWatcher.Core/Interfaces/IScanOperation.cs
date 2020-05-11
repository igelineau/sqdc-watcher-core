#region

using System.Threading;
using System.Threading.Tasks;

#endregion

namespace XFactory.SqdcWatcher.Core.Interfaces
{
    public interface IScanOperation
    {
        Task Execute(bool forceProductsRefresh, CancellationToken cancellationToken);
    }
}