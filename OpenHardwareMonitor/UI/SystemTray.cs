using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.UI;

public class SystemTray : IDisposable
{
    private readonly PersistentSettings _settings;
    private readonly List<SensorNotifyIcon> _sensorList = new();
    private bool _mainIconEnabled;
    private readonly NotifyIconAdv _mainIcon;

    public SystemTray(IComputer computer, PersistentSettings settings)
    {
        _settings = settings;
        computer.HardwareAdded += HardwareAdded;
        computer.HardwareRemoved += HardwareRemoved;

        _mainIcon = new NotifyIconAdv();

        var contextMenuStrip = new ContextMenuStrip();
        contextMenuStrip.Renderer = new ThemedToolStripRenderer();
        var hideShowItem = new ToolStripMenuItem("Hide/Show");
        hideShowItem.Click += delegate
        {
            SendHideShowCommand();
        };
        contextMenuStrip.Items.Add(hideShowItem);
        contextMenuStrip.Items.Add("-");
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += delegate
        {
            SendExitCommand();
        };
        contextMenuStrip.Items.Add(exitItem);
        _mainIcon.ContextMenuStrip = contextMenuStrip;
        _mainIcon.DoubleClick += delegate
        {
            SendHideShowCommand();
        };
        _mainIcon.Icon = EmbeddedResources.GetIcon("icon.ico");
        _mainIcon.Text = Updater.ApplicationTitle;
    }

    private void HardwareRemoved(IHardware hardware)
    {
        hardware.SensorAdded -= SensorAdded;
        hardware.SensorRemoved -= SensorRemoved;

        foreach (ISensor sensor in hardware.Sensors)
            SensorRemoved(sensor);

        foreach (IHardware subHardware in hardware.SubHardware)
            HardwareRemoved(subHardware);
    }

    private void HardwareAdded(IHardware hardware)
    {
        foreach (ISensor sensor in hardware.Sensors)
            SensorAdded(sensor);

        hardware.SensorAdded += SensorAdded;
        hardware.SensorRemoved += SensorRemoved;

        foreach (IHardware subHardware in hardware.SubHardware)
            HardwareAdded(subHardware);
    }

    private void SensorAdded(ISensor sensor)
    {
        if (_settings.GetValue(new Identifier(sensor.Identifier, "tray").ToString(), false))
            Add(sensor);
    }

    private void SensorRemoved(ISensor sensor)
    {
        Remove(sensor, false);
    }

    public void Dispose()
    {
        lock(_sensorList)
        foreach (SensorNotifyIcon icon in _sensorList)
            icon.Dispose();
        _mainIcon.Dispose();
    }

    public void Redraw()
    {
        lock(_sensorList)
        foreach (SensorNotifyIcon icon in _sensorList)
            icon.Update();
    }

    public bool Contains(ISensor sensor)
    {
        lock(_sensorList)
            return _sensorList.Any(x => x.Sensor.Identifier.ToString() == sensor.Identifier.ToString());
    }

    public bool Add(ISensor sensor)
    {
        if (Contains(sensor))
            return false;

        lock (_sensorList)
        _sensorList.Add(new SensorNotifyIcon(this, sensor, _settings));
        UpdateMainIconVisibility();
        _settings.SetValue(new Identifier(sensor.Identifier, "tray").ToString(), true);
        return true;
    }

    public void Remove(ISensor sensor)
    {
        Remove(sensor, true);
    }

    private void Remove(ISensor sensor, bool deleteConfig)
    {
        if (deleteConfig)
        {
            _settings.Remove(new Identifier(sensor.Identifier, "tray").ToString());
            _settings.Remove(new Identifier(sensor.Identifier, "traycolor").ToString());
        }

        lock (_sensorList)
        {
            var toRemove = _sensorList.Where(s => s.Sensor.Identifier.ToString() == sensor.Identifier.ToString())
                .ToArray();
            foreach (var instance in toRemove)
            {
                _sensorList.Remove(instance);
                UpdateMainIconVisibility();
                instance.Dispose();
            }
        }
    }

    public event EventHandler HideShowCommand;

    public void SendHideShowCommand()
    {
        HideShowCommand?.Invoke(this, null);
    }

    public event EventHandler ExitCommand;

    public void SendExitCommand()
    {
        ExitCommand?.Invoke(this, null);
    }

    private void UpdateMainIconVisibility()
    {
        if (_mainIconEnabled)
            lock (_sensorList)
            _mainIcon.Visible = _sensorList.Count == 0;
        else
            _mainIcon.Visible = false;
    }

    public bool IsMainIconEnabled
    {
        get { return _mainIconEnabled; }
        set
        {
            if (_mainIconEnabled != value)
            {
                _mainIconEnabled = value;
                UpdateMainIconVisibility();
            }
        }
    }
}
