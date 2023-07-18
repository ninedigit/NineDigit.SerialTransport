using System;
using System.Threading;
using System.Threading.Tasks;

namespace NineDigit.SerialTransport
{
    public interface ISerialPort : IDisposable
    {
        event EventHandler<EventArgs> OnError;

        string Name { get; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }

        Task OpenAsync(CancellationToken cancellationToken);
        void Write(byte[] data);
        byte[] Read(int responseLength);
        void DiscardBuffers();
        void Close();
    }
}
