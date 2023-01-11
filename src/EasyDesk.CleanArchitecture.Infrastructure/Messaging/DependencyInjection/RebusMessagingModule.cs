using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Threading;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Threading;
using Rebus.Transport;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;

public class RebusMessagingModule : AppModule
{
    private readonly RebusEndpoint _endpoint;
    private readonly RebusTransportConfiguration _transport;
    private readonly Action<RebusMessagingOptions> _configure;

    public RebusMessagingModule(RebusEndpoint endpoint, RebusTransportConfiguration transport, Action<RebusMessagingOptions> configure = null)
    {
        _endpoint = endpoint;
        _transport = transport;
        _configure = configure;
    }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline.AddStep(typeof(RebusServiceProviderStep<,>));
            pipeline
                .AddStep(typeof(InboxStep<>))
                .After(typeof(UnitOfWorkStep<,>));
        });
        app.RequireModule<JsonModule>();
    }

    public override void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        var options = new RebusMessagingOptions();

        options.AddKnownMessageTypesFromAssemblies(
            app.GetLayerAssembly(CleanArchitectureLayer.Application),
            typeof(SagasModule).Assembly);

        _configure?.Invoke(options);

        services.AddSingleton(_endpoint);
        services.AddSingleton(options);
        services.AddSingleton(_transport);

        ITransport originalTransport = null;
        services.AddSingleton<PausableAsyncTaskFactory>();
        services.AddSingleton<IRebusPausableTaskPool>(provider =>
            provider.GetRequiredService<PausableAsyncTaskFactory>());
        services.AddRebus((configurer, provider) =>
        {
            options.Apply(provider, _endpoint, configurer);
            configurer.Options(o =>
            {
                o.Decorate(c => originalTransport = c.Get<ITransport>());
                o.Register<IAsyncTaskFactory>(_ => provider.GetRequiredService<PausableAsyncTaskFactory>());
                o.UseOutbox();
                if (app.IsMultitenant())
                {
                    o.SetupForMultitenancy();
                }
            });
            return configurer;
        });

        app.RequireModule<DataAccessModule>().Implementation.AddMessagingUtilities(services, app);
        SetupMessageHandlers(services, options.KnownMessageTypes);
        AddOutboxServices(services, new(() => originalTransport, isThreadSafe: true), options.OutboxOptions);

        AddEventPropagators(services, options.KnownMessageTypes);
        services.AddScoped<MessageBroker>();
        services.AddScoped<IEventPublisher>(provider => provider.GetRequiredService<MessageBroker>());
        services.AddScoped<ICommandSender>(provider => provider.GetRequiredService<MessageBroker>());

        if (options.AutoSubscribe)
        {
            services.AddHostedService<AutoSubscriptionService>();
        }
    }

    private void SetupMessageHandlers(IServiceCollection services, IEnumerable<Type> knownMessageTypes)
    {
        knownMessageTypes
            .Where(x => x.IsSubtypeOrImplementationOf(typeof(IIncomingMessage)))
            .ForEach(t =>
            {
                var interfaceType = typeof(IHandleMessages<>).MakeGenericType(t);
                var implementationType = typeof(DispatchingMessageHandler<>).MakeGenericType(t);
                services.AddTransient(interfaceType, implementationType);
            });
    }

    private IEnumerable<Type> GetDispatchableReturnTypes(Type messageType)
    {
        return messageType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDispatchable<>))
            .Select(i => i.GetGenericArguments()[0]);
    }

    private void AddEventPropagators(IServiceCollection services, IEnumerable<Type> knownMessageTypes)
    {
        var arguments = new object[] { services };
        foreach (var messageType in knownMessageTypes)
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

    private void AddOutboxServices(IServiceCollection services, Lazy<ITransport> originalTransport, OutboxOptions options)
    {
        services.AddScoped<OutboxTransactionHelper>();

        if (options.PeriodicTaskEnabled)
        {
            services.AddHostedService<PeriodicOutboxAwaker>(provider => new(
                options.FlushingPeriod,
                provider.GetRequiredService<OutboxFlushRequestsChannel>(),
                provider.GetRequiredService<ILogger<PeriodicOutboxAwaker>>()));
        }

        services.AddHostedService<OutboxFlusherBackgroundService>();
        services.AddSingleton<OutboxFlushRequestsChannel>();
        services.AddScoped(provider =>
        {
            // Required to force initialization of the bus, which in turn sets the 'originalTransport' variable.
            _ = provider.GetRequiredService<IBus>();

            return new OutboxFlusher(
                options.FlushingBatchSize,
                provider.GetRequiredService<IUnitOfWorkProvider>(),
                provider.GetRequiredService<IOutbox>(),
                originalTransport.Value);
        });
    }
}

public static class RebusMessagingModuleExtensions
{
    public static AppBuilder AddRebusMessaging(this AppBuilder builder, string inputQueueAddress, RebusTransportConfiguration transport, Action<RebusMessagingOptions> configure = null)
    {
        return builder.AddModule(new RebusMessagingModule(new(inputQueueAddress), transport, configure));
    }

    public static bool HasRebusMessaging(this AppDescription app) => app.HasModule<RebusMessagingModule>();

    public static RebusMessagingModule RequireRebusMessaging(this AppDescription app) => app.RequireModule<RebusMessagingModule>();
}
