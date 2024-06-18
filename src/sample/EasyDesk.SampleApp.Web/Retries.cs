using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using NodaTime;

namespace EasyDesk.SampleApp.Web;

public static class Retries
{
    public static BackoffStrategy BackoffStrategy { get; } = BackoffStrategies
        .Exponential(Duration.FromSeconds(5), 1.6)
        .LimitedTo(3);
}
