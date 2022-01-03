using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Events.EventBus.EventBusMessageHandlerResult;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus;

public sealed class AzureServiceBusConsumer : IEventBusConsumer
{
    private const int DefaultPrefetchCount = 8;
    private const int DeadLetterQueueBatchSize = 10;
    private static readonly TimeSpan _deadLetterQueueTimeout = TimeSpan.FromSeconds(1);

    private readonly ServiceBusProcessor _mainProcessor;
    private readonly ServiceBusReceiver _deadLetterQueueReceiver;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AzureServiceBusReceiverDescriptor _descriptor;
    private readonly ILogger<AzureServiceBusConsumer> _logger;

    public AzureServiceBusConsumer(
        IServiceScopeFactory serviceScopeFactory,
        ServiceBusClient client,
        AzureServiceBusReceiverDescriptor descriptor,
        ILogger<AzureServiceBusConsumer> logger)
    {
        var mainProcessorOptions = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            MaxConcurrentCalls = 1,
            PrefetchCount = DefaultPrefetchCount
        };
        _mainProcessor = descriptor.Match(
            queue: q => client.CreateProcessor(q, mainProcessorOptions),
            subscription: (t, s) => client.CreateProcessor(t, s, mainProcessorOptions));

        var deadLetterQueueReceiverOptions = new ServiceBusReceiverOptions
        {
            SubQueue = SubQueue.DeadLetter,
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
            PrefetchCount = DefaultPrefetchCount
        };
        _deadLetterQueueReceiver = descriptor.Match(
            queue: q => client.CreateReceiver(q, deadLetterQueueReceiverOptions),
            subscription: (t, s) => client.CreateReceiver(t, s, deadLetterQueueReceiverOptions));

        _serviceScopeFactory = serviceScopeFactory;
        _descriptor = descriptor;
        _logger = logger;
    }

    public async Task StartListening()
    {
        _mainProcessor.ProcessMessageAsync += OnMessageReceived;
        _mainProcessor.ProcessErrorAsync += OnError;

        try
        {
            await FlushDeadLetterQueue();
            await StartProcessor();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to start listening");
        }
    }

    private async Task StartProcessor()
    {
        await _mainProcessor.StartProcessingAsync();
        _descriptor.Match(
            queue: q => _logger.LogInformation("Started listening on queue '{queueName}'", q),
            subscription: (t, s) => _logger.LogInformation(
                "Started listening on subscription '{subscriptionName}' of '{topicName}'", s, t));
    }

    private async Task FlushDeadLetterQueue()
    {
        while (await HandleNextBatchFromDeadLetter())
        {
        }
    }

    private async Task<bool> HandleNextBatchFromDeadLetter()
    {
        var messages = await _deadLetterQueueReceiver.ReceiveMessagesAsync(DeadLetterQueueBatchSize, maxWaitTime: _deadLetterQueueTimeout);
        if (messages.Count == 0)
        {
            return false;
        }
        var allHandled = true;
        foreach (var message in messages)
        {
            var handlerResult = await HandleMessage(message);
            await ProcessDeadLetterHandlerResult(handlerResult, message);
            if (handlerResult is not Handled)
            {
                allHandled = false;
            }
        }
        return allHandled;
    }

    private async Task OnMessageReceived(ProcessMessageEventArgs eventArgs)
    {
        var serviceBusMessage = eventArgs.Message;
        var handlerResult = await HandleMessage(serviceBusMessage);
        await ProcessMainMessageResult(handlerResult, serviceBusMessage, eventArgs);
    }

    private Task ProcessMainMessageResult(
        EventBusMessageHandlerResult handlerResult,
        ServiceBusReceivedMessage serviceBusMessage,
        ProcessMessageEventArgs eventArgs)
    {
        return handlerResult switch
        {
            Handled => eventArgs.CompleteMessageAsync(serviceBusMessage),
            TransientFailure => eventArgs.AbandonMessageAsync(serviceBusMessage),
            _ => eventArgs.DeadLetterMessageAsync(serviceBusMessage)
        };
    }

    private Task ProcessDeadLetterHandlerResult(
        EventBusMessageHandlerResult handlerResult,
        ServiceBusReceivedMessage serviceBusMessage)
    {
        return handlerResult switch
        {
            Handled => _deadLetterQueueReceiver.CompleteMessageAsync(serviceBusMessage),
            _ => _deadLetterQueueReceiver.AbandonMessageAsync(serviceBusMessage)
        };
    }

    private async Task<EventBusMessageHandlerResult> HandleMessage(ServiceBusReceivedMessage serviceBusMessage)
    {
        var eventBusMessage = serviceBusMessage.ToEventBusMessage();
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var handler = scope.ServiceProvider.GetRequiredService<IEventBusMessageHandler>();
            return await handler.Handle(eventBusMessage);
        }
    }

    private Task OnError(ProcessErrorEventArgs eventArgs) => Task.CompletedTask;

    public async void Dispose() => await _mainProcessor.DisposeAsync();
}
