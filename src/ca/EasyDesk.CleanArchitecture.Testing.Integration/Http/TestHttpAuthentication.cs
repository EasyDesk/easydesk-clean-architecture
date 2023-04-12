using EasyDesk.CleanArchitecture.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.Commons.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public static class TestHttpAuthentication
{
    public static ITestHttpAuthentication NoAuthentication => new NoAuthentication();

    public static ITestHttpAuthentication CreateFromServices(IServiceProvider serviceProvider)
    {
        return serviceProvider
            .GetServiceAsOption<AuthenticationModuleOptions>()
            .FlatMap(options => GetDefaultAuthenticationConfiguration(options, serviceProvider))
            .OrElseGet(() => NoAuthentication);
    }

    private static Option<ITestHttpAuthentication> GetDefaultAuthenticationConfiguration(AuthenticationModuleOptions options, IServiceProvider serviceProvider)
    {
        if (options.Schemes.IsEmpty())
        {
            return None;
        }
        var schemeName = options.DefaultScheme ?? throw new InvalidOperationException("A default scheme is not available.");
        var provider = options.Schemes[schemeName];
        return provider switch
        {
            JwtAuthenticationProvider => Some(GetJwtAuthenticationConfiguration(serviceProvider, schemeName)),
            _ => None
        };
    }

    private static ITestHttpAuthentication GetJwtAuthenticationConfiguration(IServiceProvider serviceProvider, string schemeName) =>
        new JwtHttpAuthentication(
            serviceProvider.GetRequiredService<JwtFacade>(),
            serviceProvider.GetJwtConfigurationFromAuthScheme(schemeName));
}
