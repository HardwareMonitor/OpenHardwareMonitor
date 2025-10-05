using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RAMSPDToolkit.I2CSMBus;
using RAMSPDToolkit.SPD;
using RAMSPDToolkit.SPD.Enums;
using RAMSPDToolkit.SPD.Interop.Shared;
using RAMSPDToolkit.Windows.Driver;

namespace OpenHardwareMonitor.Hardware.Memory;

internal class MemoryGroup : IGroup, IHardwareChanged
{
    private static readonly object _lock = new();
    private List<Hardware> _hardware = [];

    private CancellationTokenSource _cancellationTokenSource;
    private Exception _lastException;
    private bool _disposed = false;

    public MemoryGroup(ISettings settings)
    {
        if (Ring0.IsOpen && (DriverManager.Driver is null || !DriverManager.Driver.IsOpen))
        {
            // Assign implementation of IDriver.
            DriverManager.Driver = new RAMSPDToolkitDriver(Ring0.KernelDriver);
            SMBusManager.UseWMI = false;
        }

        _hardware.Add(new VirtualMemory(settings));
        _hardware.Add(new TotalMemory(settings));

        if (DriverManager.Driver == null)
        {
            return;
        }

        StartRetryTask(settings);
    }

    public event HardwareEventHandler HardwareAdded;
    public event HardwareEventHandler HardwareRemoved;

    public IReadOnlyList<IHardware> Hardware => _hardware;

    public string GetReport()
    {
        StringBuilder report = new();
        report.AppendLine("Memory Report:");
        if (_lastException != null)
        {
            report.AppendLine($"Error while detecting memory: {_lastException.Message}");
        }

        foreach (Hardware hardware in _hardware)
        {
            report.AppendLine($"{hardware.Name} ({hardware.Identifier}):");
            report.AppendLine();
            foreach (ISensor sensor in hardware.Sensors)
            {
                report.AppendLine($"{sensor.Name}: {sensor.Value?.ToString() ?? "No value"}");
            }
        }

        return report.ToString();
    }

    public void Close()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        lock (_lock)
        {
            foreach (Hardware ram in _hardware)
                ram.Close();

            _hardware = [];
            _disposed = true;
        }
    }

    private bool TryAddDimms(ISettings settings)
    {
        try
        {
            lock (_lock)
            {
                if (_disposed)
                    return false;
                return AddDimms(settings);
            }
        }
        catch (Exception ex)
        {
            _lastException = ex;
            Debug.Assert(false, "Exception while detecting RAM: " + ex.Message);
        }
        return false;
    }

    private void StartRetryTask(ISettings settings)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            int retryRemaining = 5;
            while (!_cancellationTokenSource.IsCancellationRequested && --retryRemaining > 0)
            {
                if (TryAddDimms(settings))
                    break;
                await Task.Delay(TimeSpan.FromSeconds(2.5), _cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }, _cancellationTokenSource.Token);
    }

    private bool AddDimms(ISettings settings)
    {
        List<SPDAccessor> accessors = [];
        bool ramDetected = false;
        SMBusManager.DetectSMBuses();
        foreach (SMBusInterface smbus in SMBusManager.RegisteredSMBuses)
        {
            for (byte i = SPDConstants.SPD_BEGIN; i <= SPDConstants.SPD_END; ++i)
            {
                SPDDetector detector = new(smbus, i);
                if (detector.Accessor != null)
                {
                    accessors.Add(detector.Accessor);
                    ramDetected = true;
                }
            }
        }

        List<Hardware> additions = [];
        foreach (SPDAccessor ram in accessors)
        {
            //Default value
            string name = $"DIMM #{ram.Index}";

            //Check if we can switch to the correct page
            if (ram.ChangePage(PageData.ModulePartNumber))
                name = $"{ram.GetModuleManufacturerString()} - {ram.ModulePartNumber()} (#{ram.Index})";

            DimmMemory memory = new(ram, name, new Identifier("memory", "dimm", $"{ram.Index}"), settings);
            additions.Add(memory);
        }

        _hardware = [.. _hardware, .. additions];
        foreach (Hardware hardware in additions)
            HardwareAdded?.Invoke(hardware);

        return ramDetected;
    }
}
