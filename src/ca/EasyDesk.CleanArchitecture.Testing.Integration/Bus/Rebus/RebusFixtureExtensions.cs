using Autofac;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus.Scheduler;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;
using Microsoft.Extensions.Hosting;
using NodaTime;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Subscriptions;
using Rebus.Time;
using Rebus.Transport.InMem;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;

public static class RebusFixtureExtensions
{
    public static TestFixtureConfigurer AddInMemoryRebus(this TestFixtureConfigurer configurer)
    {
        var network = new InMemNetwork();
        var subscriberStore = new InMemorySubscriberStore();

        configurer.ContainerBuilder
            .RegisterType<TestBusEndpointsManager>()
            .InstancePerLifetimeScope();

        // TODO: use an abstraction or remove multitenancy entirely.
        configurer.ContainerBuilder
            .Register(_ => new TestTenantManager(new(None)))
            .IfNotRegistered(typeof(TestTenantManager))
            .InstancePerLifetimeScope();

        return configurer
            .ConfigureHost(host => host.ConfigureContainer(builder =>
            {
                builder.RegisterDecorator<RebusTransportConfiguration>((_, _, _) => (t, e) =>
                {
                    t.UseInMemoryTransport(network, e.InputQueueAddress, registerSubscriptionStorage: false);
                    t.OtherService<ISubscriptionStorage>().StoreInMemory(subscriberStore);
                });
                builder.RegisterType<RebusResettingTask>().As<IHostedService>().SingleInstance();
            }))
            .RegisterLifetimeHooks(betweenTests: network.Reset);
    }

    public static TestFixtureConfigurer AddInMemoryRebusScheduler(
        this TestFixtureConfigurer configurer,
        string address,
        Duration pollInterval)
    {
        return configurer.RegisterLifetimeHooks(c => new InMemoryRebusSchedulerLifetimeHooks(
            c.Resolve<ITestHost>(),
            address,
            pollInterval));
    }

    private class InMemoryRebusSchedulerLifetimeHooks : LifetimeHooks
    {
        private readonly ITestHost _testHost;
        private readonly string _address;
        private readonly Duration _pollInterval;

        private IBus? _bus;

        public InMemoryRebusSchedulerLifetimeHooks(ITestHost testHost, string address, Duration pollInterval)
        {
            _testHost = testHost;
            _address = address;
            _pollInterval = pollInterval;
        }

        public override Task BeforeTest()
        {
            var rebus = Configure.With(new BuiltinHandlerActivator());
            var lifetimeScope = _testHost.LifetimeScope;

            var transport = lifetimeScope.Resolve<RebusTransportConfiguration>();
            rebus
                .Transport(t => transport(t, new(_address)))
                .UseNodaTimeClock(lifetimeScope.Resolve<IClock>())
                .Timeouts(t => t.Decorate(c => new InMemTimeoutManager(new InMemTimeoutStore(), c.Get<IRebusTime>())))
                .Options(o =>
                {
                    o.SetDueTimeoutsPollInterval(_pollInterval.ToTimeSpan());
                });

            _bus = rebus.Start();

            return Task.CompletedTask;
        }

        public override Task AfterTest()
        {
            _bus!.Dispose();
            _bus = null;
            return Task.CompletedTask;
        }
    }
}
