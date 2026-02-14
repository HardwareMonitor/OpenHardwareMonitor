using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenHardwareMonitor.Hardware;

internal class Sensor : ISensor
{
    private readonly string _defaultName;
    private readonly Hardware _hardware;
    private readonly ISettings _settings;
    private readonly bool _trackMinMax;
    private readonly List<SensorValue> _values = new();
    private int _count;
    private float? _currentValue;
    private string _name;
    private float _sum;
    private TimeSpan _valuesTimeWindow = TimeSpan.FromDays(1.0);

    public Sensor(string name, int index, SensorType sensorType, Hardware hardware, ISettings settings) :
        this(name, index, sensorType, hardware, null, settings)
    { }

    public Sensor(string name, int index, SensorType sensorType, Hardware hardware, ParameterDescription[] parameterDescriptions, ISettings settings) :
        this(name, index, false, sensorType, hardware, parameterDescriptions, settings)
    { }

    public Sensor
    (
        string name,
        int index,
        bool defaultHidden,
        SensorType sensorType,
        Hardware hardware,
        ParameterDescription[] parameterDescriptions,
        ISettings settings,
        bool disableHistory = false)
    {
        Index = index;
        IsDefaultHidden = defaultHidden;
        SensorType = sensorType;
        _hardware = hardware;
        Identifier = new Identifier(_hardware.Identifier, SensorType.ToString().ToLowerInvariant(), Index.ToString(CultureInfo.InvariantCulture));

        Parameter[] parameters = new Parameter[parameterDescriptions?.Length ?? 0];
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameterDescriptions != null)
                parameters[i] = new Parameter(parameterDescriptions[i], this, settings);
        }

        Parameters = parameters;

        _settings = settings;
        _defaultName = name;
        _name = settings.GetValue(new Identifier(Identifier, "name").ToString(), name);
        _trackMinMax = !disableHistory;
        if (disableHistory)
        {
            _valuesTimeWindow = TimeSpan.Zero;
        }
    }

    public IControl Control { get; internal set; }

    public IHardware Hardware
    {
        get { return _hardware; }
    }

    public Identifier Identifier { get; internal set; }

    public int Index { get; }

    public bool IsDefaultHidden { get; }

    public float? Max { get; private set; }

    public float? Min { get; private set; }

    public string Name
    {
        get { return _name; }
        set
        {
            if (_name == value) return;
            _name = !string.IsNullOrEmpty(value) ? value : _defaultName;

            _settings.SetValue(new Identifier(Identifier, "name").ToString(), _name);
        }
    }

    public IReadOnlyList<IParameter> Parameters { get; }

    public SensorType SensorType { get; }

    public virtual float? Value
    {
        get { return _currentValue; }
        set
        {
            if (_valuesTimeWindow != TimeSpan.Zero)
            {
                DateTime now = DateTime.UtcNow;
                while (_values.Count > 0 && now - _values[0].Time > _valuesTimeWindow)
                    _values.RemoveAt(0);

                if (value.HasValue)
                {
                    _sum += value.Value;
                    _count++;
                    if (_count == 4)
                    {
                        AppendValue(_sum / _count, now);
                        _sum = 0;
                        _count = 0;
                    }
                }
            }

            _currentValue = value;
            if (_trackMinMax)
            {
                if (value.HasValue && !float.IsNaN(value.Value) && !float.IsInfinity(value.Value))
                {
                    if (!Min.HasValue || Min > value)
                        Min = value;

                    if (!Max.HasValue || Max < value)
                        Max = value;
                }
            }
        }
    }

    public IEnumerable<SensorValue> Values
    {
        get { return _values; }
    }

    public TimeSpan ValuesTimeWindow
    {
        get { return _valuesTimeWindow; }
        set
        {
            _valuesTimeWindow = value;
            if (value == TimeSpan.Zero)
                _values.Clear();
        }
    }

    public void ResetMin()
    {
        Min = null;
    }

    public void ResetMax()
    {
        Max = null;
    }

    public void ClearValues()
    {
        _values.Clear();
    }

    public void Accept(IVisitor visitor)
    {
        if (visitor == null)
            throw new ArgumentNullException(nameof(visitor));

        visitor.VisitSensor(this);
    }

    public void Traverse(IVisitor visitor)
    {
        foreach (IParameter parameter in Parameters)
            parameter.Accept(visitor);
    }

    private void AppendValue(float value, DateTime time)
    {
        if (_values.Count >= 2 && _values[_values.Count - 1].Value == value && _values[_values.Count - 2].Value == value)
        {
            _values[_values.Count - 1] = new SensorValue(value, time);
            return;
        }

        _values.Add(new SensorValue(value, time));
    }
}
