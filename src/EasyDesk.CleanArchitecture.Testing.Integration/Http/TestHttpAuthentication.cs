using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Web.Authentication.Jwt;
using EasyDesk.Tools.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public static class TestHttpAuthentication
{
    public static ITestHttpAuthentication NoAuthentication => new NoAuthentication();

    public static ITestHttpAuthentication CreateFromServices(IServiceProvider serviceProvider)
    {
        return serviceProvider
            .GetService<AuthenticationModuleOptions>()
            .AsOption()
            .FlatMap(options => GetDefaultAuthenticationConfiguration(options, serviceProvider))
            .OrElseGet(() => NoAuthentication);
    }

    private static Option<ITestHttpAuthentication> GetDefaultAuthenticationConfiguration(AuthenticationModuleOptions options, IServiceProvider serviceProvider)
    {
        if (options.Schemes.IsEmpty())
        {
            return None;
        }
        var schemeName = options.DefaultScheme;
        var provider = options.Schemes[schemeName];
        return provider switch
        {
            JwtAuthenticationProvider => Some(GetJwtAuthenticationConfiguration(serviceProvider, schemeName)),
            _ => None
        };
    }

    private static ITestHttpAuthentication GetJwtAuthenticationConfiguration(IServiceProvider serviceProvider, string schemeName)
    {
        var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(schemeName);
        var jwtValidationConfiguration = jwtBearerOptions.Configuration;
        var jwtConfiguration = new JwtTokenConfiguration(
                new SigningCredentials(jwtValidationConfiguration.ValidationKey, JwtConfigurationUtils.DefaultAlgorithm),
                Duration.FromDays(365),
                jwtValidationConfiguration.Issuers.FirstOption(),
                jwtValidationConfiguration.Audiences.FirstOption());
        return new JwtHttpAuthentication(serviceProvider.GetRequiredService<JwtFacade>(), jwtConfiguration);
    }
}
