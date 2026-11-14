using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.UI;

internal class SensorNode : Node
{
    private readonly PersistentSettings _settings;
    private Color? _penColor;
    private bool _plot;

    public SensorNode(ISensor sensor, PersistentSettings settings)
    {
        Sensor = sensor;
        _settings = settings;
        Format = SensorValueFormatter.GetFormat(sensor.SensorType);

        bool hidden = settings.GetValue(new Identifier(sensor.Identifier, "hidden").ToString(), sensor.IsDefaultHidden);
        base.IsVisible = !hidden;
        _plot = settings.GetValue(new Identifier(sensor.Identifier, "plot").ToString(), false);
        string id = new Identifier(sensor.Identifier, "penColor").ToString();

        if (settings.Contains(id))
            PenColor = settings.GetValue(id, Color.Black);
    }

    public event EventHandler PlotSelectionChanged;

    public string Format { get; }

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
        get { return SensorValueFormatter.Format(Sensor, Sensor.Max); }
    }

    public string Min
    {
        get { return SensorValueFormatter.Format(Sensor, Sensor.Min); }
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

            PlotSelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool Plot
    {
        get { return _plot; }
        set
        {
            _plot = value;
            _settings.SetValue(new Identifier(Sensor.Identifier, "plot").ToString(), value);
            PlotSelectionChanged?.Invoke(this, EventArgs.Empty);
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
        get { return SensorValueFormatter.FormatValue(Sensor); }
    }

    private void OptionallyAppendCriticalRange(StringBuilder str, float? min, float? max, string kind)
    {
        if (min.HasValue)
        {
            str.AppendLine(max.HasValue
                               ? $"{CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(kind)} range: {SensorValueFormatter.Format(Sensor, min)} to {SensorValueFormatter.Format(Sensor, max)}."
                               : $"Minimal {kind} value: {SensorValueFormatter.Format(Sensor, min)}.");
        }
        else if (max.HasValue)
        {
            str.AppendLine($"Maximal {kind} value: {SensorValueFormatter.Format(Sensor, max)}.");
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
