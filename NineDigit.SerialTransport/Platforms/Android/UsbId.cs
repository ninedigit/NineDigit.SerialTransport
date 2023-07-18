#if MONOANDROID
namespace NineDigit.SerialTransport
{
    /// <summary>
    /// List of additional USB device vendors/products
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "<Pending>")]
    public static class UsbId
    {
        /// <summary>
        /// JM Systems, s.r.o.
        /// </summary>
        public const int VENDOR_JMSYSTEMS = 0x0483;
        /// <summary>
        /// CHDU Lite >=v1.0
        /// </summary>
        public const int JMSYSTEMS_CHDULITE = 0x5740;
    }
}
#endif