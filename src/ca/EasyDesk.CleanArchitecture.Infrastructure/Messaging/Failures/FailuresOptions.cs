using Autofac;
using NodaTime;
using Rebus.Bus;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;

public class FailuresOptions
{
    public static BackoffStrategy DefaultBackoffStrategy { get; } = BackoffStrategies.Exponential(Duration.FromSeconds(5), 2).LimitedTo(5);

    private Action<ContainerBuilder>? _configureContainer;

    public int MaxDeliveryAttempts { get; set; } = 1;

    public FailuresOptions AddFailureStrategy(Func<IComponentContext, IFailureStrategy> factory)
    {
        _configureContainer += builder => builder.Register(factory).As<IFailureStrategy>().InstancePerDependency();
        return this;
    }

    public FailuresOptions AddScheduledRetries(BackoffStrategy? backoffStrategy) =>
        AddFailureStrategy(c => new ScheduledRetriesStrategy(
            c.Resolve<IBus>(),
            backoffStrategy ?? DefaultBackoffStrategy));

    public FailuresOptions AddDispatchAsFailure() =>
        AddFailureStrategy(c => new DispatchAsFailureStrategy(
            c.Resolve<ILifetimeScope>()));

    public FailuresOptions ClearFailureStrategies()
    {
        _configureContainer = null;
        return this;
    }

    public void RegisterFailureStrategies(ContainerBuilder builder)
    {
        _configureContainer?.Invoke(builder);
    }
}
