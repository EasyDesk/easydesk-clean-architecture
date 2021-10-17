using Azure.Messaging.ServiceBus;
using EasyDesk.CleanArchitecture.Application.Events.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Events.EventBus.EventBusMessageHandlerResult;

namespace EasyDesk.CleanArchitecture.Infrastructure.Events.ServiceBus
{
    public sealed class AzureServiceBusConsumer : IEventBusConsumer
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly AzureServiceBusReceiverDescriptor _descriptor;
        private readonly ILogger<AzureServiceBusConsumer> _logger;

        public AzureServiceBusConsumer(
            IServiceScopeFactory serviceScopeFactory,
            ServiceBusClient client,
            AzureServiceBusReceiverDescriptor descriptor,
            ILogger<AzureServiceBusConsumer> logger)
        {
            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                ReceiveMode = ServiceBusReceiveMode.PeekLock,
                MaxConcurrentCalls = 1
            };
            _processor = descriptor.Match(
                queue: q => client.CreateProcessor(q, options),
                subscription: (t, s) => client.CreateProcessor(t, s, options));
            _serviceScopeFactory = serviceScopeFactory;
            _descriptor = descriptor;
            _logger = logger;
        }

        public async Task StartListening()
        {
            _processor.ProcessMessageAsync += OnMessageReceived;
            _processor.ProcessErrorAsync += OnError;

            try
            {
                await _processor.StartProcessingAsync();
                _descriptor.Match(
                    queue: q => _logger.LogInformation("Started listening on queue '{queueName}'", q),
                    subscription: (t, s) => _logger.LogInformation(
                        "Started listening on subscription '{subscriptionName}' of '{topicName}'", s, t));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to start listening");
            }
        }

        private async Task OnMessageReceived(ProcessMessageEventArgs eventArgs)
        {
            var serviceBusMessage = eventArgs.Message;
            var message = serviceBusMessage.ToEventBusMessage();
            var handlerResult = await HandleMessage(message);
            await ProcessMessageHandlerResult(handlerResult, serviceBusMessage, eventArgs);
        }

        private Task ProcessMessageHandlerResult(
            EventBusMessageHandlerResult handlerResult,
            ServiceBusReceivedMessage serviceBusMessage,
            ProcessMessageEventArgs eventArgs)
        {
            return handlerResult switch
            {
                Handled => eventArgs.CompleteMessageAsync(serviceBusMessage),
                TransientFailure => eventArgs.AbandonMessageAsync(serviceBusMessage),
                GenericFailure or NotSupported => eventArgs.DeadLetterMessageAsync(serviceBusMessage),
                _ => Task.FromException(new InvalidOperationException())
            };
        }

        private async Task<EventBusMessageHandlerResult> HandleMessage(EventBusMessage message)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var handler = scope.ServiceProvider.GetRequiredService<IEventBusMessageHandler>();
                return await handler.Handle(message);
            }
        }

        private Task OnError(ProcessErrorEventArgs eventArgs) => Task.CompletedTask;

        public async void Dispose() => await _processor.DisposeAsync();
    }
}
