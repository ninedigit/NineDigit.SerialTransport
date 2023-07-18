using System;
using Microsoft.Extensions.Logging;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// Helper for instantiating transport instance.
    /// </summary>
    public static class TransportFactory
    {
        /// <summary>
        /// Instantiates new transport instance.
        /// </summary>
        /// <param name="portName">Port name (or device name for mono)</param>
        /// <param name="options">Serial port options</param>
        /// <param name="loggerFactory">Logger factory</param>
        /// <returns></returns>
        public static ITransport CreateSerialTransport(string portName, SerialPortOptions options, ILoggerFactory loggerFactory)
        {
            if (loggerFactory is null)
                throw new System.ArgumentNullException(nameof(loggerFactory));

            var serialPort = CreateSerialPort(portName, options, loggerFactory);
            var serialTransportConnectionLogger = loggerFactory.CreateLogger<SerialTransportConnection>();
            var serialTransportConnection = new SerialTransportConnection(serialPort, serialTransportConnectionLogger);

            return serialTransportConnection;
        }

        /// <summary>
        /// Instantiates new serial port instance.
        /// </summary>
        /// <param name="portName">Port name (or device name for mono)</param>
        /// <param name="options">Serial port options</param>
        /// <param name="loggerFactory">Logger factory</param>
        /// <returns></returns>
        internal static ISerialPort CreateSerialPort(string portName, SerialPortOptions options, ILoggerFactory loggerFactory)
        {
            if (loggerFactory is null)
                throw new System.ArgumentNullException(nameof(loggerFactory));

            ISerialPort serialPort;
            ILogger logger = loggerFactory.CreateLogger(typeof(TransportFactory));

#if MONOANDROID
            var transportOptions = new AndroidSerialPortOptions(options);
            serialPort = new AndroidSerialPort(portName, transportOptions, loggerFactory);
            logger.LogDebug("Instantiated Android Serial Port implementation.");
#elif NET
            var serialPortOptions = new DotNetSerialPortOptions(options);
            serialPort = new DotNetSerialPort(portName, serialPortOptions, loggerFactory);
            logger.LogDebug("Instantiated .NET Standard Serial Port implementation");
#else
            throw new PlatformNotSupportedException();
#endif
            return serialPort;
        }
    }
}
