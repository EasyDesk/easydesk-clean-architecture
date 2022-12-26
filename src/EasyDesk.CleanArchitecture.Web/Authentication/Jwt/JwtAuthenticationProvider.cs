using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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

public static class JwtBearerExtensions
{
    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options, string schemeName, Action<JwtBearerOptions> configureOptions)
    {
        return options.AddScheme(schemeName, new JwtAuthenticationProvider(configureOptions));
    }
}
