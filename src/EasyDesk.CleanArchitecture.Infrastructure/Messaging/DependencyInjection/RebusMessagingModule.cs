using EasyDesk.CleanArchitecture.Application.Cqrs.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Messaging.Messages;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;

public class RebusMessagingModule : AppModule
{
    private readonly RebusEndpoint _endpoint;
    private readonly RebusMessagingOptions _options;

    public RebusMessagingModule(RebusEndpoint endpoint, RebusMessagingOptions options)
    {
        _endpoint = endpoint;
        _options = options;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.RequireModule<CqrsModule>().Pipeline.AddStep(typeof(RebusTransactionScopeStep<,>));
        app.RequireModule<JsonModule>();
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        services.AddSingleton(_endpoint);
        services.AddSingleton(_options);
        services.AddSingleton(ScanForKnownMessageTypes(app));

        ITransport originalTransport = null;
        services.AddRebus((configurer, provider) =>
        {
            configurer.ConfigureStandardBehavior(_endpoint, _options, provider);
            configurer.Options(o =>
            {
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
        var options = new RebusMessagingOptions();
        configure(options);

        return builder.AddModule(new RebusMessagingModule(new(inputQueueAddress), options));
    }

    public static bool HasRebusMessaging(this AppDescription app) => app.HasModule<RebusMessagingModule>();

    public static RebusMessagingModule RequireRebusMessaging(this AppDescription app) => app.RequireModule<RebusMessagingModule>();
}
