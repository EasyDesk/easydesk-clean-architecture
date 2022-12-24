using NodaTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public class OutboxOptions
{
    public Duration FlushingPeriod { get; set; } = Duration.FromMinutes(1);

    public int FlushingBatchSize { get; set; } = 10;

    public bool PeriodicTaskEnabled { get; set; } = true;
}
