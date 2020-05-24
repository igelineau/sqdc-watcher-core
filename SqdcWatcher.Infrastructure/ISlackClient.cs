using System.Threading.Tasks;

namespace SqdcWatcher.Infrastructure
{
    public interface ISlackClient
    {
        Task PostToSlackAsync(string message);
    }
}