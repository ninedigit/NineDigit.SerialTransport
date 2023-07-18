#if NET
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace NineDigit.SerialTransport
{
    internal sealed class DotNetSerialPort : ISerialPort
    {
        public event EventHandler<EventArgs>? OnError;

        readonly SerialPort serialPort;
        readonly ILogger logger;

        /// <summary>
        /// </summary>
        /// <param name="portName">Názov sériového portu, napríklad COM1 alebo /dev/ttyS0.</param>
        /// <param name="options">Communication options.</param>
        public DotNetSerialPort(string portName, DotNetSerialPortOptions options)
            : this(portName, options, NullLoggerFactory.Instance)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="portName">Názov sériového portu, napríklad COM1 alebo /dev/ttyS0.</param>
        /// <param name="options">Communication options.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public DotNetSerialPort(string portName, DotNetSerialPortOptions options, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(portName))
                throw new ArgumentException("Invalid serial port name.", nameof(portName));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (options.BaudRate <= 0)
                throw new ArgumentException("Baud rate must be an positive number.", nameof(options));

            if (options.ReadTimeout.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Read timeout must be an positive non-zero number.");

            if (options.WriteTimeout.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Write timeout must be an positive non-zero number.");

            this.serialPort = new SerialPort(portName, options.BaudRate, options.Parity, options.DataBits, options.StopBits)
            {
                WriteTimeout = (int)options.WriteTimeout.TotalMilliseconds,
                ReadTimeout = (int)options.ReadTimeout.TotalMilliseconds
            };
            this.serialPort.ErrorReceived += OnSerialPortErrorReceived;

            this.logger = loggerFactory.CreateLogger<DotNetSerialPort>();
        }

        public string Name
            => this.serialPort.PortName;

        public int ReadTimeout
        {
            get => this.serialPort.ReadTimeout;
            set => this.serialPort.ReadTimeout = value;
        }

        public int WriteTimeout
        {
            get => this.serialPort.WriteTimeout;
            set => this.serialPort.WriteTimeout = value;
        }

        private void OnSerialPortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            this.OnError?.Invoke(this, e);
        }

        public Task OpenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.EnsurePortIsOpen();
            return Task.CompletedTask;
        }

        private void EnsurePortIsOpen()
        {
            try
            {
                if (this.serialPort.IsOpen)
                    return;
                
                this.serialPort.Open();
                this.DiscardBuffers();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new BusyPortException( this.serialPort.PortName, $"Port '{this.serialPort.PortName}' is busy.", ex);
            }
            catch (Exception ex) // System.IO.FileNotFoundException
            {
                throw new TransportException(this.serialPort.PortName, $"Unable to connect to the device connected at {this.serialPort.PortName}.", ex);
            }
        }

        public void Write(byte[] data)
        {
            try
            {
                this.EnsurePortIsOpen();
                this.serialPort.Write(data, 0, data.Length);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TransportException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WriteTransportException(this.serialPort.PortName, "Failed to write data to the port.", ex);
            }
        }

        public byte[] Read(int responseLength)
        {
            var stream = this.serialPort.BaseStream;
            var result = new byte[responseLength];
            var bytesToRead = responseLength;
            var totalBytesRead = 0;

            while (bytesToRead > 0)
            {
                var bytesRead = stream.Read(result, totalBytesRead, bytesToRead);
                if (bytesRead == 0)
                    break;

                bytesToRead -= bytesRead;
                totalBytesRead += bytesRead;
            }

            if (totalBytesRead != responseLength)
                throw new InvalidOperationException($"Incomplete message received. Expected bytes: {responseLength}, received bytes: {totalBytesRead}.");

            return result;
        }

        public void DiscardBuffers()
        {
            if (!this.serialPort.IsOpen)
                return;
            
            this.serialPort.DiscardInBuffer();
            this.serialPort.DiscardOutBuffer();
        }

        public void Close()
        {
            if (this.serialPort.IsOpen)
                this.serialPort.Close();
        }

#region IDisposable
        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            
            if (disposing)
            {
                try
                {
                    this.serialPort.ErrorReceived -= OnSerialPortErrorReceived;
                    this.serialPort.Dispose();
                }
                // "Port does not exists" occurs if device has been plugged out physically.
                catch (IOException) { }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
#endregion
    }
}
#endif