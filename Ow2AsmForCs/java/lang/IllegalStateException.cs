using System;
using System.Runtime.Serialization;

namespace java.lang
{
    public class IllegalStateException : Exception
    {
        public IllegalStateException()
        {
        }

        protected IllegalStateException(SerializationInfo? info, StreamingContext context) : base(info, context)
        {
        }

        public IllegalStateException(string? message) : base(message)
        {
        }

        public IllegalStateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}