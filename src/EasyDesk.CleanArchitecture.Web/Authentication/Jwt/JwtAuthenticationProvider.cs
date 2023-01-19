using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using EasyDesk.Tools.Collections;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NodaTime;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Claims;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public class JwtAuthenticationProvider : IAuthenticationProvider
{
    private readonly Action<JwtBearerOptions> _configureOptions;

    public JwtAuthenticationProvider(Action<JwtBearerOptions> configureOptions)
    {
        _configureOptions = configureOptions;
    }

    public void AddUtilityServices(IServiceCollection services)
    {
        services.TryAddSingleton<JwtFacade>();
    }

    public void AddAuthenticationHandler(string schemeName, AuthenticationBuilder authenticationBuilder) =>
        authenticationBuilder.AddScheme<JwtBearerOptions, JwtBearerHandler>(schemeName, _configureOptions);

    public void ConfigureOpenApi(string schemeName, SwaggerGenOptions options)
    {
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Description = $"JWT Token Authentication ({schemeName})",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT"
        };
        options.ConfigureSecurityRequirement(schemeName, jwtSecurityScheme);
    }
}

public static class JwtAuthenticationExtensions
{
    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options, string schemeName, Action<JwtBearerOptions> configureOptions)
    {
        return options.AddScheme(schemeName, new JwtAuthenticationProvider(configureOptions));
    }

    public static void LogForgedJwtForUser(this IServiceProvider serviceProvider, string userId, string schemeName = null) =>
        serviceProvider.LogForgedJwt(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, schemeName);

    public static void LogForgedJwt(this IServiceProvider serviceProvider, IEnumerable<Claim> claims, string schemeName = null)
    {
        var authModuleOptions = serviceProvider.GetRequiredService<AuthenticationModuleOptions>();
        var scheme = schemeName ?? authModuleOptions.Schemes
            .Where(s => s.Value is JwtAuthenticationProvider)
            .Select(s => s.Key)
            .FirstOption()
            .OrElseThrow(() => new InvalidOperationException(
                $"Missing a {nameof(JwtAuthenticationProvider)} in the authentication configuration"));

        var jwtConfiguration = serviceProvider.GetJwtConfigurationFromAuthScheme(scheme);
        var logger = serviceProvider.GetRequiredService<ILogger<JwtAuthenticationProvider>>();
        var jwtFacade = serviceProvider.GetRequiredService<JwtFacade>();
        var jwt = jwtFacade.Create(claims, jwtConfiguration.ConfigureBuilder);
        logger.LogDebug("Forged JWT: {jwt}", jwt);
    }

    public static JwtGenerationConfiguration GetJwtConfigurationFromAuthScheme(
        this IServiceProvider serviceProvider,
        string schemeName)
    {
        var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(schemeName);
        return jwtBearerOptions.Configuration.ToJwtGenerationConfiguration();
    }

    public static JwtGenerationConfiguration ToJwtGenerationConfiguration(this JwtValidationConfiguration configuration) =>
        new(
            new SigningCredentials(configuration.ValidationKey, JwtConfigurationUtils.DefaultAlgorithm),
            Duration.FromDays(365),
            configuration.Issuers.FirstOption(),
            configuration.Audiences.FirstOption());
}
