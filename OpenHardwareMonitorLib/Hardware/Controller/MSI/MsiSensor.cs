namespace OpenHardwareMonitor.Hardware.Controller.MSI;

internal delegate float GetMsiSensorValue(MsiFanControl msi);

internal class MsiSensor : Sensor
{
    private readonly GetMsiSensorValue _getValue;

    public MsiSensor(string name, int index, SensorType sensorType, Hardware hardware, ISettings settings, GetMsiSensorValue getValue)
        : base(name, index, sensorType, hardware, settings)
    {
        _getValue = getValue;
    }

    internal void Update(MsiFanControl msi)
    {
        float value = _getValue(msi);

        Value = value;
    }
}
