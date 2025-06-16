using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public static class TestHttpAuthentication
{
    public static ITestHttpAuthentication NoAuthentication => new NoAuthentication();

    public static ITestHttpAuthentication CreateFromServices(IComponentContext componentContext)
    {
        return componentContext
            .ResolveOption<AuthenticationModuleOptions>()
            .FlatMap(options => GetDefaultAuthenticationConfiguration(options, componentContext))
            .OrElseGet(() => NoAuthentication);
    }

    private static Option<ITestHttpAuthentication> GetDefaultAuthenticationConfiguration(AuthenticationModuleOptions options, IComponentContext componentContext)
    {
        if (options.Schemes.IsEmpty())
        {
            return None;
        }
        var schemeName = options.DefaultScheme.Value;
        var provider = options.GetSchemeProvider(schemeName).Value;
        return provider switch
        {
            JwtBearerProvider jwtProvider => Some(GetJwtAuthenticationConfiguration(componentContext, jwtProvider)),
            _ => None,
        };
    }

    private static ITestHttpAuthentication GetJwtAuthenticationConfiguration(IComponentContext componentContext, JwtBearerProvider jwtProvider) =>
        new JwtHttpAuthentication(
            componentContext.Resolve<JwtFacade>(),
            jwtProvider.Options.Configuration.ToJwtGenerationConfiguration());
}
