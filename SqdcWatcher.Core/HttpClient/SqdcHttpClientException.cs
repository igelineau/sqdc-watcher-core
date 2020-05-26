using System;
using System.Runtime.Serialization;

namespace XFactory.SqdcWatcher.Core.HttpClient
{
    [Serializable]
    public class SqdcHttpClientException : Exception
    {
        public SqdcHttpClientException()
        {
        }

        public SqdcHttpClientException(string message) : base(message)
        {
        }

        public SqdcHttpClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqdcHttpClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}