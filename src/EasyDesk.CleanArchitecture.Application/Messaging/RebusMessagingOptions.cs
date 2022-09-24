using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using Rebus.Config;
using Rebus.Injection;
using Rebus.Routing;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class RebusMessagingOptions
{
    private Action<RebusConfigurer> _configureRebus;

    public OutboxOptions OutboxOptions { get; private set; } = new();

    public bool AutoSubscribe { get; set; } = true;

    public RebusMessagingOptions ConfigureRebus(Action<RebusConfigurer> configurationAction)
    {
        _configureRebus += configurationAction;
        return this;
    }

    public RebusMessagingOptions ConfigureTransport(Action<StandardConfigurer<ITransport>> configurationAction) =>
        ConfigureRebus(c => c.Transport(configurationAction));

    public RebusMessagingOptions ConfigureRouting(Action<StandardConfigurer<IRouter>> configurationAction) =>
        ConfigureRebus(c => c.Routing(configurationAction));

    public RebusMessagingOptions ConfigureRebusOptions(Action<OptionsConfigurer> configurationAction) =>
        ConfigureRebus(c => c.Options(configurationAction));

    public RebusMessagingOptions DecorateRebusService<T>(Func<IResolutionContext, T> factory, string description = null) =>
        ConfigureRebusOptions(o => o.Decorate(factory, description));

    internal void ApplyDefaultConfiguration(RebusConfigurer configurer)
    {
        _configureRebus?.Invoke(configurer);
    }
}
