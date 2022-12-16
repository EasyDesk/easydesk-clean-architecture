using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Infrastructure.Messaging;
using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Rebus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using Rebus.Routing.TypeBased;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public abstract class IntegrationTestsWebApplicationFactory<T> : WebApplicationFactory<T>, IAsyncLifetime
    where T : class
{
    public const string DefaultTestEnvironment = "IntegrationTest";

    private readonly ContainersContext _containers = new();

    public IntegrationTestsWebApplicationFactory()
    {
    }

    public HttpClient HttpClient { get; private set; }

    protected virtual string Environment => DefaultTestEnvironment;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(ConfigureConfiguration);
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environment);
    }

    protected virtual void ConfigureConfiguration(IConfigurationBuilder config)
    {
    }

    public TContainer RegisterTestContainer<TContainer>(Func<ITestcontainersBuilder<TContainer>, ITestcontainersBuilder<TContainer>> configureContainer)
        where TContainer : ITestcontainersContainer
    {
        return _containers.RegisterTestContainer(configureContainer);
    }

    public HttpTestHelper CreateHttpHelper(IClock clock, Action<HttpRequestBuilder> configure = null)
    {
        var jsonSettings = Services.GetRequiredService<JsonSettingsConfigurator>();
        var configuration = Services.GetRequiredService<IConfiguration>();
        return new(HttpClient, jsonSettings, clock, GetJwtTokenConfiguration(configuration), configure);
    }

    public RebusTestHelper CreateRebusHelper(string inputQueueAddress = null, Duration? defaultTimeout = null)
    {
        var options = Services.GetRequiredService<RebusMessagingOptions>();
        var appEndpoint = Services.GetRequiredService<RebusEndpoint>();
        var endpoint = new RebusEndpoint(inputQueueAddress ?? GenerateNewRandomAddress());
        return new RebusTestHelper(
            rebus => rebus
                .ConfigureStandardBehavior(endpoint, options, Services)
                .Routing(r => r.TypeBased().MapFallback(appEndpoint.InputQueueAddress)),
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

    protected virtual Option<JwtTokenConfiguration> GetJwtTokenConfiguration(IConfiguration configuration) =>
        configuration
            .GetSectionAsOption(JwtConfigurationUtils.DefaultConfigurationSectionName)
            .Map(_ => DeriveFromValidation(configuration));

    private JwtTokenConfiguration DeriveFromValidation(IConfiguration configuration)
    {
        var validationSection = configuration
            .RequireSection(JwtConfigurationUtils.DefaultConfigurationSectionName)
            .GetSectionAsOption(JwtConfigurationUtils.DefaultValidationSectionName);
        var authoritySectionPrefix = $"{JwtConfigurationUtils.DefaultConfigurationSectionName}:{JwtConfigurationUtils.DefaultAuthoritySectionName}";
        var validIssuers = validationSection.FlatMap(s => s.GetValueAsOption<IEnumerable<string>>(JwtConfigurationUtils.DefaultValidationIssuersKeyName));
        var validAudiences = validationSection.FlatMap(s => s.GetValueAsOption<IEnumerable<string>>(JwtConfigurationUtils.DefaultValidationAudiencesKeyName));
        var automaticTokenConfiguration = new Dictionary<string, string>();
        validIssuers
            .Filter(issuers => issuers.Any())
            .IfPresent(issuers =>
            {
                automaticTokenConfiguration[$"{authoritySectionPrefix}:{JwtConfigurationUtils.DefaultIssuerKeyName}"] = issuers.First();
            });
        validAudiences
            .Filter(audiences => audiences.Any())
            .IfPresent(audiences =>
            {
                automaticTokenConfiguration[$"{authoritySectionPrefix}:{JwtConfigurationUtils.DefaultAudienceKeyName}"] = audiences.First();
            });
        automaticTokenConfiguration[$"{authoritySectionPrefix}:{JwtConfigurationUtils.DefaultLifetimeKeyName}"] = TimeSpan.FromDays(365).ToString();
        var configurationOverride = new ConfigurationBuilder();
        configurationOverride.AddInMemoryCollection(automaticTokenConfiguration);
        configurationOverride.AddConfiguration(configuration);
        return JwtConfigurationUtils.GetJwtTokenConfiguration(configurationOverride.Build(), JwtConfigurationUtils.DefaultConfigurationSectionName);
    }
}
