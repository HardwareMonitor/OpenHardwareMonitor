namespace OpenHardwareMonitor.Hardware.Memory;

internal sealed class VirtualMemory : Hardware
{
    public VirtualMemory(ISettings settings)
        : base("Virtual Memory", new Identifier("vram"), settings)
    {
        VirtualMemoryUsed = new Sensor("Used", 2, SensorType.Data, this, settings);
        ActivateSensor(VirtualMemoryUsed);

        VirtualMemoryAvailable = new Sensor("Available", 3, SensorType.Data, this, settings);
        ActivateSensor(VirtualMemoryAvailable);

        VirtualMemoryLoad = new Sensor("Memory", 1, SensorType.Load, this, settings);
        ActivateSensor(VirtualMemoryLoad);


        ActivateSensor(VirtualMemoryTotal = new Sensor("Total", 4, SensorType.Data, this, settings));
        ActivateSensor(KernelSize = new Sensor("Kernel usage", 5, SensorType.Data, this, settings));
        ActivateSensor(ProcessCount = new Sensor("Processes", 0, SensorType.IntFactor, this, settings));
        ActivateSensor(ThreadCount = new Sensor("Threads", 1, SensorType.IntFactor, this, settings));
        ActivateSensor(HandleCount = new Sensor("Handles", 2, SensorType.IntFactor, this, settings));
    }

    public override HardwareType HardwareType => HardwareType.Memory;

    internal Sensor VirtualMemoryAvailable { get; }

    internal Sensor VirtualMemoryLoad { get; }

    internal Sensor VirtualMemoryUsed { get; }

    internal readonly Sensor VirtualMemoryTotal;
    internal readonly Sensor KernelSize;
    internal readonly Sensor ProcessCount;
    internal readonly Sensor ThreadCount;
    internal readonly Sensor HandleCount;

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
