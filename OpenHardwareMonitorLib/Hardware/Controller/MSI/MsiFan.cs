using System;
using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Hardware.Controller.MSI;

public class MsiFan
{
    /// <summary>
    /// Speed of Fan in RPM.
    /// </summary>
    public int Speed { get; set; }
    public int Duty { get; set; }

    /// <summary>
    /// Speed of Fan in percentage 0-100. This can e.g. be used to set fan curve when <see cref="MsiFanConfigure.Mode"/> is <see cref="MsiFanMode.Custom"/>.
    /// </summary>
    public MsiFanConfigure ConfigureDuty;

    /// <summary>
    /// Temperature of Fan in degrees Celsius. This can e.g. be used to set fan curve when <see cref="MsiFanConfigure.Mode"/> is <see cref="MsiFanMode.Custom"/>.
    /// </summary>
    public MsiFanConfigure ConfigureTemp;
}

public struct MsiFanConfigure
{
    public MsiFanConfigure()
    {
        if (Marshal.SizeOf<MsiFanConfigure>() != 8)
        {
            throw new InvalidOperationException($"{nameof(MsiFanConfigure)} struct size is invalid.");
        }
    }

    public MsiFanMode Mode = MsiFanMode.Unknown;
    public byte Item0;
    public byte Item1;
    public byte Item2;
    public byte Item3;
    public byte Item4;
    public byte Item5;
    public byte Item6;
}
