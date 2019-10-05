using System;
using System.Net;
using System.Runtime.Serialization;

namespace SqdcWatcher.Services
{
    [Serializable]
    internal class SqdcRestRequestException : Exception
    {
        private Exception errorException;
        private HttpStatusCode statusCode;

        public SqdcRestRequestException()
        {
        }

        public SqdcRestRequestException(string message) : base(message)
        {
        }

        public SqdcRestRequestException(Exception errorException, HttpStatusCode statusCode)
        {
            this.errorException = errorException;
            this.statusCode = statusCode;
        }

        public SqdcRestRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqdcRestRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}