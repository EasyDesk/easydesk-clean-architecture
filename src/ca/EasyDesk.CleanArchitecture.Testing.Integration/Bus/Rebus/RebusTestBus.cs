using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;
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

public class RebusTestBus : ITestBus
{
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(10);

    private readonly IBus _bus;
    private readonly ISet<Type> _subscriptions = new HashSet<Type>();
    private readonly Channel<IMessage> _messages = Channel.CreateUnbounded<IMessage>();
    private readonly IList<IMessage> _deadLetter = new List<IMessage>();
    private readonly Duration _timeout;
    private readonly BuiltinHandlerActivator _handlerActivator;
    private readonly TestTenantManager _tenantManager = new();

    public RebusTestBus(Action<RebusConfigurer> configureRebus, Duration? timeout = null)
    {
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
                    .OnSend(new TestTenantManagementStep(_tenantManager), PipelineAbsolutePosition.Front);
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

    public Task Defer<T>(T message, Duration delay, TenantId tenant) where T : ICommand => Defer(message, delay, tenant.AsSome());

    public Task Publish<T>(T message, TenantId tenant) where T : IEvent => Publish(message, tenant.AsSome());

    public Task Send<T>(T message, TenantId tenant) where T : ICommand => Send(message, tenant.AsSome());

    public Task Defer<T>(T message, Duration delay) where T : ICommand => Defer(message, delay, None);

    public Task Publish<T>(T message) where T : IEvent => Publish(message, None);

    public Task Send<T>(T message) where T : ICommand => Send(message, None);

    private async Task Publish<T>(T message, Option<TenantId> tenant = default) where T : IEvent
    {
        _tenantManager.TenantInfo = tenant.Map(TenantInfo.Tenant).OrElse(TenantInfo.Public);
        await _bus.Publish(message);
    }

    private async Task Send<T>(T message, Option<TenantId> tenant = default) where T : ICommand
    {
        _tenantManager.TenantInfo = tenant.Map(TenantInfo.Tenant).OrElse(TenantInfo.Public);
        await _bus.Send(message);
    }

    private async Task Defer<T>(T message, Duration delay, Option<TenantId> tenant = default) where T : ICommand
    {
        _tenantManager.TenantInfo = tenant.Map(TenantInfo.Tenant).OrElse(TenantInfo.Public);
        await _bus.Defer(delay.ToTimeSpan(), message);
    }

    private async Task Handler(IMessage message) =>
        await _messages.Writer.WriteAsync(message);

    public async Task<T> WaitForMessageOrFail<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
    {
        var (m, deadLetterIndex) = await WaitForMessage(predicate, timeout);
        deadLetterIndex.IfPresent(_deadLetter.RemoveAt);
        return m;
    }

    private async Task<(T, Option<int>)> WaitForMessage<T>(Func<T, bool> predicate, Duration? timeout = null)
    {
        var actualTimeout = timeout ?? _timeout;
        try
        {
            using var cts = new CancellationTokenSource(actualTimeout.ToTimeSpan());
            var messages = _deadLetter
                .Select((m, i) => (m, Some(i)))
                .ToAsyncEnumerable()
                .ThenConcat(() =>
                {
                    return _messages.Reader.ReadAllAsync(cts.Token).Select(m => (m, NoneT<int>()));
                });

            await foreach (var (m, deadLetterIndex) in messages)
            {
                if (m is T t && predicate(t))
                {
                    return (t, deadLetterIndex);
                }
                _deadLetter.Add(m);
            }

            throw new UnreachableException();
        }
        catch (OperationCanceledException)
        {
            throw new MessageNotReceivedWithinTimeoutException(actualTimeout, typeof(T));
        }
    }

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

    public static RebusTestBus CreateFromServices(IServiceProvider serviceProvider, string? inputQueueAddress = null, Duration? defaultTimeout = null)
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

    private class TestTenantManager : ITenantProvider
    {
        public TenantInfo TenantInfo { get; set; } = TenantInfo.Public;
    }
}
