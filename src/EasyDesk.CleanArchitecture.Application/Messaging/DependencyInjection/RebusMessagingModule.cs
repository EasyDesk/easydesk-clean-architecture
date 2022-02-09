using System;
using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.Outbox;
using EasyDesk.CleanArchitecture.Application.Messaging.Steps;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Config;
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

        services.AddRebus((configurer, provider) =>
        {
            configurer
                .Options(o => o.LogPipeline(verbose: true))
                .Logging(l => l.MicrosoftExtensionsLogging(provider.GetRequiredService<ILoggerFactory>()))
                .Options(o =>
                {
                    o.Decorate<ITopicNameConvention>(c => new TopicNameConvention());
                    o.Decorate<IMessageTypeNameConvention>(c => new KnownTypesConvention(_options.KnownMessageTypes));
                    o.WrapHandlersInsideTransaction();
                })
                .Serialization(s => s.UseNewtonsoftJson(JsonInteroperabilityMode.PureJson));
            _options.ApplyDefaultConfiguration(configurer);

            configurer.Options(o => o.Decorate(c => originalTransport = c.Get<ITransport>()));

            if (app.IsMultitenant())
            {
                configurer.Options(o => o.AddMultitenancySupport());
            }

            _options.OutboxOptions.IfPresent(_ =>
            {
                configurer.Options(t => t.Decorate<ITransport>(c => new TransportWithOutbox(c.Get<ITransport>())));
            });

            if (_options.UseIdempotentConsumer)
            {
                configurer.Options(o => o.HandleMessagesIdempotently());
            }

            configurer.Options(o => o.Decorate<IPipeline>(c =>
            {
                return new PipelineStepConcatenator(c.Get<IPipeline>())
                    .OnReceive(new ServiceScopeOpeningStep(), PipelineAbsolutePosition.Front)
                    .OnReceive(new DomainEventHandlingStep(), PipelineAbsolutePosition.Back);
            }));

            return configurer;
        });

        services.AutoRegisterHandlersFromAssembly(app.ApplicationAssemblyMarker.Assembly);

        services.AddScoped<MessageBroker>();

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
