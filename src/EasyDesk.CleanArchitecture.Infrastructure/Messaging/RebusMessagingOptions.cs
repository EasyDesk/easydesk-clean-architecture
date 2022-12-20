using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using Rebus.Config;
using Rebus.Injection;
using Rebus.Routing;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public class RebusMessagingOptions
{
    private Action<RebusEndpoint, RebusConfigurer> _configureRebus;

    public RebusMessagingOptions()
    {
    }

    public OutboxOptions OutboxOptions { get; private set; } = new();

    public bool AutoSubscribe { get; set; } = true;

    public RebusMessagingOptions ConfigureRebus(Action<RebusEndpoint, RebusConfigurer> configurationAction)
    {
        _configureRebus += configurationAction;
        return this;
    }

    public RebusMessagingOptions ConfigureTransport(Action<RebusEndpoint, StandardConfigurer<ITransport>> configurationAction) =>
        ConfigureRebus((e, c) => c.Transport(t => configurationAction(e, t)));

    public RebusMessagingOptions ConfigureRouting(Action<RebusEndpoint, StandardConfigurer<IRouter>> configurationAction) =>
        ConfigureRebus((e, c) => c.Routing(r => configurationAction(e, r)));

    public RebusMessagingOptions ConfigureRebusOptions(Action<OptionsConfigurer> configurationAction) =>
        ConfigureRebus((_, c) => c.Options(configurationAction));

    public RebusMessagingOptions DecorateRebusService<T>(Func<IResolutionContext, T> factory, string description = null) =>
        ConfigureRebusOptions(o => o.Decorate(factory, description));

    public void ApplyDefaultConfiguration(RebusEndpoint endpoint, RebusConfigurer configurer)
    {
        _configureRebus?.Invoke(endpoint, configurer);
    }
}
