using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;
using EasyDesk.CleanArchitecture.Testing.Integration.Commons;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Routing;
using Rebus.Transport;
using System.Diagnostics;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public sealed class RebusTestBusEndpoint : ITestBusEndpoint
{
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(10);

    private readonly IBus _bus;
    private readonly ISet<Type> _subscriptions = new HashSet<Type>();
    private readonly Channel<Message> _messages = Channel.CreateUnbounded<Message>();
    private readonly IList<Message> _deadLetter = new List<Message>();
    private readonly ITestTenantNavigator _tenantNavigator;
    private readonly Duration _timeout;
    private readonly BuiltinHandlerActivator _handlerActivator;

    public RebusTestBusEndpoint(Action<RebusConfigurer> configureRebus, ITestTenantNavigator tenantNavigator, Duration? timeout = null)
    {
        _tenantNavigator = tenantNavigator;
        _timeout = timeout ?? _defaultTimeout;

        _handlerActivator = new BuiltinHandlerActivator();
        _handlerActivator.Handle<IMessage>(Handler);

        var configurer = Configure.With(_handlerActivator);
        configureRebus(configurer);
        configurer.Options(o =>
        {
            o.Decorate<IPipeline>(c =>
            {
                return new PipelineStepConcatenator(c.Get<IPipeline>())
                    .OnSend(new TestTenantManagementStep(_tenantNavigator), PipelineAbsolutePosition.Front);
            });
        });
        _bus = configurer.Start();
    }

    public async Task Subscribe<T>() where T : IEvent
    {
        await _bus.Subscribe<T>();
        _subscriptions.Add(typeof(T));
    }

    public async Task Unsubscribe<T>() where T : IEvent
    {
        await _bus.Unsubscribe<T>();
        _subscriptions.Remove(typeof(T));
    }

    public async Task Publish<T>(T message) where T : IEvent
    {
        await _bus.Publish(message);
    }

    public async Task Send<T>(T message) where T : ICommand
    {
        await _bus.Send(message);
    }

    public async Task Defer<T>(T message, Duration delay) where T : ICommand
    {
        await _bus.Defer(delay.ToTimeSpan(), message);
    }

    private async Task Handler(IMessage message) =>
        await _messages.Writer.WriteAsync(MessageContext.Current.Message);

    public async Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        var (message, _) = await WaitForMessageAndHeadersOrFail(predicate, timeout);
        return message;
    }

    public async Task<(T Message, IDictionary<string, string> Headers)> WaitForMessageAndHeadersOrFail<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        var deadLetterMessage = _deadLetter
            .ZipWithIndex()
            .SelectMany(x => ValidateMessage(x.Item, predicate).Map(v => (x.Index, Message: v, x.Item.Headers)))
            .FirstOption();
        if (deadLetterMessage.IsPresent)
        {
            _deadLetter.RemoveAt(deadLetterMessage.Value.Index);
            return (deadLetterMessage.Value.Message, deadLetterMessage.Value.Headers);
        }

        var actualTimeout = timeout ?? _timeout;
        try
        {
            using var cts = new CancellationTokenSource(actualTimeout.ToTimeSpan());
            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }
                var message = await _messages.Reader.ReadAsync(cts.Token);
                var validatedMessage = ValidateMessage(message, predicate);
                if (validatedMessage.IsPresent)
                {
                    return (validatedMessage.Value, message.Headers);
                }
                _deadLetter.Add(message);
            }

            throw new UnreachableException();
        }
        catch (OperationCanceledException)
        {
            throw new MessageNotReceivedWithinTimeoutException(actualTimeout, typeof(T));
        }
    }

    public async Task FailIfMessageIsReceivedWithin<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        IMessage message;
        IDictionary<string, string> headers;
        try
        {
            (message, headers) = await WaitForMessageAndHeadersOrFail(predicate, timeout);
        }
        catch (MessageNotReceivedWithinTimeoutException)
        {
            return;
        }

        throw new UnexpectedMessageReceivedException(typeof(T), message, headers);
    }

    private Option<T> ValidateMessage<T>(Message message, Func<T, bool> predicate) =>
        (_tenantNavigator.IsMultitenancyIgnored || _tenantNavigator.Tenant.Id == message.Headers.GetOption(MultitenantMessagingUtils.TenantIdHeader).Map(TenantId.New))
        && message.Body is T t
        && predicate(t)
        ? Some(t) : None;

    public async ValueTask DisposeAsync()
    {
        foreach (var sub in _subscriptions)
        {
            await _bus.Unsubscribe(sub);
        }
        _subscriptions.Clear();
        _bus.Dispose();
        _handlerActivator.Dispose();
        GC.SuppressFinalize(this);
    }

    public static RebusTestBusEndpoint CreateFromServices(
        IServiceProvider serviceProvider,
        ITestTenantNavigator testTenantNavigator,
        string? inputQueueAddress = null,
        Duration? defaultTimeout = null)
    {
        var options = serviceProvider.GetRequiredService<RebusMessagingOptions>();
        var serviceEndpoint = serviceProvider.GetRequiredService<RebusEndpoint>();
        var helperEndpoint = new RebusEndpoint(inputQueueAddress ?? GenerateNewRandomAddress());
        return new RebusTestBusEndpoint(
            rebus =>
            {
                options.Apply(serviceProvider, helperEndpoint, rebus);
                rebus.Routing(r => r.Decorate(c => new TestRouterWrapper(c.Get<IRouter>(), serviceEndpoint)));
            },
            testTenantNavigator,
            defaultTimeout);
    }

    private static string GenerateNewRandomAddress() => $"rebus-test-helper-{Guid.NewGuid()}";

    private class TestTenantManagementStep : IOutgoingStep
    {
        private readonly TenantManagementStep _inner;
        private readonly IServiceProvider _serviceProvider;

        public TestTenantManagementStep(ITenantProvider tenantProvider)
        {
            _inner = new TenantManagementStep();
            _serviceProvider = new ServiceCollection()
                .AddSingleton(tenantProvider)
                .BuildServiceProvider();
        }

        public async Task Process(OutgoingStepContext context, Func<Task> next)
        {
            context.Load<ITransactionContext>().SetServiceProvider(_serviceProvider);
            await _inner.Process(context, next);
        }
    }
}
