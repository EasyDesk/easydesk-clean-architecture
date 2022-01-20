using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox;
using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Features;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Application.Tenants;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;

public static class EventManagementExtensions
{
    public static EventManagementBuilder AddEventManagement(
        this IServiceCollection services, IEventBusImplementation eventBusImplementation, AppDescription app)
    {
        eventBusImplementation.AddCommonServices(services);
        return new(services, eventBusImplementation, app);
    }
}

public class EventManagementBuilder
{
    private readonly IServiceCollection _services;
    private readonly IEventBusImplementation _implementation;
    private readonly AppDescription _app;

    public EventManagementBuilder(IServiceCollection services, IEventBusImplementation implementation, AppDescription app)
    {
        _services = services;
        _implementation = implementation;
        _app = app;
    }

    public EventManagementBuilder AddOutboxPublisher()
    {
        _services.AddSingleton<IOutboxChannel, OutboxChannel>();
        _services.AddHostedService<PrimaryOutboxBackgroundService>();
        _services.AddHostedService<FallbackOutboxBackgroundService>();
        return AddPublisher(provider => new OutboxPublisher(
            provider.GetRequiredService<ITransactionManager>(),
            provider.GetRequiredService<IOutbox>(),
            provider.GetRequiredService<IOutboxChannel>()));
    }

    public EventManagementBuilder AddSimplePublisher()
    {
        return AddPublisher(provider => provider.GetRequiredService<IEventBusPublisher>());
    }

    private EventManagementBuilder AddPublisher(Func<IServiceProvider, IEventBusPublisher> eventBusPublisherImplementation)
    {
        _services.AddScoped<IExternalEventPublisher>(provider => new ExternalEventPublisher(
            eventBusPublisherImplementation(provider),
            provider.GetRequiredService<ITimestampProvider>(),
            provider.GetRequiredService<IJsonSerializer>(),
            _app.IsMultitenant() ? provider.GetRequiredService<ITenantProvider>() : new NoTenant()));
        return AddEventBusPublisherImplementation();
    }

    private EventManagementBuilder AddEventBusPublisherImplementation()
    {
        _implementation.AddPublisher(_services);
        return this;
    }

    public EventManagementBuilder AddIdempotentConsumer(params Type[] assemblyTypes)
    {
        return AddConsumer(assemblyTypes, () => _services.Decorate<IEventBusMessageHandler, IdempotentMessageHandler>());
    }

    public EventManagementBuilder AddSimpleConsumer(params Type[] assemblyTypes)
    {
        return AddConsumer(assemblyTypes, () => { });
    }

    private EventManagementBuilder AddConsumer(
        IEnumerable<Type> assemblyTypes,
        Action decorate)
    {
        var eventTypes = ReflectionUtils.InstantiableTypesInAssemblies(assemblyTypes)
            .SelectMany(t => GetEventType(t))
            .ToList();

        _services.AddSingleton(new EventBusConsumerDefinition(eventTypes.Select(t => t.GetEventTypeName()).ToArray()));
        _services.AddScoped<IExternalEventHandler, MediatorEventHandler>();
        _services.AddScoped<IEventBusMessageHandler>(provider => new DefaultEventBusMessageHandler(
            provider.GetRequiredService<IExternalEventHandler>(),
            provider.GetRequiredService<IJsonSerializer>(),
            eventTypes));

        decorate();
        if (_app.IsMultitenant())
        {
            _services.Decorate<IEventBusMessageHandler, TenantAwareEventBusMessageHandler>();
        }
        _services.Decorate<IEventBusMessageHandler, TransactionalEventBusMessageHandler>();
        _services.Decorate<IEventBusMessageHandler, ErrorSafeEventBusMessageHandler>();

        return AddEventBusConsumerImplementation();
    }

    private static Option<Type> GetEventType(Type handlerType)
    {
        if (handlerType is null)
        {
            return None;
        }
        if (handlerType.IsGenericType && handlerType.GetGenericTypeDefinition().Equals(typeof(ExternalEventHandlerBase<>)))
        {
            return handlerType.GetGenericArguments().First();
        }
        return GetEventType(handlerType.BaseType);
    }

    private EventManagementBuilder AddEventBusConsumerImplementation()
    {
        _implementation.AddConsumer(_services);
        return this;
    }
}
