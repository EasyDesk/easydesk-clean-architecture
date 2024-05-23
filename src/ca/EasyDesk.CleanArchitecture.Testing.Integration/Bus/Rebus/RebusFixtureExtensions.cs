using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus.Scheduler;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
    public static WebServiceTestsFixtureBuilder<T> AddInMemoryRebus<T>(this WebServiceTestsFixtureBuilder<T> builder)
        where T : ITestFixture
    {
        var network = new InMemNetwork();
        var subscriberStore = new InMemorySubscriberStore();
        return builder
            .ConfigureWebService(ws => ws.WithServices(services =>
            {
                services.RemoveAll<RebusTransportConfiguration>();
                services.AddSingleton<RebusTransportConfiguration>((t, e) =>
                {
                    t.UseInMemoryTransport(network, e, registerSubscriptionStorage: false);
                    t.OtherService<ISubscriptionStorage>().StoreInMemory(subscriberStore);
                });
                services.AddHostedService<RebusResettingTask>();
            }))
            .OnReset(_ =>
            {
                network.Reset();
            });
    }

    public static WebServiceTestsFixtureBuilder<T> AddInMemoryRebusScheduler<T>(
        this WebServiceTestsFixtureBuilder<T> builder,
        string address,
        Duration pollInterval)
        where T : ITestFixture
    {
        IBus? bus = null;
        var store = new InMemTimeoutStore();
        return builder
            .OnInitialization(f =>
            {
                var rebus = Configure.With(new BuiltinHandlerActivator());
                var serviceProvider = f.WebService.Services;

                var transport = serviceProvider.GetRequiredService<RebusTransportConfiguration>();
                rebus
                    .Transport(t => transport(t, address))
                    .UseNodaTimeClock(serviceProvider.GetRequiredService<IClock>())
                    .Timeouts(t => t.Decorate(c => new InMemTimeoutManager(store, c.Get<IRebusTime>())))
                    .Options(o => o.SetDueTimeoutsPollInteval(pollInterval.ToTimeSpan()));
                bus = rebus.Start();
            })
            .OnReset(_ => store.Reset())
            .OnDisposal(_ => bus?.Dispose());
    }
}
