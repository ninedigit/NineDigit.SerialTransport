using System;
using System.Threading;
using System.Threading.Tasks;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// Interface for transport layer that exchanges byte array between two endpoints.
    /// </summary>
    public interface ITransport : ITransportConnection, IDisposable
    {
        /// <summary>
        /// Executes data exchange with another device.
        /// </summary>
        /// <param name="data">Data to be written.</param>
        /// <param name="responseLength">Expected resposne length (in bytes)</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Response received from opposite endpoint.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="data"/> contains no elements.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="responseLength"/> is negative.</exception>
        /// <exception cref="OperationCanceledException">Operation was canceled.</exception>
        /// <exception cref="TransportException">Error during writing/reading data.</exception>
        Task<byte[]> WriteAndReadAsync(byte[] data, long responseLength, CancellationToken cancellationToken);
        
        Task WriteAsync(byte[] data, CancellationToken cancellationToken);        
        
        Task<byte[]> ReadAsync(long responseLength, CancellationToken cancellationToken);

        /// <summary>
        /// Discards input and output buffer.
        /// </summary>
        void DiscardBuffers();

        /// <summary>
        /// Disconnects from the other endpoint.
        /// </summary>
        /// <param name="ex">Error that casuses the disconnection.</param>
        void Disconnect(Exception ex);
    }

    public static class ITransportExtensions
    {
        public static Task WriteOneAsync(this ITransport self, byte data, CancellationToken cancellationToken)
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));

            return self.WriteAsync(new byte[] { data }, cancellationToken);
        }
        
        public static async Task<byte> ReadOneAsync(this ITransport self, CancellationToken cancellationToken)
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));

            var buffer = await self.ReadAsync(responseLength: 1, cancellationToken).ConfigureAwait(false);
            return buffer[0];
        }

        /// <summary>
        /// </summary>
        /// <param name="self">Transport</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains the total number of bytes read into the buffer.
        /// </returns>
        public static async Task<int> ReadAsync(this ITransport self, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (self is null)
                throw new ArgumentNullException(nameof(self));

            var bytes = await self.ReadAsync(count, cancellationToken).ConfigureAwait(false);
            bytes.CopyTo(buffer, offset);
            return bytes.Length;
        }
    }
}
