using EasyDesk.CleanArchitecture.DependencyInjection.Modules;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public abstract class AbstractAuthenticationProvider<O, H> : IAuthenticationProvider
    where O : AuthenticationSchemeOptions, new()
    where H : AuthenticationHandler<O>
{
    private readonly Action<O>? _configureOptions;

    protected AbstractAuthenticationProvider(Action<O>? configureOptions)
    {
        _configureOptions = configureOptions;
    }

    public void AddAuthenticationHandler(string schemeName, AuthenticationBuilder authenticationBuilder) =>
        authenticationBuilder.AddScheme<O, H>(schemeName, _configureOptions);

    public abstract void AddUtilityServices(IServiceCollection services, AppDescription app);

    public abstract void ConfigureOpenApi(string schemeName, SwaggerGenOptions options);
}
