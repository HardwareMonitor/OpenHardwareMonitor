namespace OpenHardwareMonitor.Hardware.Memory;

internal sealed class TotalMemory : Hardware
{
    public TotalMemory(ISettings settings)
        : base("Physical Memory", new Identifier("pram"), settings)
    {
        PhysicalMemoryUsed = new Sensor("Used", 0, SensorType.Data, this, settings);
        ActivateSensor(PhysicalMemoryUsed);

        PhysicalMemoryAvailable = new Sensor("Available", 1, SensorType.Data, this, settings);
        ActivateSensor(PhysicalMemoryAvailable);

        ActivateSensor(PhysicalMemoryTotal = new Sensor("Total", 2, SensorType.Data, this, settings));

        PhysicalMemoryLoad = new Sensor("Memory", 0, SensorType.Load, this, settings);
        ActivateSensor(PhysicalMemoryLoad);
    }

    public override HardwareType HardwareType => HardwareType.Memory;

    internal Sensor PhysicalMemoryTotal { get; }

    internal Sensor PhysicalMemoryAvailable { get; }

    internal Sensor PhysicalMemoryLoad { get; }

    internal Sensor PhysicalMemoryUsed { get; }

    public override void Update()
    {
        if (OSHelper.IsUnix)
        {
            MemoryLinux.Update(this);
        }
        else
        {
            MemoryWindows.Update(this);
        }
    }
}
