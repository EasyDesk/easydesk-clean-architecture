using System;
using System.Linq;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox;
using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;

public class EventManagementOptions
{
    private readonly IServiceCollection _services;
    private readonly IEventBusImplementation _eventBusImplementation;
    private readonly AppDescription _app;

    public EventManagementOptions(IServiceCollection services, IEventBusImplementation implementation, AppDescription app)
    {
        _services = services;
        _eventBusImplementation = implementation;
        _app = app;
    }

    public EventManagementOptions AddSimplePublisher() => AddPublisher();

    public EventManagementOptions AddOutboxPublisher()
    {
        _app.RequireModule<DataAccessModule>().Implementation.AddOutbox(_services, _app);

        _services.AddSingleton<IOutboxChannel, OutboxChannel>();
        _services.AddHostedService<PrimaryOutboxBackgroundService>();
        _services.AddHostedService<FallbackOutboxBackgroundService>();

        return AddPublisher(() =>
        {
            _services.Decorate<IEventBusPublisher, OutboxPublisher>();
        });
    }

    private EventManagementOptions AddPublisher(Action decorate = null)
    {
        _eventBusImplementation.AddEventBusPublisher(_services);

        decorate?.Invoke();

        _services.AddScoped<IExternalEventPublisher>(provider => new ExternalEventPublisher(
            provider.GetRequiredService<IEventBusPublisher>(),
            provider.GetRequiredService<ITimestampProvider>(),
            provider.GetRequiredService<IJsonSerializer>(),
            _app.IsMultitenant() ? provider.GetRequiredService<ITenantProvider>() : new NoTenant()));

        return this;
    }

    public EventManagementOptions AddSimpleConsumer() => AddConsumer();

    public EventManagementOptions AddIdempotentConsumer()
    {
        _app.RequireModule<DataAccessModule>().Implementation.AddIdempotenceManager(_services, _app);

        return AddConsumer(() =>
        {
            _services.Decorate<IEventBusMessageHandler, IdempotentMessageHandler>();
        });
    }

    private EventManagementOptions AddConsumer(Action decorate = null)
    {
        _eventBusImplementation.AddEventBusConsumer(_services);

        var eventTypes = EventTypeScanning.FindConsumerEventTypes(_app.ApplicationAssemblyMarker);

        _services.AddSingleton(new EventBusConsumerDefinition(eventTypes.Select(t => t.GetEventTypeName()).ToArray()));
        _services.AddScoped<IEventBusMessageHandler>(provider => new DefaultEventBusMessageHandler(
            provider.GetRequiredService<IExternalEventHandler>(),
            provider.GetRequiredService<IJsonSerializer>(),
            eventTypes));

        decorate?.Invoke();
        if (_app.IsMultitenant())
        {
            _services.Decorate<IEventBusMessageHandler, TenantAwareEventBusMessageHandler>();
        }
        _services.Decorate<IEventBusMessageHandler, TransactionalEventBusMessageHandler>();
        _services.Decorate<IEventBusMessageHandler, ErrorSafeEventBusMessageHandler>();

        _services.AddScoped<IExternalEventHandler, MediatorEventHandler>();

        return this;
    }
}
