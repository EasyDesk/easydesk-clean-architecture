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
using Microsoft.Extensions.Hosting;
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

    public RebusMessagingModule(RebusEndpoint endpoint, RebusTransportConfiguration transport, RebusMessagingOptions options)
    {
        _endpoint = endpoint;
        _transport = transport;
        Options = options;
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
            pipeline.AddStepAfterAll(typeof(OutboxStoreMessagesStep<,>))
                .Before(typeof(SaveChangesStep<,>))
                .Before(typeof(DomainEventHandlingStep<,>));
            pipeline.AddStep(typeof(OutboxFlushRequestStep<,>))
                .Before(typeof(UnitOfWorkStep<,>));
        });
        app.RequireModule<JsonModule>();

        var assembliesToScan = app.Assemblies.Append(typeof(SagasModule).Assembly);
        Options.AddKnownMessageTypesFromAssemblies(assembliesToScan);
    }

    protected override void ConfigureRegistry(AppDescription app, ServiceRegistry registry)
    {
        if (Options.UseOutbox || Options.UseInbox)
        {
            app.RequireModule<DataAccessModule>().Implementation.AddMessagingUtilities(registry, app);
        }
    }

    protected override void ConfigureContainer(AppDescription app, ContainerBuilder builder)
    {
        builder.RegisterInstance(_endpoint)
            .SingleInstance();

        builder.RegisterInstance(Options)
            .SingleInstance();

        builder.RegisterInstance(_transport)
            .SingleInstance();

        Options.FailuresOptions.RegisterFailureStrategies(builder);

        builder.RegisterType<PausableAsyncTaskFactory>()
            .AsSelf()
            .As<IRebusPausableTaskPool>()
            .SingleInstance();

        ITransport? originalTransport = null;
        builder.RegisterRebus((configurer, context) =>
        {
            var options = context.Resolve<RebusMessagingOptions>();

            options.Apply(context, _endpoint, configurer);
            configurer.Options(o =>
            {
                o.Decorate(c => originalTransport = c.Get<ITransport>());
                o.Register<IAsyncTaskFactory>(_ => context.Resolve<PausableAsyncTaskFactory>());
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
            AddOutboxServices(builder, new(() => originalTransport!, isThreadSafe: true));
        }

        SetupMessageHandlers(builder, Options.KnownMessageTypes, app);

        AddEventPropagators(builder);
        AddCommandPropagators(builder);
        builder.RegisterType<MessageBroker>()
            .As<IEventPublisher>()
            .As<ICommandSender>()
            .InstancePerLifetimeScope();

        if (Options.AutoSubscribe)
        {
            builder.RegisterType<AutoSubscriptionService>()
                .As<IHostedService>()
                .SingleInstance();
        }
    }

    private void SetupMessageHandlers(ContainerBuilder builder, IEnumerable<Type> knownMessageTypes, AppDescription app)
    {
        knownMessageTypes
            .Where(x => x.IsSubtypeOrImplementationOf(typeof(IIncomingMessage)))
            .ForEach(t =>
            {
                var handlerInterfaceType = typeof(IHandleMessages<>).MakeGenericType(t);
                var handlerImplementationType = typeof(DispatchingMessageHandler<>).MakeGenericType(t);
                builder.RegisterType(handlerImplementationType)
                    .As(handlerInterfaceType)
                    .InstancePerDependency();

                var failedType = typeof(IFailed<>).MakeGenericType(t);
                var failedHandlerInterfaceType = typeof(IHandleMessages<>).MakeGenericType(failedType);
                var failedHandlerImplementationType = typeof(FailedMessageHandler<>).MakeGenericType(t);
                builder.RegisterType(failedHandlerImplementationType)
                    .As(failedHandlerInterfaceType)
                    .InstancePerDependency();
            });

        builder
            .RegisterAssemblyTypes([.. app.Assemblies])
            .AssignableToOpenGenericType(typeof(IFailedMessageHandler<>))
            .AsClosedTypesOf(typeof(IFailedMessageHandler<>))
            .InstancePerDependency();
    }

    private IEnumerable<Type> GetDispatchableReturnTypes(Type messageType)
    {
        return messageType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDispatchable<>))
            .Select(i => i.GetGenericArguments()[0]);
    }

    private void AddCommandPropagators(ContainerBuilder builder)
    {
        var arguments = new object[] { builder };
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

    private void RegisterCommandPropagatorForType<M, D>(ContainerBuilder builder)
        where M : IPropagatedCommand<M, D>, IOutgoingCommand
        where D : DomainEvent
    {
        builder.RegisterType<PropagateCommand<M, D>>()
            .As<IDomainEventHandler<D>>()
            .InstancePerDependency();
    }

    private void AddEventPropagators(ContainerBuilder builder)
    {
        var arguments = new object[] { builder };
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

    private void RegisterEventPropagatorForType<M, D>(ContainerBuilder builder)
        where M : IPropagatedEvent<M, D>, IOutgoingEvent
        where D : DomainEvent
    {
        builder.RegisterType<PropagateEvent<M, D>>()
            .As<IDomainEventHandler<D>>()
            .InstancePerDependency();
    }

    private void AddOutboxServices(ContainerBuilder builder, Lazy<ITransport> originalTransport)
    {
        builder.RegisterType<OutboxTransactionHelper>()
            .InstancePerLifetimeScope();

        if (Options.OutboxOptions.PeriodicTaskEnabled)
        {
            builder
                .Register(c => new PeriodicOutboxAwaker(
                    Options.OutboxOptions.FlushingPeriod,
                    c.Resolve<OutboxFlushRequestsChannel>()))
                .As<IHostedService>()
                .SingleInstance();
        }

        builder.RegisterType<OutboxConsumer>()
            .As<IHostedService>()
            .SingleInstance();

        builder.RegisterType<OutboxFlushRequestsChannel>()
            .SingleInstance();

        builder
            .Register(c =>
            {
                // Required to force initialization of the bus, which in turn sets the 'originalTransport' variable.
                _ = c.Resolve<IBus>();

                return new OutboxFlusher(
                    Options.OutboxOptions.FlushingStrategy,
                    c.Resolve<IUnitOfWorkManager>(),
                    c.Resolve<IOutbox>(),
                    originalTransport.Value);
            })
            .InstancePerLifetimeScope();
    }
}

public static class RebusMessagingModuleExtensions
{
    public static IAppBuilder AddRebusMessaging(this IAppBuilder builder, string inputQueueAddress, RebusTransportConfiguration transport, Action<RebusMessagingOptions>? configure = null)
    {
        var options = new RebusMessagingOptions();
        configure?.Invoke(options);
        return builder.AddModule(new RebusMessagingModule(new(inputQueueAddress), transport, options));
    }

    public static bool HasRebusMessaging(this AppDescription app) => app.HasModule<RebusMessagingModule>();

    public static RebusMessagingModule RequireRebusMessaging(this AppDescription app) => app.RequireModule<RebusMessagingModule>();
}
