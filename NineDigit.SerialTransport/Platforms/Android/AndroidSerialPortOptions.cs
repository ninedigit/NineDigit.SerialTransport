#if MONOANDROID
using System;

namespace NineDigit.SerialTransport
{
    internal sealed class AndroidSerialPortOptions
    {
        public AndroidSerialPortOptions()
        { }

        public AndroidSerialPortOptions(SerialPortOptions options)
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

        private Hoho.Android.UsbSerial.Driver.Parity GetParity(Parity parity)
        {
            switch (parity)
            {
                case SerialTransport.Parity.None: return Hoho.Android.UsbSerial.Driver.Parity.None;
                case SerialTransport.Parity.Odd: return Hoho.Android.UsbSerial.Driver.Parity.Odd;
                case SerialTransport.Parity.Even: return Hoho.Android.UsbSerial.Driver.Parity.Even;
                case SerialTransport.Parity.Mark: return Hoho.Android.UsbSerial.Driver.Parity.Mark;
                case SerialTransport.Parity.Space: return Hoho.Android.UsbSerial.Driver.Parity.Space;
                default: throw new NotImplementedException();
            }
        }

        private Hoho.Android.UsbSerial.Driver.StopBits GetStopBits(StopBit stopBits)
        {
            switch (stopBits)
            {
                case SerialTransport.StopBit.One: return Hoho.Android.UsbSerial.Driver.StopBits.One;
                case SerialTransport.StopBit.OnePointFive: return Hoho.Android.UsbSerial.Driver.StopBits.OnePointFive;
                case SerialTransport.StopBit.Two: return Hoho.Android.UsbSerial.Driver.StopBits.Two;
                default: throw new NotImplementedException();
            }
        }

        public int BaudRate { get; set; } = 115200;
        public int DataBits { get; set; } = 8;
        public Hoho.Android.UsbSerial.Driver.Parity Parity { get; set; } = Hoho.Android.UsbSerial.Driver.Parity.None;
        public Hoho.Android.UsbSerial.Driver.StopBits StopBits { get; set; } = Hoho.Android.UsbSerial.Driver.StopBits.One;
        public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
        public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
    }
}
#endif