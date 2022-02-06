using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.Tools.Options;
using Rebus.Config;
using Rebus.Injection;
using Rebus.Routing;
using Rebus.Transport;
using System;
using System.Collections.Generic;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Messaging;

public class RebusMessagingOptions
{
    private Action<RebusConfigurer> _configureRebus = _ => { };
    private readonly ISet<Type> _knownMessageTypes = new HashSet<Type>();

    public RebusMessagingOptions()
    {
    }

    internal Option<OutboxOptions> OutboxOptions { get; private set; } = None;

    internal bool IsIdempotent { get; private set; } = false;

    internal IEnumerable<Type> KnownMessageTypes => _knownMessageTypes;

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

    public RebusMessagingOptions AddKnownMessageType<T>() where T : IMessage
    {
        _knownMessageTypes.Add(typeof(T));
        return this;
    }

    public RebusMessagingOptions AddKnownMessageTypesFromAssembliesOf(params Type[] assemblyMarkers)
    {
        _knownMessageTypes.UnionWith(MessageTypeScanning.FindMessageTypes(assemblyMarkers));
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
        IsIdempotent = true;
        return this;
    }

    internal void ApplyDefaultConfiguration(RebusConfigurer configurer)
    {
        _configureRebus(configurer);
    }
}
