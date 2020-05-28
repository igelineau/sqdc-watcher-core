using System.Threading.Tasks;

namespace SqdcWatcher.Infrastructure.Abstractions
{
    public interface ISlackClient
    {
        Task PostToSlackAsync(string message);
    }
}