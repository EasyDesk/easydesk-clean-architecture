using NodaTime;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public static class BackoffStrategies
{
    private static BackoffStrategy Infinite(Func<int, Duration> f) => x => Some(f(x));

    public static BackoffStrategy Exponential(Duration seed, double factor, Duration? offset = null) =>
        Infinite(x => seed * Math.Pow(factor, x) + (offset ?? Duration.Zero));

    public static BackoffStrategy Constant(Duration duration) =>
        Infinite(_ => duration);

    public static BackoffStrategy LimitedTo(this BackoffStrategy strategy, int max) =>
        x => strategy(x).Filter(_ => x < max);
}
