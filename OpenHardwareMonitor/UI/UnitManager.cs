namespace OpenHardwareMonitor.UI;

public static class UnitManager
{
    public static bool IsFahrenheitUsed { get; set; } = !OperatingSystemHelper.IsMetricSystemUsed;

    public static float? CelsiusToFahrenheit(float? valueInCelsius)
    {
        return valueInCelsius * 1.8f + 32;
    }
}
