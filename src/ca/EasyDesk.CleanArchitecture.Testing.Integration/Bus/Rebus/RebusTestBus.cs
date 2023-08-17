using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;
using EasyDesk.Commons.Collections;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Routing;
using Rebus.Transport;
using System.Diagnostics;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public sealed class RebusTestBus : ITestBus
{
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(10);

    private record ReceivedMessage(IMessage Message, Option<TenantId> Tenant);

    private readonly IBus _bus;
    private readonly ISet<Type> _subscriptions = new HashSet<Type>();
    private readonly Channel<ReceivedMessage> _messages = Channel.CreateUnbounded<ReceivedMessage>();
    private readonly IList<ReceivedMessage> _deadLetter = new List<ReceivedMessage>();
    private readonly ITenantNavigator _tenantNavigator;
    private readonly Duration _timeout;
    private readonly BuiltinHandlerActivator _handlerActivator;

    public RebusTestBus(Action<RebusConfigurer> configureRebus, ITenantNavigator tenantNavigator, Duration? timeout = null)
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
        await _messages.Writer.WriteAsync(new(message, CommonTenantReaders.ReadFromMessageContext(MessageContext.Current).Map(TenantId.New)));

    public async Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        var deadLetterMessage = _deadLetter
            .Select(m => ValidateMessage(m, predicate))
            .ZipWithIndex()
            .SelectMany(x => x.Item.Map(v => (x.Index, Item: v)))
            .FirstOption();
        if (deadLetterMessage.IsPresent)
        {
            _deadLetter.RemoveAt(deadLetterMessage.Value.Index);
            return deadLetterMessage.Value.Item;
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
                    return validatedMessage.Value;
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

    private Option<T> ValidateMessage<T>(ReceivedMessage receivedMessage, Func<T, bool> predicate) =>
        _tenantNavigator.ContextTenant.Match(some: tenant => receivedMessage.Tenant == tenant.Id, none: () => true)
        && receivedMessage.Message is T t
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

    public static RebusTestBus CreateFromServices(
        IServiceProvider serviceProvider,
        ITenantNavigator testTenantNavigator,
        string? inputQueueAddress = null,
        Duration? defaultTimeout = null)
    {
        var options = serviceProvider.GetRequiredService<RebusMessagingOptions>();
        var serviceEndpoint = serviceProvider.GetRequiredService<RebusEndpoint>();
        var helperEndpoint = new RebusEndpoint(inputQueueAddress ?? GenerateNewRandomAddress());
        return new RebusTestBus(
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
