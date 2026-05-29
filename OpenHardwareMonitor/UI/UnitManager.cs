namespace OpenHardwareMonitor.UI;

internal static class UnitManager
{
    public static bool IsFahrenheitUsed { get; set; } = !OSHelper.IsMetricSystemUsed;

    public static float? CelsiusToFahrenheit(float? valueInCelsius)
    {
        return valueInCelsius * 1.8f + 32;
    }
}
