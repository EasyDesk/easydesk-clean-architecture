using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Routing;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Rebus.Config;
using Rebus.Retry.FailFast;
using Rebus.Retry.Simple;
using Rebus.Timeouts;
using Rebus.Workers.TplBased;
using System.Reflection;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public sealed class RebusMessagingOptions
{
    private Action<RebusEndpoint, RebusConfigurer>? _configureRebus;

    public IList<Func<Exception, Option<bool>>> FailFastCheckers { get; } = [];

    public OutboxOptions OutboxOptions { get; } = new();

    public FailuresOptions FailuresOptions { get; } = new();

    public string ErrorQueueName { get; set; } = RetryStrategySettings.DefaultErrorQueueName;

    public bool AutoSubscribe { get; set; } = true;

    public bool UseOutbox { get; set; } = true;

    public bool UseInbox { get; set; } = true;

    public IFixedSet<Type> KnownMessageTypes { get; private set; } = Set<Type>();

    public bool DeferredMessagesEnabled { get; private set; } = false;

    public RebusMessagingOptions AddKnownMessageTypes(IEnumerable<Type> types)
    {
        KnownMessageTypes = KnownMessageTypes.Union(types);
        return this;
    }

    public RebusMessagingOptions AddKnownMessageTypesFromAssemblies(IEnumerable<Assembly> assemblies)
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

    public RebusMessagingOptions ConfigureRebus(Action<RebusConfigurer> configurationAction) =>
        ConfigureRebus((_, c) => configurationAction(c));

    public RebusMessagingOptions ConfigureRebusOptions(Action<OptionsConfigurer> configurationAction) =>
        ConfigureRebus(c => c.Options(configurationAction));

    public RebusMessagingOptions EnableDeferredMessages(Action<StandardConfigurer<ITimeoutManager>> configurationAction)
    {
        DeferredMessagesEnabled = true;
        return ConfigureRebus(c => c.Timeouts(configurationAction));
    }

    public void Apply(IServiceProvider serviceProvider, RebusEndpoint endpoint, RebusConfigurer configurer)
    {
        var transport = serviceProvider.GetRequiredService<RebusTransportConfiguration>();

        configurer.Transport(t => transport(t, endpoint.InputQueueAddress));

        configurer.Routing(r =>
        {
            r.Register(_ => new OutgoingCommandRouter(endpoint, ErrorQueueName));
        });

        configurer.Logging(l =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            l.MicrosoftExtensionsLogging(loggerFactory);
        });

        configurer
            .UseJsonOptions(serviceProvider.GetRequiredService<JsonOptionsConfigurator>().CreateOptions())
            .UseNodaTimeClock(serviceProvider.GetRequiredService<IClock>())
            .UseKnownMessageTypesConventions(KnownMessageTypes)
            .Options(o =>
            {
                o.UseTplToReceiveMessages();
                o.RetryStrategy(errorQueueName: ErrorQueueName, secondLevelRetriesEnabled: true, maxDeliveryAttempts: FailuresOptions.MaxDeliveryAttempts);
                o.Decorate<IFailFastChecker>(c => new FailFastCheckerWrapper(c.Get<IFailFastChecker>(), FailFastCheckers));
            });

        _configureRebus?.Invoke(endpoint, configurer);
    }
}
