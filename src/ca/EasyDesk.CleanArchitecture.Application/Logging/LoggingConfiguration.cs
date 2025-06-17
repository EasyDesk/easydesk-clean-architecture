namespace EasyDesk.CleanArchitecture.Application.Logging;

public class LoggingConfiguration
{
    public bool RequestLoggingEnabled { get; private set; }

    public bool ResultLoggingEnabled { get; private set; }

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
