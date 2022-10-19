using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.OpenApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.SampleApp.Web.Authentication;

public class TestAuthScheme : IAuthenticationScheme
{
    private readonly Action<TestAuthOptions> _options;

    public TestAuthScheme(Action<TestAuthOptions> options = null)
    {
        _options = options;
    }

    public void AddUtilityServices(IServiceCollection services)
    {
    }

    public void AddAuthenticationHandler(string schemeName, AuthenticationBuilder authenticationBuilder)
    {
        authenticationBuilder.AddScheme<TestAuthOptions, TestAuthHandler>(schemeName, _options);
    }

    public void ConfigureOpenApi(SwaggerGenOptions options)
    {
        options.ConfigureSecurityRequirement("Test", new OpenApiSecurityScheme
        {
            Description = "Test Authentication",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme
        });
    }
}

public static class TestAuthSchemeException
{
    public static AuthenticationModuleOptions AddTestAuth(this AuthenticationModuleOptions options, string schemeName, Action<TestAuthOptions> configure = null)
    {
        return options.AddScheme(schemeName, new TestAuthScheme(configure));
    }
}
