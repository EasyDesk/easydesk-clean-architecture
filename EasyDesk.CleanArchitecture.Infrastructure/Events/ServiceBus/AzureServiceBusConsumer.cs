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
        private readonly AzureServiceBusSettings _settings;
        private readonly ILogger<AzureServiceBusConsumer> _logger;

        public AzureServiceBusConsumer(
            IServiceScopeFactory serviceScopeFactory,
            ServiceBusClient client,
            AzureServiceBusSettings settings,
            ILogger<AzureServiceBusConsumer> logger)
        {
            _processor = client.CreateProcessor(
                settings.CompleteTopicPath,
                settings.SubscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    ReceiveMode = ServiceBusReceiveMode.PeekLock,
                    MaxConcurrentCalls = 1
                });
            _serviceScopeFactory = serviceScopeFactory;
            _settings = settings;
            _logger = logger;
        }

        public async Task StartListening()
        {
            _processor.ProcessMessageAsync += OnMessageReceived;
            _processor.ProcessErrorAsync += OnError;

            try
            {
                await _processor.StartProcessingAsync();
                _logger.LogInformation(
                    "Started listening on {topicName} -> {subscriptionName}",
                    _settings.CompleteTopicPath,
                    _settings.SubscriptionName);
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
            await (handlerResult switch
            {
                Handled => eventArgs.CompleteMessageAsync(serviceBusMessage),
                TransientFailure => eventArgs.AbandonMessageAsync(serviceBusMessage),
                GenericFailure or NotSupported => eventArgs.DeadLetterMessageAsync(serviceBusMessage),
                _ => Task.FromException(new InvalidOperationException())
            });
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
