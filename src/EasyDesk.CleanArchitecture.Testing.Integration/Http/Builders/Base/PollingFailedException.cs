using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public class PollingFailedException : Exception
{
    public PollingFailedException(int attempts, Duration timeout) : base($"Polling timed out. Attempted {attempts} polls in {timeout.TotalSeconds:0.000} seconds")
    {
    }
}
