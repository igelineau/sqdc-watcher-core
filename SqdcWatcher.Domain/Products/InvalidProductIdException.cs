using System;
using System.Runtime.Serialization;

namespace XFactory.SqdcWatcher.Data.Entities.Products
{
    [Serializable]
    public class InvalidProductIdException : Exception
    {
        public string Id { get; }
        
        public InvalidProductIdException(string message) : base(message)
        {
        }

        public InvalidProductIdException(string message, string id) : base(message)
        {
            Id = id;
        }

        public InvalidProductIdException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidProductIdException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}