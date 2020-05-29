using System;
using System.Runtime.Serialization;

namespace CannaWatch.Markets.CannaFarms
{
    [Serializable]
    public class CannaFarmsMarketFetchException : Exception
    {
        public CannaFarmsMarketFetchException()
        {
        }

        public CannaFarmsMarketFetchException(string message) : base(message)
        {
        }

        public CannaFarmsMarketFetchException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CannaFarmsMarketFetchException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}