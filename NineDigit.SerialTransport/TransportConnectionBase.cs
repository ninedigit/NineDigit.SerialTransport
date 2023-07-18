using System;
using System.IO;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// Base implementation of transport connection
    /// </summary>
    internal abstract class TransportConnectionBase : ITransportConnection, IDisposable
    {
        /// <summary>
        /// Raised when transport error occurs.
        /// </summary>
        public event EventHandler<ErrorEventArgs>? Error;
        /// <summary>
        /// Raised when trasnport connection state is changed.
        /// </summary>
        public event EventHandler<TransportConnectionStateChange>? StateChanged;

        /// <summary>
        /// Current transport connection state.
        /// </summary>
        public TransportConnectionState State { get; private set; }

        /// <summary>
        /// Last error that caused disconnecting of trasnport connection.
        /// Value is non-null only when <see cref="State"/> is equal to <see cref="TransportConnectionState.Disconnected"/>.
        /// </summary>
        /// <value><see cref="Exception"/> or <c>null</c>.</value>
        public Exception? LastError { get; private set; }

        /// <summary>
        /// Sets connection state to <see cref="TransportConnectionState.Connected"/> and clears last error.
        /// </summary>
        protected void SetConnected()
        {
            this.LastError = null;
            this.SetState(TransportConnectionState.Connected);
        }

        /// <summary>
        /// Sets connection state to <see cref="TransportConnectionState.Disconnected"/> and raises the <see cref="Error"/> event.
        /// </summary>
        protected void SetDisconnected()
        {
            this.LastError = null;
            this.SetState(TransportConnectionState.Disconnected);
        }

        /// <summary>
        /// Sets connection state to <see cref="TransportConnectionState.Disconnected"/>, stores given <paramref name="ex"/> and raises the <see cref="Error"/> event.
        /// </summary>
        /// <param name="ex"></param>
        protected void SetDisconnected(Exception ex)
        {
            this.RaiseError(ex);
            this.SetState(TransportConnectionState.Disconnected);
        }

        private void SetState(TransportConnectionState state)
        {
            var oldState = this.State;
            if (oldState != state)
            {
                this.State = state;
                var eventArg = new TransportConnectionStateChange(state, oldState);

                this.StateChanged?.Invoke(this, eventArg);
            }
        }

        private void RaiseError(Exception ex)
        {
            this.LastError = ex ?? throw new ArgumentNullException(nameof(ex));
            var errEventArgs = new ErrorEventArgs(ex);
            this.Error?.Invoke(this, errEventArgs);
        }

        #region IDisposable
        bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            
            if (disposing)
                this.SetDisconnected();

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
