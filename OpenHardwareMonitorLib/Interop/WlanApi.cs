using System;
using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Interop;

internal static class WlanApi
{
    internal const string DllName = "wlanapi.dll";

    private const uint WLAN_CLIENT_VERSION_VISTA = 2;
    private const int WLAN_INTF_OPCODE_CURRENT_CONNECTION = 7;
    private const int ERROR_SUCCESS = 0;

    // WLAN_CONNECTION_ATTRIBUTES: isState (4) + wlanConnectionMode (4) + strProfileName (512),
    // then WLAN_ASSOCIATION_ATTRIBUTES (SSID length at 0, SSID bytes at 4, signal quality at 56, rx/tx rate in Kbps at 60/64).
    private const int AssociationOffset = 8 + 512;
    private const int SsidBytesOffset = AssociationOffset + 4;
    private const int SignalQualityOffset = AssociationOffset + 56;
    private const int RxRateOffset = AssociationOffset + 60;
    private const int TxRateOffset = AssociationOffset + 64;
    private const int MaxSsidLength = 32;

    internal readonly struct Connection
    {
        public Connection(int signalQuality, string ssid, uint rxRateKbps, uint txRateKbps)
        {
            SignalQuality = signalQuality;
            Ssid = ssid;
            RxRateKbps = rxRateKbps;
            TxRateKbps = txRateKbps;
        }

        public int SignalQuality { get; }
        public string Ssid { get; }
        public uint RxRateKbps { get; }
        public uint TxRateKbps { get; }
    }

    [DllImport(DllName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int WlanOpenHandle(uint clientVersion, IntPtr reserved, out uint negotiatedVersion, out IntPtr clientHandle);

    [DllImport(DllName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int WlanCloseHandle(IntPtr clientHandle, IntPtr reserved);

    [DllImport(DllName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int WlanQueryInterface(IntPtr clientHandle, ref Guid interfaceGuid, int opCode, IntPtr reserved, out int dataSize, out IntPtr data, IntPtr opcodeValueType);

    [DllImport(DllName)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern void WlanFreeMemory(IntPtr memory);

    /// <summary>
    /// Opens a WLAN client handle, or returns <see cref="IntPtr.Zero"/> if the WLAN service is unavailable.
    /// </summary>
    internal static IntPtr Open()
    {
        try
        {
            return WlanOpenHandle(WLAN_CLIENT_VERSION_VISTA, IntPtr.Zero, out _, out IntPtr handle) == ERROR_SUCCESS ? handle : IntPtr.Zero;
        }
        catch (DllNotFoundException)
        {
            return IntPtr.Zero;
        }
        catch (EntryPointNotFoundException)
        {
            return IntPtr.Zero;
        }
    }

    internal static void Close(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
            WlanCloseHandle(handle, IntPtr.Zero);
    }

    /// <summary>
    /// Gets the current connection of the interface, or null if it is not connected or access is denied.
    /// </summary>
    internal static Connection? GetConnection(IntPtr handle, Guid interfaceGuid)
    {
        if (WlanQueryInterface(handle, ref interfaceGuid, WLAN_INTF_OPCODE_CURRENT_CONNECTION, IntPtr.Zero, out int size, out IntPtr data, IntPtr.Zero) != ERROR_SUCCESS)
            return null;

        try
        {
            if (size <= TxRateOffset)
                return null;

            int ssidLength = Math.Min(Marshal.ReadInt32(data, AssociationOffset), MaxSsidLength);
            byte[] ssid = new byte[ssidLength];
            Marshal.Copy(data + SsidBytesOffset, ssid, 0, ssidLength);

            return new Connection(
                Marshal.ReadInt32(data, SignalQualityOffset),
                System.Text.Encoding.UTF8.GetString(ssid),
                (uint)Marshal.ReadInt32(data, RxRateOffset),
                (uint)Marshal.ReadInt32(data, TxRateOffset));
        }
        finally
        {
            WlanFreeMemory(data);
        }
    }
}
