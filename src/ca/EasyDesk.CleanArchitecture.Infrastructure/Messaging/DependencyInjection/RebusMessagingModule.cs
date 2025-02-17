using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Dispatching.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.DomainServices;
using EasyDesk.CleanArchitecture.Application.Json.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Sagas.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Failures;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Inbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Threading;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy.DependencyInjection;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Reflection;
using EasyDesk.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Logging;
using Rebus.Retry;
using Rebus.Retry.PoisonQueues;
using Rebus.Retry.Simple;
using Rebus.Threading;
using Rebus.Transport;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;

public class RebusMessagingModule : AppModule
{
    private readonly RebusEndpoint _endpoint;
    private readonly RebusTransportConfiguration _transport;
    private readonly Action<RebusMessagingOptions>? _configure;

    public RebusMessagingModule(RebusEndpoint endpoint, RebusTransportConfiguration transport, Action<RebusMessagingOptions>? configure = null)
    {
        _endpoint = endpoint;
        _transport = transport;
        _configure = configure;
        Options = new RebusMessagingOptions();
        configure?.Invoke(Options);
    }

    public RebusMessagingOptions Options { get; }

    public override void BeforeServiceConfiguration(AppDescription app)
    {
        app.ConfigureDispatchingPipeline(pipeline =>
        {
            pipeline.AddStep(typeof(RebusServiceProviderStep<,>));
            if (Options.UseInbox)
            {
                pipeline
                    .AddStep(typeof(InboxStep<>))
                    .After(typeof(UnitOfWorkStep<,>));
            }
        });
        app.RequireModule<JsonModule>();

        var assembliesToScan = app.Assemblies.Append(typeof(SagasModule).Assembly);
        Options.AddKnownMessageTypesFromAssemblies(assembliesToScan);
    }

    public override void ConfigureServices(AppDescription app, IServiceCollection services, ContainerBuilder builder)
    {
        services.AddSingleton(_endpoint);
        services.AddSingleton(Options);
        services.AddSingleton(_transport);

        if (Options.UseOutbox || Options.UseInbox)
        {
            app.RequireModule<DataAccessModule>().Implementation.AddMessagingUtilities(services, app);
        }

        Options.FailuresOptions.RegisterFailureStrategies(services);

        ITransport? originalTransport = null;
        services.AddSingleton<PausableAsyncTaskFactory>();
        services.AddSingleton<IRebusPausableTaskPool>(provider => provider.GetRequiredService<PausableAsyncTaskFactory>());
        services.AddRebus((configurer, provider) =>
        {
            var options = provider.GetRequiredService<RebusMessagingOptions>();

            options.Apply(provider, _endpoint, configurer);
            configurer.Options(o =>
            {
                o.Decorate(c => originalTransport = c.Get<ITransport>());
                o.Register<IAsyncTaskFactory>(_ => provider.GetRequiredService<PausableAsyncTaskFactory>());
                if (options.UseOutbox)
                {
                    o.UseOutbox();
                }
                o.PatchAsyncDisposables();
                if (app.IsMultitenant())
                {
                    o.SetupForMultitenancy();
                }
                o.Decorate<IErrorHandler>(c =>
                {
                    _ = c.Get<ITransport>(); // Forces initialization of 'originalTransport'.
                    return new DeadletterQueueErrorHandler(c.Get<RetryStrategySettings>(), originalTransport, c.Get<IRebusLoggerFactory>());
                });
            });
            return configurer;
        });

        if (Options.UseOutbox)
        {
            AddOutboxServices(services, new(() => originalTransport!, isThreadSafe: true));
        }

        SetupMessageHandlers(services, Options.KnownMessageTypes, app);

        AddEventPropagators(services);
        AddCommandPropagators(services);
        services.AddScoped<MessageBroker>();
        services.AddScoped<IEventPublisher>(provider => provider.GetRequiredService<MessageBroker>());
        services.AddScoped<ICommandSender>(provider => provider.GetRequiredService<MessageBroker>());

        if (Options.AutoSubscribe)
        {
            services.AddHostedService<AutoSubscriptionService>();
        }
    }

