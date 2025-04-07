using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Host;

internal class TestHostBuilder<T> : ITestHostBuilder
    where T : class
{
    private Action<IWebHostBuilder>? _configureWebHost;
    private Action<IHostBuilder>? _configureHost;

    public TestHostBuilder(Action<IWebHostBuilder>? configureWebHost = null)
    {
        _configureWebHost = configureWebHost;
    }

    public ITestHostBuilder WithEnvironment(string environment) =>
        ConfigureWebHost(builder => builder.UseEnvironment(environment));

    public ITestHostBuilder ConfigureContainer(Action<ContainerBuilder> configure) =>
        ConfigureHost(builder => builder.ConfigureContainer(configure));

    public ITestHostBuilder WithConfiguration(string key, string value) =>
        ConfigureWebHost(builder => builder.UseSetting(key, value));

    private ITestHostBuilder ConfigureWebHost(Action<IWebHostBuilder> configure)
    {
        _configureWebHost += configure;
        return this;
    }

    private ITestHostBuilder ConfigureHost(Action<IHostBuilder> configure)
    {
        _configureHost += configure;
        return this;
    }

    public TestHostApplicationFactory<T> CreateFactory() => new(_configureHost, _configureWebHost);
}
