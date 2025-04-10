using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using NodaTime;
using NodaTime.Testing;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Time;

public static class TestFixtureTimeExtensions
{
    public static TestFixtureConfigurer AddFakeClock(this TestFixtureConfigurer configurer, Instant initialInstant)
    {
        var clock = new FakeClock(initialInstant);

        configurer.ContainerBuilder.RegisterInstance(clock)
            .AsSelf()
            .As<IClock>()
            .SingleInstance();

        configurer.ConfigureHost(host => host.ConfigureContainer(builder =>
        {
            builder.RegisterInstance(clock)
                .As<IClock>()
                .SingleInstance();
        }));

        return configurer.RegisterLifetimeHooks(
            betweenTests: () => clock.Reset(initialInstant));
    }
}
