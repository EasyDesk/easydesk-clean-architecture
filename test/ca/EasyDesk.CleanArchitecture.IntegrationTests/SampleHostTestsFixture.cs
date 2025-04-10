using EasyDesk.CleanArchitecture.Testing.Integration.Bus.Rebus;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Time;
using EasyDesk.SampleApp.Web;
using EasyDesk.SampleHostApp.Service.V_1_0.AsyncCommands;
using NodaTime;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleHostTestsFixture : IntegrationTestsFixture
{
    protected override void ConfigureFixture(TestFixtureConfigurer configurer)
    {
        configurer.AddFakeClock(Instant.FromUtc(2021, 11, 20, 11, 45));

        configurer.RegisterHost<SendMessage>();

        configurer.AddInMemoryRebus();
        configurer.AddInMemoryRebusScheduler(Scheduler.Address, Duration.FromSeconds(1));
    }
}
