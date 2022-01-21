using System;
using System.Linq;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Messaging.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker;
using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker.Idempotence;
using EasyDesk.CleanArchitecture.Application.Messaging.MessageBroker.Outbox;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Messaging.DependencyInjection;

public class MessagingOptions
{
    private readonly IServiceCollection _services;
    private readonly IMessageBrokerImplementation _messageBrokerImplementation;
    private readonly AppDescription _app;

    public MessagingOptions(IServiceCollection services, IMessageBrokerImplementation implementation, AppDescription app)
    {
        _services = services;
        _messageBrokerImplementation = implementation;
        _app = app;
    }

    public MessagingOptions AddSimplePublisher() => AddPublisher();

    public MessagingOptions AddOutboxPublisher()
    {
        _app.RequireModule<DataAccessModule>().Implementation.AddOutbox(_services, _app);

        _services.AddSingleton<IOutboxChannel, OutboxChannel>();
        _services.AddHostedService<PrimaryOutboxBackgroundService>();
        _services.AddHostedService<FallbackOutboxBackgroundService>();

        return AddPublisher(() =>
        {
            _services.Decorate<IMessagePublisher, OutboxPublisher>();
        });
    }

    private MessagingOptions AddPublisher(Action decorate = null)
    {
        _messageBrokerImplementation.AddMessagePublisher(_services);

        decorate?.Invoke();

        _services.AddScoped<IExternalEventPublisher>(provider => new ExternalEventPublisher(
            provider.GetRequiredService<IMessagePublisher>(),
            provider.GetRequiredService<ITimestampProvider>(),
            provider.GetRequiredService<IJsonSerializer>(),
            _app.IsMultitenant() ? provider.GetRequiredService<ITenantProvider>() : new NoTenant()));

        return this;
    }

    public MessagingOptions AddSimpleConsumer() => AddConsumer();

    public MessagingOptions AddIdempotentConsumer()
    {
        _app.RequireModule<DataAccessModule>().Implementation.AddIdempotenceManager(_services, _app);

        return AddConsumer(() =>
        {
            _services.Decorate<IMessageHandler, IdempotentMessageHandler>();
        });
    }

    private MessagingOptions AddConsumer(Action decorate = null)
    {
        _messageBrokerImplementation.AddMessageConsumer(_services);

        var eventTypes = EventTypeScanning.FindExternalEventTypes(_app.ApplicationAssemblyMarker);

        _services.AddSingleton(new MessageConsumerDefinition(eventTypes.Select(t => t.GetEventTypeName()).ToArray()));
        _services.AddScoped<IMessageHandler>(provider => new ExternalEventMessageHandler(
            provider.GetRequiredService<IExternalEventHandler>(),
            provider.GetRequiredService<IJsonSerializer>(),
            eventTypes));

        decorate?.Invoke();
        if (_app.IsMultitenant())
        {
            _services.Decorate<IMessageHandler, TenantAwareMessageHandler>();
        }
        _services.Decorate<IMessageHandler, TransactionalMessageHandler>();
        _services.Decorate<IMessageHandler, ErrorSafeMessageHandler>();

        _services.AddScoped<IExternalEventHandler, MediatorEventHandler>();

        return this;
    }
}
