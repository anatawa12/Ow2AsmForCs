using System;
using System.Runtime.Serialization;

namespace java.io
{
    public class IOException : Exception
    {
        public IOException()
        {
        }

        protected IOException(SerializationInfo? info, StreamingContext context) : base(info, context)
        {
        }

        public IOException(string? message) : base(message)
        {
        }

        public IOException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}