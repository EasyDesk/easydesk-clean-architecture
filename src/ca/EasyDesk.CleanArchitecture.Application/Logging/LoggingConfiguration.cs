namespace EasyDesk.CleanArchitecture.Application.Logging;

public class LoggingConfiguration
{
    public bool RequestLoggingEnabled { get; private set; } = false;

    public bool ResultLoggingEnabled { get; private set; } = false;

    public LoggingConfiguration EnableRequestLogging(bool enabled = true)
    {
        RequestLoggingEnabled = enabled;
        return this;
    }

    public LoggingConfiguration EnableResultLogging(bool enabled = true)
    {
        ResultLoggingEnabled = enabled;
        return this;
    }
}
