using System;
using System.Linq;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Application.Messaging.Steps;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Multitenancy.DependencyInjection;
using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Pipeline;
using Rebus.Serialization;
using Rebus.Serialization.Json;
using Rebus.Topic;
using Rebus.Transport;

namespace EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;

public class RebusMessagingModule : IAppModule
{
    private readonly RebusMessagingOptions _options;

    public RebusMessagingModule(RebusMessagingOptions options)
    {
        _options = options;
    }

    public void ConfigureServices(IServiceCollection services, AppDescription app)
    {
        ITransport originalTransport = null;

        var knownMessageTypes = KnownMessageTypes.ScanAssemblies(app.ApplicationAssemblyMarker);
        services.AddSingleton(knownMessageTypes);

        services.AddRebus((configurer, provider) =>
        {
            configurer
                .Logging(l => l.MicrosoftExtensionsLogging(provider.GetRequiredService<ILoggerFactory>()))
                .Serialization(s => s.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson))
                .Options(o =>
                {
                    o.Decorate<ITopicNameConvention>(c => new TopicNameConvention());
                    o.Decorate<IMessageTypeNameConvention>(c => new KnownTypesConvention(knownMessageTypes));
                    o.WrapHandlersInsideTransaction();
                });

            _options.ApplyDefaultConfiguration(configurer);

            configurer.Options(o =>
            {
                o.Decorate(c => originalTransport = c.Get<ITransport>());

                if (app.IsMultitenant())
                {
                    o.AddMultitenancySupport();
                }

                _options.OutboxOptions.IfPresent(_ =>
                {
                    o.Decorate<ITransport>(c => new TransportWithOutbox(c.Get<ITransport>()));
                });

                if (_options.UseIdempotentConsumer)
                {
                    o.HandleMessagesIdempotently();
                }

                o.HandleDomainEventsAfterMessageHandlers();

                o.Decorate<IPipeline>(c =>
                {
                    return new PipelineStepConcatenator(c.Get<IPipeline>())
                        .OnReceive(new ServiceScopeOpeningStep(), PipelineAbsolutePosition.Front);
                });
            });

            return configurer;
        });

        ReflectionUtils.InstantiableSubtypesOfGenericInterface(typeof(IMessageHandler<>), app.ApplicationAssemblyMarker).ForEach(x =>
        {
            services.AddTransient(x.Interface, x.Implementation);
            var messageType = x.Interface.GetGenericArguments().First();
            var rebusHandlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);
            var adapterType = typeof(MessageHandlerAdapter<>).MakeGenericType(messageType);
            services.AddTransient(rebusHandlerType, adapterType);
        });

        services.AddScoped<MessageBroker>();
        services.AddScoped<IMessagePublisher>(provider => provider.GetRequiredService<MessageBroker>());
        services.AddScoped<IMessageSender>(provider => provider.GetRequiredService<MessageBroker>());

        if (_options.AutoSubscribe)
        {
            services.AddHostedService<AutoSubscriptionService>();
        }

        _options.OutboxOptions.IfPresent(outboxOptions =>
        {
            app.RequireModule<DataAccessModule>().Implementation.AddOutbox(services, app);

            services.AddScoped<OutboxTransactionHelper>();
            services.AddHostedService<PeriodicOutboxAwaker>(provider => new(
                outboxOptions.FlushingPeriod,
                provider.GetRequiredService<OutboxFlushRequestsChannel>(),
                provider.GetRequiredService<ILogger<PeriodicOutboxAwaker>>()));

            services.AddHostedService<OutboxFlusherBackgroundService>();
            services.AddSingleton<OutboxFlushRequestsChannel>();
            services.AddScoped<OutboxFlusher>(provider =>
            {
                // Required to force initialization of the bus, which in turn sets the 'originalTransport' variable.
                _ = provider.GetRequiredService<IBus>();

                return new(
                    outboxOptions.FlushingBatchSize,
                    provider.GetRequiredService<IUnitOfWorkProvider>(),
                    provider.GetRequiredService<IOutbox>(),
                    originalTransport);
            });
        });

        if (_options.UseIdempotentConsumer)
        {
            app.RequireModule<DataAccessModule>().Implementation.AddIdempotenceManager(services, app);
        }
    }
}

public static class RebusMessagingModuleExtensions
{
    public static AppBuilder AddRebusMessaging(this AppBuilder builder, Action<RebusMessagingOptions> configure)
    {
        var options = new RebusMessagingOptions();
        configure(options);

        return builder.AddModule(new RebusMessagingModule(options));
    }

    public static bool HasRebusMessaging(this AppDescription app) => app.HasModule<RebusMessagingModule>();
}
