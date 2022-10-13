using NodaTime;
using Rebus.Time;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class NodaTimeRebusClock : IRebusTime
{
    private readonly IClock _clock;

    public NodaTimeRebusClock(IClock clock)
    {
        _clock = clock;
    }

    public DateTimeOffset Now => _clock.GetCurrentInstant().ToDateTimeOffset();
}
