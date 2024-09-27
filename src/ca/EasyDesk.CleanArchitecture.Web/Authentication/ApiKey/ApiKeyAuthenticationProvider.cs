using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.Data.DependencyInjection;
using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication.ApiKey;

internal sealed class ApiKeyAuthenticationProvider : AbstractAuthenticationProvider<ApiKeyOptions, ApiKeyHandler>
{
    public ApiKeyAuthenticationProvider(Action<ApiKeyOptions>? configureOptions) : base(configureOptions)
    {
    }

    public override void AddUtilityServices(IServiceCollection services, AppDescription app)
    {
        services.AddScoped<ApiKeyValidator>();
        app.RequireModule<DataAccessModule>().Implementation.AddApiKeysManagement(services, app);
    }

    public override void ConfigureOpenApi(string schemeName, SwaggerGenOptions options)
    {
        var apiKeySecurityScheme = new OpenApiSecurityScheme
        {
            Description = $"ApiKey Token Authenticationn ({schemeName})",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = ApiKeyOptions.ApiKeyDefaultScheme,
        };
        options.ConfigureSecurityRequirement(schemeName, apiKeySecurityScheme);
    }
}
