using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Polling;

public sealed class PollingFailedException : Exception
{
    public PollingFailedException(int attempts, Duration timeout, Exception? innerException = null)
        : base($"Polling timed out. Attempted {attempts} polls in {timeout.TotalSeconds:0.000} seconds", innerException)
    {
    }
}
