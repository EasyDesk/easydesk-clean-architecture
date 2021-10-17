using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Idempotence;
using EasyDesk.CleanArchitecture.Application.Events.EventBus.Outbox;
using EasyDesk.CleanArchitecture.Application.Events.ExternalEvents;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Mediator;
using EasyDesk.CleanArchitecture.Domain.Time;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Events.DependencyInjection
{
    public static class EventManagementExtensions
    {
        public static EventBusImplementationBuilder AddEventManagement(this IServiceCollection services)
        {
            return new(services);
        }
    }

    public class EventBusImplementationBuilder
    {
        private readonly IServiceCollection _services;

        public EventBusImplementationBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public EventManagementBuilder AddEventBusImplementation(IEventBusImplementation implementation)
        {
            implementation.AddCommonServices(_services);
            return new(_services, implementation);
        }
    }

    public class EventManagementBuilder
    {
        private readonly IServiceCollection _services;
        private readonly IEventBusImplementation _implementation;

        public EventManagementBuilder(IServiceCollection services, IEventBusImplementation implementation)
        {
            _services = services;
            _implementation = implementation;
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
                provider.GetRequiredService<IJsonSerializer>()));
            return AddEventBusPublisherImplementation();
        }

        private EventManagementBuilder AddEventBusPublisherImplementation()
        {
            _implementation.AddPublisher(_services);
            return this;
        }

        public EventManagementBuilder AddIdempotentConsumer(params Type[] assemblyTypes)
        {
            return AddConsumer(assemblyTypes, (handler, provider) => new IdempotentMessageHandler(
                handler,
                provider.GetRequiredService<IIdempotenceManager>()));
        }

        public EventManagementBuilder AddSimpleConsumer(params Type[] assemblyTypes)
        {
            return AddConsumer(assemblyTypes, (handler, _) => handler);
        }

        private EventManagementBuilder AddConsumer(
            IEnumerable<Type> assemblyTypes,
            Func<IEventBusMessageHandler, IServiceProvider, IEventBusMessageHandler> decorate)
        {
            var eventTypes = ReflectionUtils.InstantiableTypesInAssemblies(assemblyTypes)
                .SelectMany(t => GetEventType(t))
                .ToList();

            _services.AddSingleton(new EventBusConsumerDefinition(eventTypes.Select(t => t.GetEventTypeName()).ToArray()));
            _services.AddScoped<IExternalEventHandler, MediatorEventHandler>();
            _services.AddScoped(provider => CreateEventBusMessageHandler(eventTypes, provider, decorate));

            return AddEventBusConsumerImplementation();
        }

        private IEventBusMessageHandler CreateEventBusMessageHandler(
            IEnumerable<Type> eventTypes,
            IServiceProvider provider,
            Func<IEventBusMessageHandler, IServiceProvider, IEventBusMessageHandler> decorate)
        {
            var initial = new DefaultEventBusMessageHandler(
                provider.GetRequiredService<IExternalEventHandler>(),
                provider.GetRequiredService<IJsonSerializer>(),
                eventTypes);

            var decorated = decorate(initial, provider);

            var transactional = new TransactionalEventBusMessageHandler(
                decorated,
                provider.GetRequiredService<ITransactionManager>());

            var errorSafe = new ErrorSafeEventBusMessageHandler(
                transactional,
                provider.GetRequiredService<ILogger<ErrorSafeEventBusMessageHandler>>());

            return errorSafe;
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
}
