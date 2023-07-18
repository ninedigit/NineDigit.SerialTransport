#if MONOANDROID
using Android.Content;
using Android.Hardware.Usb;
using Hoho.Android.UsbSerial.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NineDigit.SerialTransport
{
    /// <summary>
    /// https://github.com/mik3y/usb-serial-for-android/blob/master/usbSerialForAndroid/src/main/java/com/hoho/android/usbserial/driver/UsbSerialPort.java
    /// </summary>
    internal class AndroidSerialPort : ISerialPort
    {
        public event EventHandler<EventArgs>? OnError;

        Intent? usbDeviceDetachedIntent;
        IntentFilter? usbDeviceDetachedIntentFilter;
        UsbDeviceDetachedReceiver? detachedReceiver;
        IUsbSerialPort? port;
        bool isOpen;
        
        readonly UsbManager usbManager;
        readonly string deviceName;
        readonly int baudRate;
        readonly int dataBits;
        readonly Hoho.Android.UsbSerial.Driver.Parity parity;
        readonly Hoho.Android.UsbSerial.Driver.StopBits stopBits;
        readonly ILogger logger;
        
        public AndroidSerialPort(string deviceName, AndroidSerialPortOptions options, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                throw new ArgumentException($"Value can not be null or whitespace.", nameof(deviceName));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (options.ReadTimeout.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Read timeout must be an positive non-zero number.");

            if (options.WriteTimeout.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(options), "Write timeout must be an positive non-zero number.");

            this.ReadTimeout = (int)options.ReadTimeout.TotalMilliseconds;
            this.WriteTimeout = (int)options.WriteTimeout.TotalMilliseconds; 
            this.usbManager = GetUsbManager();
            this.deviceName = deviceName;
            this.baudRate = options.BaudRate;
            this.parity = options.Parity;
            this.dataBits = options.DataBits;
            this.stopBits = options.StopBits;
            this.logger = loggerFactory.CreateLogger<AndroidSerialPort>();
        }

        public string Name
            => this.deviceName;

        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }

        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (this.isOpen)
                return;

            var port = this.port;
            if (port is null)
            {
                this.logger.LogDebug("Getting USB Serial Port by device name '{deviceName}'.", deviceName);

                port = this.port = await this.usbManager
                    .GetUsbSerialPortAndRequestPermissionByDeviceNameAsync(deviceName, cancellationToken)
                    .ConfigureAwait(false);
            }

            var isOpen = this.isOpen;
            if (!isOpen)
            {
                this.logger.LogDebug("Opening USB Serial Port device with name '{deviceName}' ...", port.Driver.Device.DeviceName);

                var connection = this.usbManager.OpenDevice(port.Driver.Device);
                if (connection is null)
                    throw new InvalidOperationException($"Failed to open device {port.Driver.Device.DeviceName}.");

                try
                {
                    this.logger.LogDebug("Opening USB Serial Port connection ...");

                    port.Open(connection);

                    var detachedReceiver = this.detachedReceiver = new UsbDeviceDetachedReceiver();
                    var intentFilter = this.usbDeviceDetachedIntentFilter = new IntentFilter(UsbManager.ActionUsbDeviceDetached);

                    void onUsbDeviceDetachedHandler(UsbDevice device)
                    {
                        if (device.DeviceName == port.Driver.Device.DeviceName)
                        {
                            this.logger.LogDebug("USB Serial Port device '{deviceName}' was detached.", device.DeviceName);

                            detachedReceiver.Detached -= onUsbDeviceDetachedHandler;
                            this.Close();
                            this.OnError?.Invoke(this, EventArgs.Empty);
                        }
                    }

                    detachedReceiver.Detached += onUsbDeviceDetachedHandler;

                    this.usbDeviceDetachedIntent = Android.App.Application.Context.RegisterReceiver(detachedReceiver, intentFilter);

                    this.isOpen = true;

                    port.SetParameters(this.baudRate, this.dataBits, this.stopBits, this.parity);
                    
                    this.DiscardBuffers();

                    // this.SetConnected();
                }
                catch (Exception)
                {
                    this.detachedReceiver?.Dispose();
                    this.usbDeviceDetachedIntentFilter?.Dispose();
                    this.usbDeviceDetachedIntent?.Dispose();
                    this.isOpen = false;

                    // this.SetDisconnected(ex);
                    throw;
                }
            }
        }

        public void Write(byte[] data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var serialPort = this.EnsureSerialPort();

            this.logger.LogDebug(
                "Writing {bytesCound} bytes of data to Serial Port with number {serialPortNumber} ...",
                data.Length, serialPort.PortNumber);

            serialPort.Write(data, this.WriteTimeout);
        }

        public byte[] Read(int responseLength)
        {
            var serialPort = this.EnsureSerialPort();

            this.logger.LogDebug(
               "Reading {bytesCount} bytes from Serial Port with number {serialPortNumber} ...",
               responseLength, serialPort.PortNumber);

            return serialPort.Read(responseLength, this.ReadTimeout, this.logger);
        }

        public void DiscardBuffers()
        {
            if (this.isOpen)
                return;

            //this.logger.LogDebug("Discarding Serial Port buffers ...");

            //this.port?.PurgeHwBuffers(
            //    true,   // discard non-transmitted output data
            //    true);  // to discard non-read input data
        }

        public void Close()
        {
            if (this.isOpen)
            {
                this.logger.LogDebug("Closing Serial Port ...");

                this.isOpen = false;

                this.port?.Close();

                this.detachedReceiver?.Dispose();
                this.detachedReceiver = null;

                this.usbDeviceDetachedIntent?.Dispose();
                this.usbDeviceDetachedIntent = null;

                this.usbDeviceDetachedIntentFilter?.Dispose();
                this.usbDeviceDetachedIntentFilter = null;
            }
        }

        private IUsbSerialPort EnsureSerialPort()
        {
            var port = this.port;
            if (port is null)
                throw new InvalidOperationException("Port is not open.");

            return port;
        }

        private static UsbManager GetUsbManager()
            => (UsbManager)Android.App.Application.Context.GetSystemService(Context.UsbService)!;

        #region UsbDeviceDetachedReceiver implementation
        class UsbDeviceDetachedReceiver : BroadcastReceiver
        {
            public event Action<UsbDevice>? Detached;

            public UsbDeviceDetachedReceiver()
            { }

            public override void OnReceive(Context? context, Intent? intent)
            {
                var device = intent?.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;
                if (device != null)
                    this.Detached?.Invoke(device);
            }
        }
        #endregion
        
        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.logger.LogDebug("Disposing Serial Port ...");

                    this.Close();
                    this.port?.Dispose();
                    this.port = null;
                    this.usbManager.Dispose();
                }

                disposedValue = true;
            }
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