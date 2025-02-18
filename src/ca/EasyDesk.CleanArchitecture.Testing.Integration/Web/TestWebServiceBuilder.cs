using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public sealed class TestWebServiceBuilder
{
    private readonly Type _startupAssemblyMarker;
    private Action<IDictionary<string, string?>>? _inMemoryConfig;
    private Action<IHostBuilder>? _configureHost;
    private Action<IWebHostBuilder>? _configureWebHost;
    private Action<IHost>? _beforeStart;

    public TestWebServiceBuilder(Type startupAssemblyMarker)
    {
        _startupAssemblyMarker = startupAssemblyMarker;
        _configureHost += builder =>
        {
            var dictionary = new Dictionary<string, string?>();
            _inMemoryConfig?.Invoke(dictionary);
            builder.ConfigureHostConfiguration(c => c.AddInMemoryCollection(dictionary));
        };
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

    public TestWebServiceBuilder WithConfiguration(string key, string value) =>
        WithConfiguration(d => d.Add(key, value));

    public TestWebServiceBuilder WithConfiguration(Action<IDictionary<string, string?>> configure)
    {
        _inMemoryConfig += configure;
        return this;
    }

    public TestWebServiceBuilder WithServices(Action<ContainerBuilder> configure) =>
        ConfigureHost(builder => builder.ConfigureContainer(configure));

    public TestWebServiceBuilder BeforeStart(Action<IHost> action)
    {
        _beforeStart += action;
        return this;
    }

    public ITestWebService Build()
    {
        var testWebServiceType = typeof(TestWebService<>).MakeGenericType(_startupAssemblyMarker);
        return (ITestWebService)Activator.CreateInstance(testWebServiceType, _configureHost, _configureWebHost, _beforeStart)!;
    }

    private class TestWebService<T> : WebApplicationFactory<T>, ITestWebService
        where T : class
    {
        private readonly Action<IHostBuilder>? _configureHost;
        private readonly Action<IWebHostBuilder>? _configureWebHost;
        private readonly Action<IHost>? _beforeStart;

        public TestWebService(
            Action<IHostBuilder>? configureHost,
            Action<IWebHostBuilder>? configureWebHost,
            Action<IHost>? beforeStart)
        {
            _configureHost = configureHost;
            _configureWebHost = configureWebHost;
            _beforeStart = beforeStart;
            HttpClient = CreateClient();
            HttpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        public HttpClient HttpClient { get; }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            _configureHost?.Invoke(builder);
            var host = builder.Build();
            _beforeStart?.Invoke(host);
            host.Start();
            return host;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _configureWebHost?.Invoke(builder);
        }
    }
}
