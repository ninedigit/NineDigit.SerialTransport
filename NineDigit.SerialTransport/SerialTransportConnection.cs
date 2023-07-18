using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// Platform-independent implementation of serial transport
    /// </summary>
    internal class SerialTransportConnection : TransportConnectionBase, ITransport
    {
        private readonly ISerialPort _serialPort;
        private readonly ILogger<SerialTransportConnection> _logger;

        public SerialTransportConnection(ISerialPort serialPort, ILogger<SerialTransportConnection> logger)
        {
            this._serialPort = serialPort
                ?? throw new ArgumentNullException(nameof(serialPort));

            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            this._serialPort.OnError += SerialPort_OnError;
        }

        private void SerialPort_OnError(object sender, EventArgs e)
        {
            this._logger.LogError("Received Serial Port error event");
            this._serialPort.Close();
            this.SetDisconnected();
        }

        public Task WriteAsync(byte[] data, CancellationToken cancellationToken)
            => WriteAndReadInternalAsync(data, responseLength: null, cancellationToken);

        public Task<byte[]> ReadAsync(long responseLength, CancellationToken cancellationToken)
            => WriteAndReadInternalAsync(null, responseLength, cancellationToken)!;

        public Task<byte[]> WriteAndReadAsync(byte[] data, long responseLength, CancellationToken cancellationToken)
            => WriteAndReadInternalAsync(data, responseLength, cancellationToken)!;

        private async Task<byte[]?> WriteAndReadInternalAsync(byte[]? data, long? responseLength, CancellationToken cancellationToken)
        {
            if (responseLength < 0 || responseLength > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(responseLength));

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this._serialPort.OpenAsync(cancellationToken).ConfigureAwait(false);
                this.SetConnected();
            }
            catch (Exception ex)
            {
                this.SetDisconnected(ex);
                throw;
            }

            if (data != null)
            {
                try
                { 
                    cancellationToken.ThrowIfCancellationRequested();
                    this._serialPort.Write(data);
                }
                catch (TransportException ex)
                {
                    this.SetDisconnected(ex);
                    throw;
                }
                catch (OperationCanceledException ex)
                {
                    this.SetDisconnected(ex);
                    throw new WriteTransportException(this._serialPort.Name, $"Write operation to port '{this._serialPort.Name}' was cancelled.", ex);
                }
                catch (TimeoutException ex)
                {
                    this.SetDisconnected(ex);
                    throw new WriteTimeoutException(this._serialPort.WriteTimeout, this._serialPort.Name, ex);
                }
                catch (Exception ex)
                {
                    this.SetDisconnected(ex);
                    throw new WriteTransportException($"An error has occured while writing to serial port '{this._serialPort.Name}'.", ex);
                }
            }

            if (responseLength.HasValue)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return this._serialPort.Read((int)responseLength.Value);
                }
                catch (TransportException ex)
                {
                    this.SetDisconnected(ex);
                    throw;
                }
                catch (OperationCanceledException ex)
                {
                    this.SetDisconnected(ex);
                    throw new ReadTransportException(this._serialPort.Name, $"Write operation from port '{this._serialPort.Name}' was cancelled.", ex);
                }
                catch (TimeoutException ex)
                {
                    this.SetDisconnected(ex);
                    throw new ReadTimeoutException(this._serialPort.ReadTimeout, this._serialPort.Name, ex);
                }
                catch (Exception ex)
                {
                    this.SetDisconnected(ex);
                    throw new ReadTransportException($"An error has occured while reading from serial port '{this._serialPort.Name}'.", ex);
                }
            }

            return null;
        }

        public void DiscardBuffers()
            => this._serialPort.DiscardBuffers();

        public void Disconnect(Exception ex)
        {
            this._logger.LogDebug(ex, "Disconnecting with error");
            this._serialPort.Close();
            this.SetDisconnected(ex);
        }
        
        #region IDisposable
        private bool _disposed;
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
                return;
            
            if (disposing)
            {
                this._serialPort.OnError -= SerialPort_OnError;
                this._serialPort?.Dispose();
            }

            _disposed = true;
        }
        #endregion
    }
}
