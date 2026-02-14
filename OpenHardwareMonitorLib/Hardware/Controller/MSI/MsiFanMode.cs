namespace OpenHardwareMonitor.Hardware.Controller.MSI;

public enum MsiFanMode : byte
{
    Silent = 0,
    Bios = 1,
    Game = 2,
    Custom = 3,
    Unknown = 4,
    Smart = 5,
}
