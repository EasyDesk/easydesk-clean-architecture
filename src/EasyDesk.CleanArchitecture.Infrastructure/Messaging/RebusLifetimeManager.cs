using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.Tools.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Config;
using Rebus.ServiceProvider;

namespace EasyDesk.CleanArchitecture.Infrastructure.Messaging;

public class RebusLifetimeManager : IPausableHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RebusMessagingOptions _options;
    private readonly Action<RebusConfigurer> _configureRebus;
    private Option<IBus> _bus;

    public RebusLifetimeManager(
        IServiceProvider serviceProvider,
        RebusMessagingOptions options,
        Action<RebusConfigurer> configureRebus)
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _configureRebus = configureRebus;
    }

    public IBus Bus
    {
        get
        {
            lock (this)
            {
                return _bus.OrElseThrow(() => new InvalidOperationException(
                    "Trying to access the current instance of the Rebus bus before its initialization"));
            }
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bus = StartRebus();

        if (_options.AutoSubscribe)
        {
            await AutoSubscribe(bus);
        }
    }

    public Task Pause(CancellationToken cancellationToken)
    {
        StopRebus();
        return Task.CompletedTask;
    }

    public Task Resume(CancellationToken cancellationToken)
    {
        StartRebus();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopRebus();
        return Task.CompletedTask;
    }

    private IBus StartRebus()
    {
        lock (this)
        {
            if (_bus.IsPresent)
            {
                return _bus.Value;
            }

            var configurer = Configure.With(new DependencyInjectionHandlerActivator(_serviceProvider));
            _configureRebus(configurer);
            var currentBus = configurer.Start();
            _bus = Some(currentBus);
            return currentBus;
        }
    }

    private async Task AutoSubscribe(IBus bus)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        using var scope = RebusTransactionScopeUtils.CreateScopeWithServiceProvider(serviceScope.ServiceProvider);
        foreach (var messageType in _options.KnownMessageTypes.Where(ShouldAutoSubscribe))
        {
            await bus.Subscribe(messageType);
        }
        await scope.CompleteAsync();
    }

    private bool ShouldAutoSubscribe(Type type) =>
        type.IsSubtypeOrImplementationOf(typeof(IIncomingEvent));

    private void StopRebus()
    {
        lock (this)
        {
            if (_bus.IsAbsent)
            {
                return;
            }
            _bus.Value.Dispose();
            _bus = None;
        }
    }
}
