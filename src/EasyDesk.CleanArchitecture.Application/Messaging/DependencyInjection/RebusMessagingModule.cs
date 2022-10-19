using EasyDesk.CleanArchitecture.Application.Cqrs.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Application.Messaging.Steps;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Serialization;
using Rebus.Serialization.Json;
using Rebus.Time;
using Rebus.Topic;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;

public class RebusMessagingModule : AppModule
{
    private readonly RebusMessagingOptions _options;

    public RebusMessagingModule(RebusMessagingOptions options)
    {
        _options = options;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<CqrsModule>().Pipeline.AddStep(typeof(RebusTransactionScopeStep<,>));
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton(_options);

        var knownMessageTypes = ScanForKnownMessageTypes(app);
        services.AddSingleton(knownMessageTypes);

        ITransport originalTransport = null;
        services.AddRebus((configurer, provider) =>
        {
            _options.ApplyDefaultConfiguration(configurer);
            configurer.Logging(l => l.MicrosoftExtensionsLogging(provider.GetRequiredService<ILoggerFactory>()));
            configurer.Serialization(s => s.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson));
            configurer.Options(o =>
            {
                o.Decorate<IRebusTime>(_ => new NodaTimeRebusClock(provider.GetRequiredService<IClock>()));
                o.Decorate<ITopicNameConvention>(_ => new TopicNameConvention());
                o.Decorate<IMessageTypeNameConvention>(c => new KnownTypesConvention(knownMessageTypes));
                o.WrapHandlersInsideUnitOfWork();
                o.Decorate(c => originalTransport = c.Get<ITransport>());

                if (app.IsMultitenant())
                {
                    o.AddMultitenancySupport();
                }

                o.UseOutbox();
                o.UseInbox();
                o.HandleDomainEventsAfterMessageHandlers();
                o.OpenServiceScopeBeforeMessageHandlers();
            });
            return configurer;
        });

        RegisterMessageHandlers(services, app);

        services.AddScoped<MessageBroker>();
        services.AddScoped<IEventPublisher>(provider => provider.GetRequiredService<MessageBroker>());
        services.AddScoped<ICommandSender>(provider => provider.GetRequiredService<MessageBroker>());

        if (_options.AutoSubscribe)
        {
            services.AddHostedService<AutoSubscriptionService>();
        }

        app.RequireModule<DataAccessModule>().Implementation.AddMessagingUtilities(services, app);
        AddOutboxServices(services, new(() => originalTransport, isThreadSafe: true));
    }

    private void RegisterMessageHandlers(IServiceCollection services, AppDescription app)
    {
        new AssemblyScanner()
            .FromAssemblies(app.GetLayerAssembly(CleanArchitectureLayer.Application))
            .NonAbstract()
            .SubtypesOrImplementationsOf(typeof(IMessageHandler<>))
            .FindTypes()
            .ForEach(t =>
            {
                var implementedInterfaces = t.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
                implementedInterfaces.ForEach(i =>
                {
                    services.AddTransient(i, t);
                    var messageType = i.GetGenericArguments().First();
                    var rebusHandlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);
                    var adapterType = typeof(EventHandlerAdapter<>).MakeGenericType(messageType);
                    services.AddTransient(rebusHandlerType, adapterType);
                });
            });
    }

    private void AddOutboxServices(IServiceCollection services, Lazy<ITransport> originalTransport)
    {
        services.AddScoped<OutboxTransactionHelper>();
        services.AddHostedService<PeriodicOutboxAwaker>(provider => new(
            _options.OutboxOptions.FlushingPeriod,
            provider.GetRequiredService<OutboxFlushRequestsChannel>(),
            provider.GetRequiredService<ILogger<PeriodicOutboxAwaker>>()));

        services.AddHostedService<OutboxFlusherBackgroundService>();
        services.AddSingleton<OutboxFlushRequestsChannel>();
        services.AddScoped(provider =>
        {
            // Required to force initialization of the bus, which in turn sets the 'originalTransport' variable.
            _ = provider.GetRequiredService<IBus>();

            return new OutboxFlusher(
                _options.OutboxOptions.FlushingBatchSize,
                provider.GetRequiredService<IUnitOfWorkProvider>(),
                provider.GetRequiredService<IOutbox>(),
                originalTransport.Value);
        });
    }

    private KnownMessageTypes ScanForKnownMessageTypes(AppDescription app)
    {
        var knownMessageTypes = new AssemblyScanner()
            .FromAssemblies(app.GetLayerAssembly(CleanArchitectureLayer.Application))
            .NonAbstract()
            .SubtypesOrImplementationsOf<IMessage>()
            .FindTypes()
            .ToEquatableSet();
        return new(knownMessageTypes);
    }
}

public static class RebusMessagingModuleExtensions
{
    public static AppBuilder AddRebusMessaging(this AppBuilder builder, string inputQueueAddress, Action<RebusMessagingOptions> configure)
    {
        var options = new RebusMessagingOptions(inputQueueAddress);
        configure(options);

        return builder.AddModule(new RebusMessagingModule(options));
    }

    public static bool HasRebusMessaging(this AppDescription app) => app.HasModule<RebusMessagingModule>();

    public static RebusMessagingModule RequireRebusMessaging(this AppDescription app) => app.RequireModule<RebusMessagingModule>();
}
