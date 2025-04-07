using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Host;

internal class TestHostApplicationFactory<T> : WebApplicationFactory<T>
    where T : class
{
    private readonly Action<IHostBuilder>? _configureHost;
    private readonly Action<IWebHostBuilder>? _configureWebHost;

    public TestHostApplicationFactory(
        Action<IHostBuilder>? configureHost = null,
        Action<IWebHostBuilder>? configureWebHost = null)
    {
        _configureHost = configureHost;
        _configureWebHost = configureWebHost;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _configureWebHost?.Invoke(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _configureHost?.Invoke(builder);
        return base.CreateHost(builder);
    }

    public void Start()
    {
        _ = Server;
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        client.Timeout = Timeout.InfiniteTimeSpan;
    }
}
