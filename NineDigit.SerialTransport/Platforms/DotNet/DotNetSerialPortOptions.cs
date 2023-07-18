﻿#if NET
using System;

namespace NineDigit.SerialTransport
{
    internal sealed class DotNetSerialPortOptions
    {
        public DotNetSerialPortOptions()
        { }

        public DotNetSerialPortOptions(SerialPortOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.BaudRate = options.BaudRate;
            this.Parity = GetParity(options.Parity);
            this.DataBits = options.DataBits;
            this.StopBits = GetStopBits(options.StopBits);
            this.ReadTimeout = options.ReadTimeout;
            this.WriteTimeout = options.WriteTimeout;
        }

        private System.IO.Ports.Parity GetParity(Parity parity)
        {
            return parity switch
            {
                SerialTransport.Parity.None => System.IO.Ports.Parity.None,
                SerialTransport.Parity.Odd => System.IO.Ports.Parity.Odd,
                SerialTransport.Parity.Even => System.IO.Ports.Parity.Even,
                SerialTransport.Parity.Mark => System.IO.Ports.Parity.Mark,
                SerialTransport.Parity.Space => System.IO.Ports.Parity.Space,
                _ => throw new NotImplementedException()
            };
        }

        private System.IO.Ports.StopBits GetStopBits(StopBit stopBits)
        {
            return stopBits switch
            {
                SerialTransport.StopBit.One => System.IO.Ports.StopBits.One,
                SerialTransport.StopBit.OnePointFive => System.IO.Ports.StopBits.OnePointFive,
                SerialTransport.StopBit.Two => System.IO.Ports.StopBits.Two,
                _ => throw new NotImplementedException()
            };
        }

        public int BaudRate { get; set; } = 115200;
        public int DataBits { get; set; } = 8;
        public System.IO.Ports.Parity Parity { get; set; } = System.IO.Ports.Parity.None;
        public System.IO.Ports.StopBits StopBits { get; set; } = System.IO.Ports.StopBits.One;
        public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
        public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}
#endif