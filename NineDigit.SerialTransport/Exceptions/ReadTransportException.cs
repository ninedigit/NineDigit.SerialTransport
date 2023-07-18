using System;
using System.Runtime.Serialization;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// Represents exception related to writing data to device.
    /// </summary>
    [Serializable]
    public class ReadTransportException : TransportException
    {
        public ReadTransportException()
            : this(portName: null, GetDefaultMessage())
        {
        }
        
        public ReadTransportException(string? message)
            : this(portName: null, message)
        {
        }

        internal ReadTransportException(string? portName, string? message)
            : this(portName, message, innerException: null)
        {
        }

        internal ReadTransportException(string? portName, Exception? innerException)
            : this(portName, GetDefaultMessage(portName), innerException)
        {
        }
        
        internal ReadTransportException(string? portName, string? message, Exception? innerException)
            : base(portName, message, innerException)
        {
        }

        protected ReadTransportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        private static string GetDefaultMessage(string? portName = null)
            => !string.IsNullOrWhiteSpace(portName)
                ? $"Read operation from port '{portName}' failed."
                : "Read operation failed.";
    }
}