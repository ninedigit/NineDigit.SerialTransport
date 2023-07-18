#if MONOANDROID
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Hoho.Android.UsbSerial.Driver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NineDigit.SerialTransport
{
    public static class UsbManagerExtensions
    {
        const string ACTION_USB_PERMISSION = "com.Hoho.Android.UsbSerial.Util.USB_PERMISSION";

        public static async Task<IUsbSerialPort> GetUsbSerialPortAndRequestPermissionByDeviceNameAsync(this UsbManager usbManager, string deviceName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (usbManager is null)
                throw new ArgumentNullException(nameof(usbManager));

            if (string.IsNullOrWhiteSpace(deviceName))
                throw new ArgumentException("Invalid USB device name.", nameof(deviceName));

            var ports = usbManager.GetAllUsbSerialPorts();

            if (ports.Count == 0)
                throw new InvalidOperationException("No connected USB device was found.");

            var port = ports.FirstOrDefault(p => p.Driver.Device.DeviceName == deviceName);
            if (port is null)
                throw new ArgumentException($"No connected device with port name '{deviceName}' was found.", nameof(deviceName));

            var device = port.Driver.Device;
            var context = Application.Context;
            
            var permissionGranted = await usbManager.RequestPermissionAsync(device, context, cancellationToken)
                .ConfigureAwait(false);

            if (!permissionGranted)
                throw new InvalidOperationException($"This application does not have permission to use usb device '{deviceName}'.");

            return port;
        }

        public static async Task<bool> RequestPermissionAsync(this UsbManager manager, UsbDevice device, Context context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (manager is null)
                throw new ArgumentNullException(nameof(manager));

            if (context is null)
                throw new ArgumentNullException(nameof(context));

            using var usbPermissionReceiver = new UsbPermissionReceiver(cancellationToken);
            using var intentFilter = new IntentFilter(ACTION_USB_PERMISSION);

            context.RegisterReceiver(usbPermissionReceiver, intentFilter);

            try
            {
                using var intent = new Intent(ACTION_USB_PERMISSION);
                var pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, 0);
                manager.RequestPermission(device, pendingIntent);

                return await usbPermissionReceiver.Task
                    .ConfigureAwait(false);
            }
            catch (Exception)
            {
                context.UnregisterReceiver(usbPermissionReceiver);
                throw;
            }
        }

        public static IReadOnlyCollection<IUsbSerialPort> GetAllUsbSerialPorts(this UsbManager usbManager)
        {
            if (usbManager is null)
                throw new ArgumentNullException(nameof(usbManager));

            var drivers = GetAllUsbSerialDrivers(usbManager);
            var ports = drivers.SelectMany(d => d.Ports).ToList();

            return new ReadOnlyCollection<IUsbSerialPort>(ports);
        }

        public static IList<IUsbSerialDriver> GetAllUsbSerialDrivers(this UsbManager usbManager)
        {
            if (usbManager is null)
                throw new ArgumentNullException(nameof(usbManager));
            
            // Adding a custom driver to the default probe table
            var table = UsbSerialProber.DefaultProbeTable;

            table.AddProduct(UsbId.VENDOR_JMSYSTEMS, UsbId.JMSYSTEMS_CHDULITE, Java.Lang.Class.FromType(typeof(CdcAcmSerialDriver))); // JM Systems, s.r.o. - ChduLite
            table.AddProduct(0x1b4f, 0x0008, Java.Lang.Class.FromType(typeof(CdcAcmSerialDriver))); // IOIO OTG

            using var prober = new UsbSerialProber(table);
            return prober.FindAllDrivers(usbManager);
        }

        private class UsbPermissionReceiver : BroadcastReceiver
        {
            private readonly CancellationTokenRegistration _cancellationTokenRegistration;
            private readonly TaskCompletionSource<bool> _completionSource;

            private bool _disposed = false;

            public UsbPermissionReceiver(CancellationToken cancellationToken)
            {
                this._cancellationTokenRegistration = cancellationToken.Register(this.OnCanceled);
                this._completionSource = new TaskCompletionSource<bool>();
            }

            public Task<bool> Task
                => this._completionSource.Task;

            private void OnCanceled()
                => this._completionSource.TrySetCanceled();

            public override void OnReceive(Context? context, Intent? intent)
            {
                // var device = intent.GetParcelableExtra(UsbManager.ExtraDevice) as UsbDevice;

                if (intent != null)
                {
                    var permissionGranted = intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false);
                    _completionSource.TrySetResult(permissionGranted);
                }

                context?.UnregisterReceiver(this);
            }

            protected override void Dispose(bool disposing)
            {
                if (this._disposed)
                    return;

                if (disposing)
                    this._cancellationTokenRegistration.Dispose();

                this._disposed = true;

                base.Dispose(disposing);
            }
        }
    }
}
#endif
