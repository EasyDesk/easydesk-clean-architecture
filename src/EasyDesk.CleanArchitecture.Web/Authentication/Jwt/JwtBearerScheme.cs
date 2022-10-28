using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

internal class JwtBearerScheme : IAuthenticationScheme
{
    private readonly Action<JwtBearerOptions> _configureOptions;

    public JwtBearerScheme(Action<JwtBearerOptions> configureOptions)
    {
        _configureOptions = configureOptions;
    }

    public void AddUtilityServices(IServiceCollection services)
    {
        services.AddSingleton<JwtFacade>();
    }

    public void AddAuthenticationHandler(string schemeName, AuthenticationBuilder authenticationBuilder) =>
        authenticationBuilder.AddScheme<JwtBearerOptions, JwtBearerHandler>(schemeName, _configureOptions);

    public void ConfigureOpenApi(SwaggerGenOptions options)
    {
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Description = "Token Authentication",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT"
        };
        options.ConfigureSecurityRequirement(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
    }
}

public static class JwtBearerExtensions
{
    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options, string schemeName, Action<JwtBearerOptions> configureOptions)
    {
        return options.AddScheme(schemeName, new JwtBearerScheme(configureOptions));
    }
}
