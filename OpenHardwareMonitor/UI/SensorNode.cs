using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.UI;

public class SensorNode : Node
{
    private readonly PersistentSettings _settings;
    private Color? _penColor;

    public SensorNode(ISensor sensor, PersistentSettings settings)
    {
        Sensor = sensor;
        _settings = settings;
        Format = sensor.SensorType switch
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
            _ => Format
        };

        bool hidden = settings.GetValue(new Identifier(sensor.Identifier, "hidden").ToString(), sensor.IsDefaultHidden);
        base.IsVisible = !hidden;
        string id = new Identifier(sensor.Identifier, "penColor").ToString();

        if (settings.Contains(id))
            PenColor = settings.GetValue(id, Color.Black);
    }

    public string Format { get; set; } = "";

    public override bool IsVisible
    {
        get { return base.IsVisible; }
        set
        {
            base.IsVisible = value;
            _settings.SetValue(new Identifier(Sensor.Identifier, "hidden").ToString(), !value);
        }
    }

    public string Max
    {
        get { return ValueToString(Sensor.Max); }
    }

    public string Min
    {
        get { return ValueToString(Sensor.Min); }
    }

    public Color? PenColor
    {
        get { return _penColor; }
        set
        {
            _penColor = value;

            string id = new Identifier(Sensor.Identifier, "penColor").ToString();
            if (value.HasValue)
                _settings.SetValue(id, value.Value);
            else
                _settings.Remove(id);
        }
    }

    public ISensor Sensor { get; }

    public override string Text
    {
        get { return Sensor.Name; }
        set { Sensor.Name = value; }
    }

    public override string ToolTip
    {
        get
        {
            StringBuilder stringBuilder = new();

            if (Sensor is ICriticalSensorLimits criticalSensorLimits)
                OptionallyAppendCriticalRange(stringBuilder, criticalSensorLimits.CriticalLowLimit, criticalSensorLimits.CriticalHighLimit, "critical");

            if (Sensor is ISensorLimits sensorLimits)
                OptionallyAppendCriticalRange(stringBuilder, sensorLimits.LowLimit, sensorLimits.HighLimit, "normal");

            return stringBuilder.ToString();
        }
    }

    public string Value
    {
        get { return ValueToString(Sensor.Value); }
    }

    public string ValueToString(float? value)
    {
        if (value.HasValue)
        {
            switch (Sensor.SensorType)
            {
                case SensorType.Temperature when UnitManager.IsFahrenheitUsed:
                    {
                        return $"{value * 1.8 + 32:0.#} °F";
                    }
                case SensorType.Throughput:
                    {
                        string result;
                        switch (Sensor.Name)
                        {
                            case "Connection Speed":
                                {
                                    switch (value)
                                    {
                                        case 100000000:
                                            {
                                                result = "100Mbps";
                                                break;
                                            }
                                        case 1000000000:
                                            {
                                                result = "1Gbps";
                                                break;
                                            }
                                        default:
                                            {
                                                if (value < 1024)
                                                    result = $"{value:F0} bps";
                                                else if (value < 1048576)
                                                    result = $"{value / 1024:0.#} Kbps";
                                                else if (value < 1073741824)
                                                    result = $"{value / 1048576:0.#} Mbps";
                                                else
                                                    result = $"{value / 1073741824:0.#} Gbps";
                                            }

                                            break;
                                    }

                                    break;
                                }
                            default:
                                {
                                    const int _1MB = 1048576;

                                    result = value < _1MB ? $"{value / 1024:0.#} KB/s" : $"{value / _1MB:0.#} MB/s";

                                    break;
                                }
                        }

                        return result;
                    }
                case SensorType.TimeSpan:
                    {
                        return value.HasValue ? string.Format(Format, TimeSpan.FromSeconds(value.Value)) : "-";
                    }
                default:
                    {
                        return string.Format(Format, value);
                    }
            }
        }

        return "-";
    }

    private void OptionallyAppendCriticalRange(StringBuilder str, float? min, float? max, string kind)
    {
        if (min.HasValue)
        {
            str.AppendLine(max.HasValue
                               ? $"{CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(kind)} range: {ValueToString(min)} to {ValueToString(max)}."
                               : $"Minimal {kind} value: {ValueToString(min)}.");
        }
        else if (max.HasValue)
        {
            str.AppendLine($"Maximal {kind} value: {ValueToString(max)}.");
        }
    }

    public override bool Equals(object obj)
    {
        return obj is SensorNode s && Sensor == s.Sensor;
    }

    public override int GetHashCode()
    {
        return Sensor.GetHashCode();
    }
}
