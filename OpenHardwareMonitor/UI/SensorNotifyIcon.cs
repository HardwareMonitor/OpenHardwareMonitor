using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.UI;

internal class SensorNotifyIcon : IDisposable
{
    private enum IconKind
    {
        Value,
        Bar,
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
        if (Enum.TryParse(settings.GetValue(new Identifier(sensor.Identifier, "iconKind").ToString(), IconKind.Value.ToString()), out IconKind iconKind))
            _iconKind = iconKind;
        else
        {
            _iconKind = sensor.SensorType.ValueIsPercent() ? IconKind.Bar : IconKind.Value;
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
            iconKindItem.DropDownItems.Add(new ToolStripMenuItem("Value", null, (_, _) => { SetIconKind(IconKind.Value); }) { Checked = _iconKind == IconKind.Value });
            iconKindItem.DropDownItems.Add(new ToolStripMenuItem("Bar", null, (_, _) => { SetIconKind(IconKind.Bar); }) { Checked = _iconKind == IconKind.Bar });
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
        if (Sensor.SensorType == SensorType.Text)
            return Sensor.Text ?? "-";

        if (!Sensor.Value.HasValue)
            return "-";

        return Sensor.SensorType switch
        {
            SensorType.Temperature => UnitManager.IsFahrenheitUsed ? $"{UnitManager.CelsiusToFahrenheit(Sensor.Value):F0}" : Sensor.Value.Value.ToTrayValue(),
            SensorType.TimeSpan => $"{TimeSpan.FromSeconds(Sensor.Value.Value):g}",
            SensorType.Timing => $"{Sensor.Value.Value:0.###}",
            SensorType.Clock or SensorType.Fan or SensorType.Flow => $"{1e-3f * Sensor.Value:0.#}",
            SensorType.Throughput => GetThroughputValue(Sensor.Value ?? 0),
            _ => Sensor.Value.Value.ToTrayValue(),
        };
    }

    private static string GetThroughputValue(float byteCount)
    {
        long bytes = Math.Abs((long)byteCount);
        if (bytes == 0)
            return "0";

        int place = Math.Max(1, Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024))));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        if (num >= 10)
            num = Math.Round(num, 0);
        return new string((Math.Sign(byteCount) * num).ToString().Take(3).ToArray());
    }

    public void Update()
    {
        Icon icon = _notifyIcon.Icon;
        _notifyIcon.Icon = Sensor.SensorType switch
        {
            SensorType.Load or SensorType.Control or SensorType.Level => _iconKind switch
            {
                IconKind.Bar => _iconFactory.CreatePercentageIcon(Sensor.Value.GetValueOrDefault()),
                IconKind.Pie => _iconFactory.CreatePercentagePieIcon((byte)Sensor.Value.GetValueOrDefault()),
                _ => _iconFactory.CreateTransparentIcon(GetString()),
            },
            _ => _iconFactory.CreateTransparentIcon(GetString()),
        };
        icon?.Destroy();
        string formattedValue = $"\n{Sensor.Name}: {SensorValueFormatter.FormatValue(Sensor)}";
        string hardwareName = Sensor.Hardware.Name;
        hardwareName = hardwareName.Substring(0, Math.Min(63 - formattedValue.Length, hardwareName.Length));
        string text = hardwareName + formattedValue;
        if (text.Length > 63)
            text = text.Substring(0, 63);

        _notifyIcon.Text = text;
        _notifyIcon.Visible = true;
    }
}
