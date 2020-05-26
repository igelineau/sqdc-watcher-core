using JetBrains.Annotations;

namespace SqdcWatcher.DataTransferObjects.RestApiModels
{
    [PublicAPI]
    public class BaseResponse
    {
        public string Message { get; private set; }
    }
}