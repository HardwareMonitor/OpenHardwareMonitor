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
    private readonly IconFactory _iconFactory;
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
            void SetIconKind(IconKind kind)
            {
                _iconKind = kind;
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

        return Sensor.SensorType switch
        {
            SensorType.Temperature => UnitManager.IsFahrenheitUsed ? $"{UnitManager.CelsiusToFahrenheit(Sensor.Value):F0}" : $"{Sensor.Value:F0}",
            SensorType.TimeSpan => $"{TimeSpan.FromSeconds(Sensor.Value.Value):g}",
            SensorType.Clock or SensorType.Fan or SensorType.Flow => $"{1e-3f * Sensor.Value:F1}",
            SensorType.Voltage or SensorType.Current or SensorType.SmallData or SensorType.Factor or SensorType.Conductivity => $"{Sensor.Value:F1}",
            SensorType.IntFactor => $"{Sensor.Value:F0}",
            SensorType.Throughput => GetThroughputValue(Sensor.Value ?? 0),
            SensorType.Control or SensorType.Frequency or SensorType.Level or SensorType.Load or SensorType.Noise or SensorType.Humidity => $"{Sensor.Value:F0}",
            SensorType.Energy or SensorType.Power or SensorType.Data => Sensor.Value.Value.ToTrayValue(),
            _ => "-",
        };
    }

    private static string GetThroughputValue(float byteCount, byte preferredSufNum = 1, bool addSuffix = false)
    {
        string[] suf = [" B", " KB", " MB", " GB"];
        if (byteCount == 0)
            return addSuffix ? "0" + suf[0] : "0";
        long bytes = Math.Abs((long)byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        if (place < preferredSufNum)
            place = preferredSufNum;
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        if (num >= 10)
            num = Math.Round(num, 0);
        string result = (Math.Sign(byteCount) * num).ToString();
        if (addSuffix)
            return result + suf[place];
        return new string(result.Take(3).ToArray());
    }

    public void Update()
    {
        Icon icon = _notifyIcon.Icon;
        _notifyIcon.Icon = Sensor.SensorType switch
        {
            SensorType.Load or SensorType.Control or SensorType.Level => _iconKind switch
            {
                IconKind.Percent => _iconFactory.CreatePercentageIcon(Sensor.Value.GetValueOrDefault()),
                IconKind.Pie => _iconFactory.CreatePercentagePieIcon((byte)Sensor.Value.GetValueOrDefault()),
                _ => _iconFactory.CreateTransparentIcon(GetString()),
            },
            _ => _iconFactory.CreateTransparentIcon(GetString()),
        };
        icon?.Destroy();
        string format = Sensor.SensorType switch
        {
            SensorType.Voltage => "\n{0}: {1:F2} V",
            SensorType.Current => "\n{0}: {1:F2} A",
            SensorType.Clock => "\n{0}: {1:F0} MHz",
            SensorType.Load => "\n{0}: {1:F1} %",
            SensorType.Temperature => "\n{0}: {1:F1} °C",
            SensorType.Fan => "\n{0}: {1:F0} RPM",
            SensorType.Flow => "\n{0}: {1:F0} L/h",
            SensorType.Control => "\n{0}: {1:F1} %",
            SensorType.Level => "\n{0}: {1:F1} %",
            SensorType.Power => "\n{0}: {1:F2} W",
            SensorType.Data => "\n{0}: {1:F2} GB",
            SensorType.Factor => "\n{0}: {1:F3}",
            SensorType.IntFactor => "\n{0}: {1:F0}",
            SensorType.Energy => "\n{0}: {0:F0} mWh",
            SensorType.Noise => "\n{0}: {0:F0} dBA",
            SensorType.Conductivity => "\n{0}: {0:F1} µS/cm",
            SensorType.Humidity => "\n{0}: {0:F0} %",
            _ => "\n{0}: {1}",
        };
        string formattedValue = Sensor.SensorType switch
        {
            SensorType.Temperature when UnitManager.IsFahrenheitUsed => string.Format("\n{0}: {1:F1} °F", Sensor.Name, UnitManager.CelsiusToFahrenheit(Sensor.Value)),
            SensorType.Throughput when !"Connection Speed".Equals(Sensor.Name) => $"\n{Sensor.Name}: {GetThroughputValue(Sensor.Value ?? 0, 0, true)}/s",
            _ => string.Format(format, Sensor.Name, Sensor.Value),
        };
        string hardwareName = Sensor.Hardware.Name;
        hardwareName = hardwareName.Substring(0, Math.Min(63 - formattedValue.Length, hardwareName.Length));
        string text = hardwareName + formattedValue;
        if (text.Length > 63)
            text = text.Substring(0, 63);

        _notifyIcon.Text = text;
        _notifyIcon.Visible = true;
    }
}
