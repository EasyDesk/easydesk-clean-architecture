using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Extensions.DependencyInjection;
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
        var schemeName = options.DefaultScheme.Value;
        var provider = options.Schemes[schemeName];
        return provider switch
        {
            JwtBearerProvider jwtProvider => Some(GetJwtAuthenticationConfiguration(serviceProvider, jwtProvider)),
            _ => None,
        };
    }

    private static ITestHttpAuthentication GetJwtAuthenticationConfiguration(IServiceProvider serviceProvider, JwtBearerProvider jwtProvider) =>
        new JwtHttpAuthentication(
            serviceProvider.GetRequiredService<JwtFacade>(),
            jwtProvider.Options.Configuration.ToJwtGenerationConfiguration());
}
