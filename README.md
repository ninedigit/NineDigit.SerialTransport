
# NineDigit Serial Transport

[![NuGet version (NineDigit.SerialTransport)](https://img.shields.io/nuget/v/NineDigit.SerialTransport)](https://www.nuget.org/packages/NineDigit.SerialTransport/)

Multiplatform implementation of data exchange over serial port for .net standard and mono platforms.

## Usage

```csharp
SerialPortOptions opts = new SerialPortOptions()
{
    BaudRate = 19200,
    ReadTimeout = TimeSpan.FromSeconds(1),
    WriteTimeout = TimeSpan.FromSeconds(1)
};
            
string portName = "/dev/ttyS1"; // e.g. "COM3" for Windows

ISerialTransport serialTransport = TransportFactory.CreateSerialTransport(portName, opts, NullLoggerFactory.Instance);

// write data
await serialTransport.WriteAsync(new byte[] { 0x01, 0x02, 0x03 }, CancellationToken.None);

// read data
byte[] response = await serialTransport.ReadAsync(responseLength: 1, CancellationToken.None);
```