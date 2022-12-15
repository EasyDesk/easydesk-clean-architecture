using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
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
using System.Reflection;

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
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline.AddStep(typeof(RebusTransactionScopeStep<,>));
            pipeline.AddStep(typeof(InboxStep<>)).After(typeof(UnitOfWorkStep<,>));
        });
        app.RequireModule<JsonModule>();
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var knownMessageTypes = ScanForKnownMessageTypes(app);
        services.AddSingleton(knownMessageTypes);

        services.AddSingleton(_endpoint);
        services.AddSingleton(_options);

        ITransport originalTransport = null;
        services.AddRebus((configurer, provider) =>
        {
            configurer.ConfigureStandardBehavior(_endpoint, _options, provider);
            configurer.Options(o =>
            {
                o.Decorate(c => originalTransport = c.Get<ITransport>());
                o.UseOutbox();
                o.OpenServiceScopeBeforeMessageHandlers();
                if (app.IsMultitenant())
                {
                    o.SetupForMultitenancy();
                }
            });
            return configurer;
        });

        app.RequireModule<DataAccessModule>().Implementation.AddMessagingUtilities(services, app);
        SetupMessageHandlers(services);
        AddOutboxServices(services, new(() => originalTransport, isThreadSafe: true));

        AddEventPropagators(services, knownMessageTypes);

        services.AddScoped<MessageBroker>();
        services.AddScoped<IEventPublisher>(provider => provider.GetRequiredService<MessageBroker>());
        services.AddScoped<ICommandSender>(provider => provider.GetRequiredService<MessageBroker>());

        if (_options.AutoSubscribe)
        {
            services.AddHostedService<AutoSubscriptionService>();
        }
    }

    private void SetupMessageHandlers(IServiceCollection services)
    {
        services.AddTransient(typeof(IHandleMessages<>), typeof(DispatchingMessageHandler<>));
    }

    private IEnumerable<Type> GetDispatchableReturnTypes(Type messageType)
    {
        return messageType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDispatchable<>))
            .Select(i => i.GetGenericArguments()[0]);
    }

    private void AddEventPropagators(IServiceCollection services, KnownMessageTypes knownMessageTypes)
    {
        var arguments = new object[] { services };
        foreach (var messageType in knownMessageTypes.Types)
        {
            messageType
                .GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IPropagatedEvent<,>))
                .Select(i => i.GetGenericArguments())
                .Select(a => typeof(RebusMessagingModule)
                    .GetMethod(nameof(RegisterPropagatorForType), BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(a))
                .ForEach(m => m.Invoke(this, arguments));
        }
    }

    private void RegisterPropagatorForType<M, D>(IServiceCollection services)
        where M : IPropagatedEvent<M, D>, IOutgoingEvent
        where D : DomainEvent
    {
        services.AddTransient<IDomainEventHandler<D>, DomainEventPropagator<M, D>>();
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
