using System.Runtime.Serialization;
using System;

namespace NineDigit.SerialTransport
{
    [Serializable]
    public class PortException : Exception
    {
        public PortException()
            : this(message: null)
        {
        }

        public PortException(string? message)
            : this(message, innerException: null)
        {
        }

        public PortException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
        
        public PortException(string? portName, string? message)
            : base(message)
        {
            this.PortName = portName;
        }

        public PortException(string? portName, string? message, Exception? innerException)
            : base(message, innerException)
        {
            this.PortName = portName;
        }
        
        protected PortException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            foreach (var entry in info)
            {
                switch (entry.Name)
                {
                    case "PortName":
                        this.PortName = (string?)entry.Value;
                        break;
                }
            }
        }
        
        public string? PortName { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            
            if (this.PortName != null)
                info.AddValue("PortName", this.PortName);
        }
    }
}