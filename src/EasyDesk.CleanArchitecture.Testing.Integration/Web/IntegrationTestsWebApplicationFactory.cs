using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public abstract class IntegrationTestsWebApplicationFactory<T> : WebApplicationFactory<T>
    where T : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(ConfigureConfiguration);
        return base.CreateHost(builder);
    }

    protected virtual void ConfigureConfiguration(IConfigurationBuilder config)
    {
    }

    public HttpTestHelper CreateHttpHelper()
    {
        var jsonSettings = Services.GetRequiredService<JsonSettingsConfigurator>();
        return new(CreateClient(), jsonSettings);
    }

    public RebusTestHelper CreateRebusHelper(string inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var options = Services.GetRequiredService<RebusMessagingOptions>();
        var endpoint = new RebusEndpoint(inputQueueAddress ?? Guid.NewGuid().ToString());
        return new RebusTestHelper(
            rebus => rebus.ConfigureStandardBehavior(endpoint, options, Services),
            defaultTimeout);
    }
}
