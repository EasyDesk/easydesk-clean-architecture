using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.Application.Messaging.Sender;
using EasyDesk.CleanArchitecture.Application.Modules;
using EasyDesk.CleanArchitecture.Application.Tenants.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Receiver.Idempotence;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Sender.Outbox;
using EasyDesk.Tools.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging.DependencyInjection;

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

    public MessagingOptions AddSimpleSender() => AddPublisher();

    public MessagingOptions AddOutboxSender()
    {
        _app.RequireModule<DataAccessModule>().Implementation.AddOutbox(_services, _app);

        _services.AddSingleton<IOutboxChannel, OutboxChannel>();
        _services.AddHostedService<PrimaryOutboxBackgroundService>();
        _services.AddHostedService<FallbackOutboxBackgroundService>();

        return AddPublisher(() =>
        {
            _services.Decorate<IMessageSender, OutboxSender>();
        });
    }

    private MessagingOptions AddPublisher(Action decorate = null)
    {
        _messageBrokerImplementation.AddMessageSender(_services);

        decorate?.Invoke();

        _services.AddScoped<IExternalEventPublisher, ExternalEventPublisher>();

        return this;
    }

    public MessagingOptions AddSimpleReceiver() => AddConsumer();

    public MessagingOptions AddIdempotentReceiver()
    {
        _app.RequireModule<DataAccessModule>().Implementation.AddIdempotenceManager(_services, _app);

        return AddConsumer(() =>
        {
            _services.Decorate<IMessageHandler, IdempotentMessageHandler>();
        });
    }

    private MessagingOptions AddConsumer(Action decorate = null)
    {
        _messageBrokerImplementation.AddMessageReceiver(_services);

        var eventTypes = EventTypeScanning.FindExternalEventTypes(_app.ApplicationAssemblyMarker);

        _services.AddSingleton(new MessageReceiverDefinition(eventTypes.Select(t => t.GetEventTypeName()).ToArray()));
        _services.AddScoped<IMessageHandler, ExternalEventMessageHandler>();

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
