using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Startup.Modules;
using EasyDesk.CleanArchitecture.Web.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public class JwtBearerScheme : IAuthenticationScheme
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

    public void ConfigureSwagger(SwaggerGenOptions options) => options.ConfigureJwtBearerAuthentication();
}

public static class JwtBearerExtensions
{
    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options, string schemeName, Action<JwtBearerOptions> configureOptions)
    {
        return options.AddScheme(schemeName, new JwtBearerScheme(configureOptions));
    }
}
