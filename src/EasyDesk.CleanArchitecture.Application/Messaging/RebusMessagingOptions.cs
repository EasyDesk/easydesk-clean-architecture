using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.Tools.Options;
using Rebus.Config;
using Rebus.Injection;
using Rebus.Routing;
using Rebus.Transport;
using System;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class RebusMessagingOptions
{
    private Action<RebusConfigurer> _configureRebus = _ => { };

    internal Option<OutboxOptions> OutboxOptions { get; private set; } = None;

    internal bool UseIdempotentConsumer { get; private set; } = false;

    internal bool AutoSubscribe { get; private set; }

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

    public RebusMessagingOptions EnableAutoSubscribe(bool autoSubscribe = true)
    {
        AutoSubscribe = autoSubscribe;
        return this;
    }

    public RebusMessagingOptions UseOutbox(Action<OutboxOptions> configureOutbox = null)
    {
        var outboxOptions = OutboxOptions.OrElseGet(() => new OutboxOptions());
        configureOutbox?.Invoke(outboxOptions);
        OutboxOptions = Some(outboxOptions);
        return this;
    }

    public RebusMessagingOptions UseIdempotentHandling()
    {
        UseIdempotentConsumer = true;
        return this;
    }

    internal void ApplyDefaultConfiguration(RebusConfigurer configurer)
    {
        _configureRebus(configurer);
    }
}
