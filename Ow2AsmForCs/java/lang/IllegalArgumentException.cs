using System;
using System.Runtime.Serialization;

namespace java.lang
{
    public class IllegalArgumentException : Exception
    {
        public IllegalArgumentException()
        {
        }

        protected IllegalArgumentException(SerializationInfo? info, StreamingContext context) : base(info, context)
        {
        }

        public IllegalArgumentException(string? message) : base(message)
        {
        }

        public IllegalArgumentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}