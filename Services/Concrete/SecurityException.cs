using System;
using System.Runtime.Serialization;

namespace VueServer.Services.Concrete
{
    [Serializable]
    internal class SecurityException : Exception
    {
        public SecurityException()
        {
        }

        public SecurityException(string message) : base(message)
        {
        }

        public SecurityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SecurityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}