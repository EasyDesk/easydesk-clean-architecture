using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using NodaTime.Testing;
using Rebus.Routing;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public abstract class IntegrationTestsWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime
    where T : class
{
    public const string DefaultTestEnvironment = "IntegrationTest";

    private readonly ContainersContext _containers = new();

    public IntegrationTestsWebApplicationFactory()
    {
        Clock = new FakeClock(SystemClock.Instance.GetCurrentInstant());
    }

    public HttpClient HttpClient { get; private set; }

    public FakeClock Clock { get; }

    protected virtual string Environment => DefaultTestEnvironment;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(ConfigureConfiguration);
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environment);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(Clock);
        });
    }

    protected virtual void ConfigureConfiguration(IConfigurationBuilder config)
    {
    }

    public TContainer RegisterTestContainer<TContainer>(Func<ITestcontainersBuilder<TContainer>, ITestcontainersBuilder<TContainer>> configureContainer)
        where TContainer : ITestcontainersContainer
    {
        return _containers.RegisterTestContainer(configureContainer);
    }

    public HttpTestHelper CreateHttpHelper(Action<HttpRequestBuilder> configure = null)
    {
        var jsonSettings = Services.GetRequiredService<JsonSettingsConfigurator>();
        return new(HttpClient, jsonSettings, GetAuthenticationConfiguration(), configure);
    }

    public RebusTestHelper CreateRebusHelper(string inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var options = Services.GetRequiredService<RebusMessagingOptions>();
        var serviceEndpoint = Services.GetRequiredService<RebusEndpoint>();
        var helperEndpoint = new RebusEndpoint(inputQueueAddress ?? GenerateNewRandomAddress());
        return new RebusTestHelper(
            rebus => rebus
                .ConfigureStandardBehavior(helperEndpoint, options, Services)
                .Routing(r => r.Decorate(c => new TestRouterWrapper(c.Get<IRouter>(), serviceEndpoint))),
            defaultTimeout);
    }

    private string GenerateNewRandomAddress() => $"rebus-test-helper-{Guid.NewGuid()}";

    public virtual async Task InitializeAsync()
    {
        await _containers.StartAsync();
        HttpClient = CreateClient();
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _containers.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected virtual ITestHttpAuthentication GetAuthenticationConfiguration()
    {
        return Services
            .GetService<AuthenticationModuleOptions>()
            .AsOption()
            .FlatMap(options => GetDefaultAuthenticationConfiguration(options, Services))
            .OrElseGet(() => ITestHttpAuthentication.NoAuthentication);
    }

    private Option<ITestHttpAuthentication> GetDefaultAuthenticationConfiguration(AuthenticationModuleOptions options, IServiceProvider serviceProvider)
    {
        if (options.Schemes.IsEmpty())
        {
            return None;
        }
        var schemeName = options.DefaultScheme;
        var provider = options.Schemes[schemeName];
        return provider switch
        {
            JwtAuthenticationProvider => Some(GetJwtAuthenticationConfiguration(serviceProvider, schemeName)),
            _ => None
        };
    }

    private ITestHttpAuthentication GetJwtAuthenticationConfiguration(IServiceProvider serviceProvider, string schemeName)
    {
        var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(schemeName);
        var jwtValidationConfiguration = jwtBearerOptions.Configuration;
        var jwtConfiguration = new JwtTokenConfiguration(
                new SigningCredentials(jwtValidationConfiguration.ValidationKey, JwtConfigurationUtils.DefaultAlgorithm),
                Duration.FromDays(365),
                jwtValidationConfiguration.Issuers.FirstOption(),
                jwtValidationConfiguration.Audiences.FirstOption());
        return new JwtHttpAuthentication(Services.GetRequiredService<JwtFacade>(), jwtConfiguration);
    }
}
