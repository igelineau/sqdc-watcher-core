using System;
using System.Runtime.Serialization;

namespace SqdcWatcher
{
    [Serializable]
    internal class WatcherStartException : Exception
    {
        public WatcherStartException()
        {
        }

        public WatcherStartException(string message) : base(message)
        {
        }

        public WatcherStartException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WatcherStartException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}