using NodaTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;

public sealed class OutboxOptions
{
    public Duration FlushingPeriod { get; set; } = Duration.FromMinutes(1);

    public OutboxFlushingStrategy FlushingStrategy { get; set; } = new OutboxFlushingStrategy.Batched(1, 1000);

    public bool PeriodicTaskEnabled { get; set; } = true;
}

public abstract record OutboxFlushingStrategy
{
    private OutboxFlushingStrategy()
    {
    }

    public record AllAtOnce : OutboxFlushingStrategy;

    public record AllInBatches(int BatchSize) : OutboxFlushingStrategy;

    public record Batched(int Batches, int BatchSize) : OutboxFlushingStrategy;
}
