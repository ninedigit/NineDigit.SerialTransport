using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using NineDigit.SerialTransport;

namespace NineDigit.SerialTransport.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var portName = "/dev/cu.usbmodem205C377548521";
            var serialPortOptions = new SerialPortOptions
            {
                Parity = Parity.None,
                BaudRate = 9600,
                DataBits = 8
            };

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            var serialTransport = TransportFactory.CreateSerialTransport(portName, serialPortOptions, loggerFactory);

            // Read CHDU Lite status
            var data = new byte[] { 0x02, 0x01, 0x00, 0x5A, 0x04 };

            var result = serialTransport.WriteAndReadAsync(data, 29, CancellationToken.None)
                .GetAwaiter().GetResult();
        }
    }
}
