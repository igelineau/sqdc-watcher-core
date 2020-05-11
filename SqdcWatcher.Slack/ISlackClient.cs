using System.Threading.Tasks;

namespace SqdcWatcher.Slack
{
    public interface ISlackClient
    {
        Task PostToSlackAsync(string message);
    }
}