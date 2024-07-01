using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;

public class FailuresOptions
{
    public static BackoffStrategy DefaultBackoffStrategy { get; } = BackoffStrategies.Exponential(Duration.FromSeconds(5), 2).LimitedTo(5);

    private Action<IServiceCollection>? _configureServices;

    public FailuresOptions AddFailureStrategy(Func<IServiceProvider, IFailureStrategy> factory)
    {
        _configureServices += services => services.AddTransient(factory);
        return this;
    }

    public FailuresOptions AddScheduledRetries(BackoffStrategy? backoffStrategy) =>
        AddFailureStrategy(sp => new ScheduledRetriesStrategy(
            sp.GetRequiredService<IBus>(),
            backoffStrategy ?? DefaultBackoffStrategy));

    public FailuresOptions AddDispatchAsFailure() =>
        AddFailureStrategy(sp => new DispatchAsFailureStrategy(
            sp.GetRequiredService<IServiceScopeFactory>()));

    public FailuresOptions ClearFailureStrategies()
    {
        _configureServices = null;
        return this;
    }

    public void RegisterFailureStrategies(IServiceCollection services)
    {
        _configureServices?.Invoke(services);
    }
}
