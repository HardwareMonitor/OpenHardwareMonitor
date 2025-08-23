namespace OpenHardwareMonitor.UI;

public static class UnitManager
{
    public static bool IsFahrenheitUsed => !OperatingSystemHelper.IsMetricSystemUsed;

    public static float? CelsiusToFahrenheit(float? valueInCelsius)
    {
        return valueInCelsius * 1.8f + 32;
    }
}
