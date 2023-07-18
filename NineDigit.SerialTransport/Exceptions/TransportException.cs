using System;
using System.Runtime.Serialization;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// Represents exception related to data exchange between devices.
    /// </summary>
    [Serializable]
    public class TransportException : PortException
    {
        public TransportException()
            : this(message: null)
        {
        }

        internal TransportException(string? message)
            : base(message)
        {
        }

        internal TransportException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
        
        internal TransportException(string? portName, string? message)
            : base(portName, message)
        {
        }

        internal TransportException(string? portName, string? message, Exception? innerException)
            : base(portName, message, innerException)
        {
        }

        protected TransportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
