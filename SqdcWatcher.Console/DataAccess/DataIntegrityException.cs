#region

using System;
using System.Runtime.Serialization;

#endregion

namespace SqdcWatcher.DataAccess
{
    [Serializable]
    public class DataIntegrityException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public DataIntegrityException()
        {
        }

        public DataIntegrityException(string message) : base(message)
        {
        }

        public DataIntegrityException(string message, Exception inner) : base(message, inner)
        {
        }

        protected DataIntegrityException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}