using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public class JwtBearerScheme : IAuthenticationScheme
{
    private readonly Action<JwtBearerOptions> _options;

    public JwtBearerScheme(Action<JwtBearerOptions> options)
    {
        _options = options;
    }

    public string Name => JwtBearerDefaults.AuthenticationScheme;

    public void AddUtilityServices(IServiceCollection services)
    {
        services.AddSingleton<JwtService>();
    }

    public void AddAuthenticationHandler(AuthenticationBuilder authenticationBuilder) =>
        authenticationBuilder.AddScheme<JwtBearerOptions, JwtBearerHandler>(Name, _options);

    public void ConfigureSwagger(SwaggerGenOptions options) => options.ConfigureJwtBearerAuthentication();
}
