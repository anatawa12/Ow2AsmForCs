using System;
using System.Runtime.Serialization;

namespace java.lang
{
    public class RuntimeException : Exception
    {
        public RuntimeException()
        {
        }

        protected RuntimeException(SerializationInfo? info, StreamingContext context) : base(info, context)
        {
        }

        public RuntimeException(string? message) : base(message)
        {
        }

        public RuntimeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}