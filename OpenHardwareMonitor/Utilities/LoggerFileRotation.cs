namespace OpenHardwareMonitor.Utilities
{
    internal enum LoggerFileRotation
    {
        // Keep the same file for the entire record session
        PerSession = 0,

        // Create a new file every day
        Daily,
    }
}
