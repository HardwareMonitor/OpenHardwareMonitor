using System.Collections.Generic;

namespace OpenHardwareMonitor.Hardware.Controller.MSI;

internal static class MsiConstants
{
    public static readonly IReadOnlyList<MsiDevice> SupportedDevices = new List<MsiDevice>
    {
        new MsiDevice(MsiDeviceType.S280, 0x0DB0, 0x75B6, 0x6A04),
        new MsiDevice(MsiDeviceType.S360, 0x0DB0, 0x9BA6, 0x6A05),
        new MsiDevice(MsiDeviceType.S360MEG, 0x1462, 0x9BA6, 0x6A05),
        new MsiDevice(MsiDeviceType.X360, 0x0DB0, 0x5259, 0x6A11),
        new MsiDevice(MsiDeviceType.X240, 0x0DB0, 0xC7B2, 0x6A10),
        new MsiDevice(MsiDeviceType.D360, 0x0DB0, 0x8DBF, 0x6A15),
        new MsiDevice(MsiDeviceType.D240, 0x0DB0, 0xD085, 0x6A16),
    };
}
