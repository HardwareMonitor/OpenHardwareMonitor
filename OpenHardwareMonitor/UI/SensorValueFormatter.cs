using System;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.UI;

internal static class SensorValueFormatter
{
    private const float Megabyte = 1048576;

    public static string GetFormat(SensorType sensorType)
    {
        return sensorType switch
        {
            SensorType.Voltage => "{0:0.###} V",
            SensorType.Current => "{0:0.###} A",
            SensorType.Clock => "{0:0.#} MHz",
            SensorType.Load => "{0:0.#} %",
            SensorType.Temperature => "{0:0.#} °C",
            SensorType.Fan => "{0:F0} RPM",
            SensorType.Flow => "{0:0.#} L/h",
            SensorType.Control => "{0:0.#} %",
            SensorType.Level => "{0:0.#} %",
            SensorType.Power => "{0:0.#} W",
            SensorType.Data => "{0:0.#} GB",
            SensorType.SmallData => "{0:0.#} MB",
            SensorType.Factor => "{0:0.###}",
            SensorType.IntFactor => "{0:F0}",
            SensorType.Frequency => "{0:0.#} Hz",
            SensorType.Throughput => "{0:0.#} B/s",
            SensorType.TimeSpan => "{0:g}",
            SensorType.Timing => "{0:0.###} ns",
            SensorType.Energy => "{0:F0} mWh",
            SensorType.Noise => "{0:F0} dBA",
            SensorType.Conductivity => "{0:0.#} µS/cm",
            SensorType.Humidity => "{0:F0} %",
            _ => ""
        };
    }

    public static string GetTitle(SensorType sensorType)
    {
        return sensorType switch
        {
            SensorType.TimeSpan => "Time",
            SensorType.SmallData => "Data",
            SensorType.IntFactor => "Factor",
            SensorType.Energy => "Capacity",
            _ => sensorType.ToString()
        };
    }

    public static string FormatValue(ISensor sensor)
    {
        return sensor.SensorType == SensorType.Text ? sensor.Text ?? "-" : Format(sensor, sensor.Value);
    }

    public static string Format(ISensor sensor, float? value)
    {
        if (!value.HasValue)
            return "-";

        switch (sensor.SensorType)
        {
            case SensorType.Temperature when UnitManager.IsFahrenheitUsed:
                return $"{value * 1.8 + 32:0.#} °F";
            case SensorType.Throughput:
                return sensor.Name == "Connection Speed" ? FormatConnectionSpeed(value.Value) : FormatThroughput(value.Value);
            case SensorType.TimeSpan:
                return string.Format(GetFormat(sensor.SensorType), TimeSpan.FromSeconds(value.Value));
            default:
                return string.Format(GetFormat(sensor.SensorType), value);
        }
    }

    public static string FormatRange(SensorType sensorType, float min, float max)
    {
        if (sensorType == SensorType.Temperature && UnitManager.IsFahrenheitUsed)
        {
            min = min * 1.8f + 32;
            max = max * 1.8f + 32;
        }

        if (sensorType == SensorType.Throughput)
        {
            return max < Megabyte
                ? $"{min / 1024:0.#}–{max / 1024:0.#} KB/s"
                : $"{min / Megabyte:0.#}–{max / Megabyte:0.#} MB/s";
        }

        if (sensorType == SensorType.TimeSpan && IsFinite(min) && IsFinite(max))
            return $"{FormatTimeSpan(min)}–{FormatTimeSpan(max)}";

        return $"{min:0.#}–{max:0.#}";
    }

    private static string FormatConnectionSpeed(float value)
    {
        switch (value)
        {
            case 100000000:
                return "100Mbps";
            case 1000000000:
                return "1Gbps";
        }

        if (value < 1024)
            return $"{value:F0} bps";
        if (value < Megabyte)
            return $"{value / 1024:0.#} Kbps";
        if (value < 1073741824)
            return $"{value / Megabyte:0.#} Mbps";
        return $"{value / 1073741824:0.#} Gbps";
    }

    private static string FormatThroughput(float value)
    {
        return value < Megabyte ? $"{value / 1024:0.#} KB/s" : $"{value / Megabyte:0.#} MB/s";
    }

    private static string FormatTimeSpan(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Math.Abs(seconds));
        string text = time.TotalHours >= 1 ? $"{(int)time.TotalHours}:{time:mm\\:ss}" : $"{time:m\\:ss}";
        return seconds < 0 ? "-" + text : text;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