    private void SetupMessageHandlers(IServiceCollection services, IEnumerable<Type> knownMessageTypes, AppDescription app)
    {
        knownMessageTypes
            .Where(x => x.IsSubtypeOrImplementationOf(typeof(IIncomingMessage)))
            .ForEach(t =>
            {
                var handlerInterfaceType = typeof(IHandleMessages<>).MakeGenericType(t);
                var handlerImplementationType = typeof(DispatchingMessageHandler<>).MakeGenericType(t);
                services.AddTransient(handlerInterfaceType, handlerImplementationType);

                var failedType = typeof(IFailed<>).MakeGenericType(t);
                var failedHandlerInterfaceType = typeof(IHandleMessages<>).MakeGenericType(failedType);
                var failedHandlerImplementationType = typeof(FailedMessageHandler<>).MakeGenericType(t);
                services.AddTransient(failedHandlerInterfaceType, failedHandlerImplementationType);
            });

        services.RegisterImplementationsAsTransient(typeof(IFailedMessageHandler<>), x => x.FromAssemblies(app.Assemblies));
    }

    private IEnumerable<Type> GetDispatchableReturnTypes(Type messageType)
    {
        return messageType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDispatchable<>))
            .Select(i => i.GetGenericArguments()[0]);
    }

    private void AddCommandPropagators(IServiceCollection services)
    {
        var arguments = new object[] { services };
        foreach (var messageType in Options.KnownMessageTypes)
        {
            messageType
                .GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IPropagatedCommand<,>))
                .Select(i => i.GetGenericArguments())
                .Select(a => typeof(RebusMessagingModule)
                    .GetMethod(nameof(RegisterCommandPropagatorForType), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(a))
                .ForEach(m => m.Invoke(this, arguments));
        }
    }

    private void RegisterCommandPropagatorForType<M, D>(IServiceCollection services)
        where M : IPropagatedCommand<M, D>, IOutgoingCommand
        where D : DomainEvent
    {
        services.AddTransient<IDomainEventHandler<D>, PropagateCommand<M, D>>();
    }

    private void AddEventPropagators(IServiceCollection services)
    {
        var arguments = new object[] { services };
        foreach (var messageType in Options.KnownMessageTypes)
        {
            messageType
                .GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IPropagatedEvent<,>))
                .Select(i => i.GetGenericArguments())
                .Select(a => typeof(RebusMessagingModule)
                    .GetMethod(nameof(RegisterEventPropagatorForType), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(a))
                .ForEach(m => m.Invoke(this, arguments));
        }
    }

    private void RegisterEventPropagatorForType<M, D>(IServiceCollection services)
        where M : IPropagatedEvent<M, D>, IOutgoingEvent
        where D : DomainEvent
    {
        services.AddTransient<IDomainEventHandler<D>, PropagateEvent<M, D>>();
    }

    private void AddOutboxServices(IServiceCollection services, Lazy<ITransport> originalTransport)
    {
        services.AddScoped<OutboxTransactionHelper>();

        if (Options.OutboxOptions.PeriodicTaskEnabled)
        {
            services.AddHostedService<PeriodicOutboxAwaker>(provider => new(
                Options.OutboxOptions.FlushingPeriod,
                provider.GetRequiredService<OutboxFlushRequestsChannel>()));
        }

        services.AddHostedService<OutboxConsumer>();
        services.AddSingleton<OutboxFlushRequestsChannel>();
        services.AddScoped(provider =>
        {
            // Required to force initialization of the bus, which in turn sets the 'originalTransport' variable.
            _ = provider.GetRequiredService<IBus>();

            return new OutboxFlusher(
                Options.OutboxOptions.FlushingStrategy,
                provider.GetRequiredService<IUnitOfWorkProvider>(),
                provider.GetRequiredService<IOutbox>(),
                originalTransport.Value);
        });
    }
}

public static class RebusMessagingModuleExtensions
{
    public static IAppBuilder AddRebusMessaging(this IAppBuilder builder, string inputQueueAddress, RebusTransportConfiguration transport, Action<RebusMessagingOptions>? configure = null)
    {
        return builder.AddModule(new RebusMessagingModule(new(inputQueueAddress), transport, configure));
    }

    public static bool HasRebusMessaging(this AppDescription app) => app.HasModule<RebusMessagingModule>();

    public static RebusMessagingModule RequireRebusMessaging(this AppDescription app) => app.RequireModule<RebusMessagingModule>();
}
