using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

internal sealed class JwtAuthenticationProvider : AbstractAuthenticationProvider<JwtBearerOptions, JwtBearerHandler>
{
    public JwtAuthenticationProvider(Action<JwtBearerOptions>? configureOptions) : base(configureOptions)
    {
    }

    public override void AddUtilityServices(IServiceCollection services)
    {
        services.TryAddSingleton<JwtFacade>();
    }

    public override void ConfigureOpenApi(string schemeName, SwaggerGenOptions options)
    {
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            Description = $"JWT Token Authentication ({schemeName})",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
        };
        options.ConfigureSecurityRequirement(schemeName, jwtSecurityScheme);
    }
}
