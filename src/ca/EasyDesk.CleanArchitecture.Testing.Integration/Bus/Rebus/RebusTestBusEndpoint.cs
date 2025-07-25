﻿using Autofac;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging.Steps;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
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
    private static readonly Duration _defaultTimeout = Duration.FromSeconds(5);

    private readonly IBus _bus;
    private readonly ISet<Type> _subscriptions = new HashSet<Type>();
    private readonly Channel<Message> _messages = Channel.CreateUnbounded<Message>();
    private readonly IList<Message> _deadLetter = [];
    private readonly TestTenantManager _tenantManager;
    private readonly Duration _timeout;
    private readonly BuiltinHandlerActivator _handlerActivator;

    public RebusTestBusEndpoint(Action<RebusConfigurer> configureRebus, TestTenantManager tenantManager, Duration? timeout = null)
    {
        _tenantManager = tenantManager;
        _timeout = timeout ?? _defaultTimeout;

        _handlerActivator = new();
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

    public async Task FailIfMessageIsReceived<T>(Func<T, bool> predicate, Duration? timeout = null) where T : IMessage
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

    private Option<T> ValidateMessage<T>(Message message, Func<T, bool> predicate)
    {
        var messageTenantId = message.Headers
            .GetOption(MultitenantMessagingUtils.TenantIdHeader)
            .Map(id => TenantInfo.Tenant(new TenantId(id)))
            .OrElse(TenantInfo.Public);
        return _tenantManager.CurrentTenantInfo.All(x => x == messageTenantId)
            && message.Body is T t
            && predicate(t)
             ? Some(t) : None;
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

    public static RebusTestBusEndpoint CreateFromServices(
        IComponentContext context,
        TestTenantManager testTenantNavigator,
        string inputQueueAddress,
        Duration? defaultTimeout = null)
    {
        var options = context.Resolve<RebusMessagingOptions>();
        var serviceEndpoint = context.Resolve<RebusEndpoint>();
        var endpoint = new RebusEndpoint(inputQueueAddress);
        return new(
            rebus =>
            {
                options.Apply(context, endpoint, rebus);
                rebus.Routing(r => r.Decorate(c => new TestRouterWrapper(c.Get<IRouter>(), serviceEndpoint)));
            },
            testTenantNavigator,
            defaultTimeout);
    }

    private class TestTenantManagementStep : IOutgoingStep
    {
        private readonly TenantManagementStep _inner;
        private readonly IComponentContext _context;

        public TestTenantManagementStep(TestTenantManager tenantManager)
        {
            _inner = new();

            var builder = new ContainerBuilder();

            builder.RegisterInstance(new TestTenantProvider(tenantManager))
                .As<ITenantProvider>()
                .SingleInstance();

            _context = builder.Build();
        }

        public async Task Process(OutgoingStepContext context, Func<Task> next)
        {
            context.Load<ITransactionContext>().SetComponentContext(_context);
            await _inner.Process(context, next);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly TestTenantManager _tenantManager;

            public TestTenantProvider(TestTenantManager tenantManager)
            {
                _tenantManager = tenantManager;
            }

            public TenantInfo Tenant => _tenantManager.CurrentTenantInfo.OrElse(TenantInfo.Public);
        }
    }
}
