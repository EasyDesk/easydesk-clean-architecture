﻿using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rebus.Persistence.InMem;
using Rebus.Subscriptions;
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
}
