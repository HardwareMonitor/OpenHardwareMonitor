namespace OpenHardwareMonitor.Hardware.Motherboard.Lpc.EC;

public class EmbeddedControllerSource
{
    public EmbeddedControllerSource(string name, SensorType type, ushort register, byte size = 1, float factor = 1.0f, float offset = 0.0f, int blank = int.MaxValue, bool isLittleEndian = false)
    {
        Name = name;

        Register = register;
        Size = size;
        Type = type;
        Factor = factor;
        Offset = offset;
        Blank = blank;
        IsLittleEndian = isLittleEndian;
    }

    public string Name { get; }
    public ushort Register { get; }
    public byte Size { get; }
    public float Factor { get; }
    public float Offset { get; }
    public bool IsLittleEndian { get; }

    public int Blank { get; }

    public EmbeddedControllerReader Reader { get; }

    public SensorType Type { get; }
}
