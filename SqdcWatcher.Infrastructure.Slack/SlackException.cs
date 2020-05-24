using System;
using System.Runtime.Serialization;

namespace SqdcWatcher.Slack
{
    [Serializable]
    public class SlackException : Exception
    {
        public SlackException()
        {
        }

        public SlackException(string message) : base(message)
        {
        }

        public SlackException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SlackException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}