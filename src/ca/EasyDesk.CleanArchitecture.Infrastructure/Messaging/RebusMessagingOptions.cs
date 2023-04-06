using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Routing;
using EasyDesk.Commons.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Rebus.Config;
using Rebus.Serialization;
using Rebus.Serialization.Json;
using Rebus.Time;
using Rebus.Topic;
using Rebus.Workers.TplBased;
using System.Collections.Immutable;
using System.Reflection;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public class RebusMessagingOptions
{
    private Action<RebusEndpoint, RebusConfigurer>? _configureRebus;

    public OutboxOptions OutboxOptions { get; private set; } = new();

    public bool AutoSubscribe { get; set; } = true;

    public IImmutableSet<Type> KnownMessageTypes { get; private set; } = Set<Type>();

    public RebusMessagingOptions AddKnownMessageTypes(IEnumerable<Type> types)
    {
        KnownMessageTypes = KnownMessageTypes.Union(types);
        return this;
    }

    public RebusMessagingOptions AddKnownMessageTypesFromAssemblies(params Assembly[] assemblies)
    {
        var foundTypes = new AssemblyScanner()
            .FromAssemblies(assemblies)
            .NonAbstract()
            .SubtypesOrImplementationsOf<IMessage>()
            .FindTypes();

        return AddKnownMessageTypes(foundTypes);
    }

    public RebusMessagingOptions ConfigureRebus(Action<RebusEndpoint, RebusConfigurer> configurationAction)
    {
        _configureRebus += configurationAction;
        return this;
    }

    public RebusMessagingOptions ConfigureRebusOptions(Action<OptionsConfigurer> configurationAction) =>
        ConfigureRebus((_, c) => c.Options(configurationAction));

    public void Apply(IServiceProvider serviceProvider, RebusEndpoint endpoint, RebusConfigurer configurer)
    {
        var transport = serviceProvider.GetRequiredService<RebusTransportConfiguration>();

        configurer.Transport(t => transport(t, endpoint.InputQueueAddress));

        configurer.Routing(r =>
        {
            r.Register(_ => new OutgoingCommandRouter(endpoint));
        });

        configurer.Logging(l =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            l.MicrosoftExtensionsLogging(loggerFactory);
        });

        configurer.Serialization(s =>
        {
            var jsonSettings = serviceProvider.GetRequiredService<JsonSettingsConfigurator>();
            s.UseNewtonsoftJson(jsonSettings.CreateSettings());
        });

        configurer.Options(o =>
        {
            o.UseTplToReceiveMessages();
            var clock = serviceProvider.GetRequiredService<IClock>();
            o.Register<IRebusTime>(_ => new NodaTimeRebusClock(clock));
            o.Register(_ => new KnownTypesConvention(KnownMessageTypes));
            o.Register<ITopicNameConvention>(c => c.Get<KnownTypesConvention>());
            o.Register<IMessageTypeNameConvention>(c => c.Get<KnownTypesConvention>());
        });

        _configureRebus?.Invoke(endpoint, configurer);
    }
}
