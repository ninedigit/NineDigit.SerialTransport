using System;
using System.Runtime.Serialization;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// Represents exception related to writing data to device.
    /// </summary>
    [Serializable]
    public class WriteTransportException : TransportException
    {
        public WriteTransportException()
            : this(GetDefaultMessage())
        {
        }
        
        public WriteTransportException(string? message)
            : this(portName: null, message)
        {
        }

        internal WriteTransportException(string? portName, string? message)
            : this(portName, message, innerException: null)
        {
        }

        internal WriteTransportException(string? portName, Exception? innerException)
            : this(portName, GetDefaultMessage(portName), innerException)
        {
        }
        
        internal WriteTransportException(string? portName, string? message, Exception? innerException)
            : base(portName, message, innerException)
        {
        }

        protected WriteTransportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        
        private static string GetDefaultMessage(string? portName = null)
            => !string.IsNullOrWhiteSpace(portName)
                ? $"Write operation to port '{portName}' failed."
                : "Write operation failed.";
    }
}