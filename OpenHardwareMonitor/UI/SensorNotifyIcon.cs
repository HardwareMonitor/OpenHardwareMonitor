using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.UI;

public class SensorNotifyIcon : IDisposable
{
    private enum IconKind
    {
        Regular,
        Percent,
        Pie,
    }

    private readonly NotifyIconAdv _notifyIcon;
    private IconFactory _iconFactory;
    private IconKind _iconKind;

    public SensorNotifyIcon(SystemTray sensorSystemTray, ISensor sensor, PersistentSettings settings)
    {
        Sensor = sensor;
        _notifyIcon = new NotifyIconAdv();
        _iconFactory = new IconFactory();
        _iconFactory.Color = settings.GetValue(new Identifier(sensor.Identifier, "traycolor").ToString(), _iconFactory.Color);
        if (Enum.TryParse(settings.GetValue(new Identifier(sensor.Identifier, "iconKind").ToString(), IconKind.Regular.ToString()), out IconKind iconKind))
            _iconKind = iconKind;
        else
        {
            _iconKind = sensor.SensorType.ValueIsPercent() ? IconKind.Percent : IconKind.Regular;
        }

        var contextMenuStrip = new ContextMenuStrip();
        contextMenuStrip.Renderer = new ThemedToolStripRenderer();
        var hideShowItem = new ToolStripMenuItem("Hide/Show");
        hideShowItem.Click += delegate
        {
            sensorSystemTray.SendHideShowCommand();
        };
        contextMenuStrip.Items.Add(hideShowItem);
        contextMenuStrip.Items.Add("-");
        var removeItem = new ToolStripMenuItem("Remove Sensor");
        removeItem.Click += delegate
        {
            sensorSystemTray.Remove(Sensor);
        };
        contextMenuStrip.Items.Add(removeItem);

        if (sensor.SensorType.ValueIsPercent())
        {
            var iconKindItem = new ToolStripMenuItem("Icon Kind");
            iconKindItem.DropDownItems.Add(new ToolStripMenuItem("Value", null, (_, _) => { SetIconKind(IconKind.Regular); }) { Checked = _iconKind == IconKind.Regular });
            iconKindItem.DropDownItems.Add(new ToolStripMenuItem("Percent", null, (_, _) => { SetIconKind(IconKind.Percent); }) { Checked = _iconKind == IconKind.Percent });
            iconKindItem.DropDownItems.Add(new ToolStripMenuItem("Pie", null, (_, _) => { SetIconKind(IconKind.Pie); }) { Checked = _iconKind == IconKind.Pie });
            void SetIconKind(IconKind iconKind)
            {
                _iconKind = iconKind;
                for (int i = 0; i < iconKindItem.DropDownItems.Count; i++)
                {
                    if (iconKindItem.DropDownItems[i] is not ToolStripMenuItem menuItem) continue;
                    menuItem.Checked = (int)_iconKind == i;
                }
                settings.SetValue(new Identifier(sensor.Identifier, "iconKind").ToString(), (int)_iconKind);
                Update();
            }
            contextMenuStrip.Items.Add(iconKindItem);
        }

        var colorItem = new ToolStripMenuItem("Change Color...");
        colorItem.Click += delegate
        {
            using (ColorDialog dialog = new ColorDialog { Color = _iconFactory.Color })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;
                _iconFactory.Color = dialog.Color;
                settings.SetValue(new Identifier(sensor.Identifier, "traycolor").ToString(), _iconFactory.Color);
            }
        };
        contextMenuStrip.Items.Add(colorItem);
        contextMenuStrip.Items.Add("-");
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += delegate
        {
            sensorSystemTray.SendExitCommand();
        };
        contextMenuStrip.Items.Add(exitItem);
        _notifyIcon.ContextMenuStrip = contextMenuStrip;
        _notifyIcon.DoubleClick += delegate
        {
            sensorSystemTray.SendHideShowCommand();
        };
    }

    public ISensor Sensor { get; }

    public void Dispose()
    {
        Icon icon = _notifyIcon.Icon;
        _notifyIcon.Icon = null;
        icon?.Destroy();
        _notifyIcon.Dispose();
        _iconFactory.Dispose();
    }

    private string GetString()
    {
        if (!Sensor.Value.HasValue)
            return "-";

        switch (Sensor.SensorType)
        {
            case SensorType.Temperature:
                return UnitManager.IsFahrenheitUsed ? $"{UnitManager.CelsiusToFahrenheit(Sensor.Value):F0}" : $"{Sensor.Value:F0}";
            case SensorType.TimeSpan:
                return $"{TimeSpan.FromSeconds(Sensor.Value.Value):g}";
            case SensorType.Clock:
            case SensorType.Fan:
            case SensorType.Flow:
                return $"{1e-3f * Sensor.Value:F1}";
            case SensorType.Voltage:
            case SensorType.Current:
            case SensorType.SmallData:
            case SensorType.Factor:
            case SensorType.Conductivity:
                return $"{Sensor.Value:F1}";
            case SensorType.IntFactor:
                return $"{Sensor.Value:F0}";
            case SensorType.Throughput:
                return GetThroughputValue(Sensor.Value ?? 0);
            case SensorType.Control:
            case SensorType.Frequency:
            case SensorType.Level:
            case SensorType.Load:
            case SensorType.Noise:
            case SensorType.Humidity:
                return $"{Sensor.Value:F0}";
            case SensorType.Energy:
            case SensorType.Power:
            case SensorType.Data:
                return Sensor.Value.Value.ToTrayValue();
            default:
                return "-";
        }
    }

    private static string GetThroughputValue(float byteCount, byte preferredSufNum = 1, bool addSuffix = false)
    {
        string[] suf = { " B", " KB", " MB", " GB" };
        if (byteCount == 0)
            return addSuffix ? "0" + suf[0] : "0";
        long bytes = Math.Abs((long)byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        if (place < preferredSufNum)
            place = preferredSufNum;
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        if (num >= 10)
            num = Math.Round(num, 0);
        var result = (Math.Sign(byteCount) * num).ToString();
        if (addSuffix)
            return result + suf[place];
        return new string(result.Take(3).ToArray());
    }

    public void Update()
    {
        Icon icon = _notifyIcon.Icon;
        switch (Sensor.SensorType)
        {
            case SensorType.Load:
            case SensorType.Control:
            case SensorType.Level:
                _notifyIcon.Icon = _iconKind switch
                {
                    IconKind.Percent => _iconFactory.CreatePercentageIcon(Sensor.Value.GetValueOrDefault()),
                    IconKind.Pie => _iconFactory.CreatePercentagePieIcon((byte)Sensor.Value.GetValueOrDefault()),
                    _ => _iconFactory.CreateTransparentIcon(GetString()),
                };
                break;
            default:
                _notifyIcon.Icon = _iconFactory.CreateTransparentIcon(GetString());
                break;
        }
        icon?.Destroy();

        string format;
        switch (Sensor.SensorType)
        {
            case SensorType.Voltage: format = "\n{0}: {1:F2} V"; break;
            case SensorType.Current: format = "\n{0}: {1:F2} A"; break;
            case SensorType.Clock: format = "\n{0}: {1:F0} MHz"; break;
            case SensorType.Load: format = "\n{0}: {1:F1} %"; break;
            case SensorType.Temperature: format = "\n{0}: {1:F1} °C"; break;
            case SensorType.Fan: format = "\n{0}: {1:F0} RPM"; break;
            case SensorType.Flow: format = "\n{0}: {1:F0} L/h"; break;
            case SensorType.Control: format = "\n{0}: {1:F1} %"; break;
            case SensorType.Level: format = "\n{0}: {1:F1} %"; break;
            case SensorType.Power: format = "\n{0}: {1:F2} W"; break;
            case SensorType.Data: format = "\n{0}: {1:F2} GB"; break;
            case SensorType.Factor: format = "\n{0}: {1:F3}"; break;
            case SensorType.IntFactor: format = "\n{0}: {1:F0}"; break;
            case SensorType.Energy: format = "\n{0}: {0:F0} mWh"; break;
            case SensorType.Noise: format = "\n{0}: {0:F0} dBA"; break;
            case SensorType.Conductivity: format = "\n{0}: {0:F1} µS/cm"; break;
            case SensorType.Humidity: format = "\n{0}: {0:F0} %"; break;
            default: format = "\n{0}: {1}"; break;
        }

        string formattedValue;
        if (Sensor.SensorType == SensorType.Temperature && UnitManager.IsFahrenheitUsed)
        {
            formattedValue = string.Format("\n{0}: {1:F1} °F", Sensor.Name, UnitManager.CelsiusToFahrenheit(Sensor.Value));
        }
        else if (Sensor.SensorType == SensorType.Throughput && !"Connection Speed".Equals(Sensor.Name))
        {
            formattedValue = $"\n{Sensor.Name}: {GetThroughputValue(Sensor.Value ?? 0, 0, true)}/s";
        }
        else
        {
            formattedValue = string.Format(format, Sensor.Name, Sensor.Value);
        }

        string hardwareName = Sensor.Hardware.Name;
        hardwareName = hardwareName.Substring(0, Math.Min(63 - formattedValue.Length, hardwareName.Length));
        string text = hardwareName + formattedValue;
        if (text.Length > 63)
            text = text.Substring(0, 63);

        _notifyIcon.Text = text;
        _notifyIcon.Visible = true;
    }
}
