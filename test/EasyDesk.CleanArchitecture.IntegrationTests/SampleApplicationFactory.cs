using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.SampleApp.Web.Controllers.V_1_0;
using Microsoft.Extensions.Configuration;

namespace EasyDesk.CleanArchitecture.IntegrationTests;

public class SampleApplicationFactory : IntegrationTestsWebApplicationFactory<PersonController>, IAsyncLifetime
{
    private readonly SampleAppContainersContext _containers = new();

    protected override void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:RabbitMq"] = _containers.RabbitMq.ConnectionString,
            ["ConnectionStrings:MainDb"] = _containers.Postgres.ConnectionString,
        });
    }

    public async Task InitializeAsync()
    {
        await _containers.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _containers.DisposeAsync();
    }
}
