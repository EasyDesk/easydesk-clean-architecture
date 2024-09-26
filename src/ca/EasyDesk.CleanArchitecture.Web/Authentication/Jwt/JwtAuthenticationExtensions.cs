using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public static class JwtAuthenticationExtensions
{
    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options,
        string schemeName,
        Action<JwtBearerOptions>? configureOptions = default)
    {
        return options.AddScheme(schemeName, new JwtAuthenticationProvider(configureOptions));
    }

    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options,
        Action<JwtBearerOptions>? configureOptions = default)
    {
        return options.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions);
    }

    public static void LogForgedJwt(this IServiceProvider serviceProvider, Agent agent, string? schemeName = null)
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
        var jwt = jwtFacade.Create(agent.ToClaimsIdentity(), jwtConfiguration.ConfigureBuilder);
        logger.LogWarning("Forged JWT: {jwt}", jwt);
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
