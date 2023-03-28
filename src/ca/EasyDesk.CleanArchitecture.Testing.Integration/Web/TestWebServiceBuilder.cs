using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public class TestWebServiceBuilder
{
    private readonly Type _startupAssemblyMarker;
    private Action<IHostBuilder>? _configureHost;
    private Action<IWebHostBuilder>? _configureWebHost;

    public TestWebServiceBuilder(Type startupAssemblyMarker)
    {
        _startupAssemblyMarker = startupAssemblyMarker;
    }

    public TestWebServiceBuilder ConfigureHost(Action<IHostBuilder> configure)
    {
        _configureHost += configure;
        return this;
    }

    public TestWebServiceBuilder ConfigureWebHost(Action<IWebHostBuilder> configure)
    {
        _configureWebHost += configure;
        return this;
    }

    public TestWebServiceBuilder WithEnvironment(string environment) =>
        ConfigureWebHost(builder => builder.UseEnvironment(environment));

    public TestWebServiceBuilder WithConfiguration(Action<IConfigurationBuilder> configure) =>
        ConfigureHost(builder => builder.ConfigureHostConfiguration(configure));

    public TestWebServiceBuilder WithServices(Action<IServiceCollection> configure) =>
        ConfigureWebHost(builder => builder.ConfigureServices(configure));

    public ITestWebService Build()
    {
        var testWebServiceType = typeof(TestWebService<>).MakeGenericType(_startupAssemblyMarker);
        return (ITestWebService)Activator.CreateInstance(testWebServiceType, _configureHost, _configureWebHost)!;
    }

    private class TestWebService<T> : WebApplicationFactory<T>, ITestWebService
        where T : class
    {
        private readonly Action<IHostBuilder> _configureHost;
        private readonly Action<IWebHostBuilder> _configureWebHost;

        public TestWebService(Action<IHostBuilder> configureHost, Action<IWebHostBuilder> configureWebHost)
        {
            _configureHost = configureHost;
            _configureWebHost = configureWebHost;
            HttpClient = CreateClient();
        }

        public HttpClient HttpClient { get; }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            _configureHost?.Invoke(builder);
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _configureWebHost?.Invoke(builder);
        }
    }
}
