using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.CleanArchitecture.Web.Authentication.DependencyInjection;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Web.Authentication.Jwt;

public static class JwtAuthenticationExtensions
{
    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options,
        string schemeName,
        Action<JwtBearerOptions>? configureOptions = default)
    {
        return options.AddScheme(schemeName, new JwtAuthenticationProvider(configureOptions));
    }

    public static AuthenticationModuleOptions AddJwtBearer(
        this AuthenticationModuleOptions options,
        Action<JwtBearerOptions>? configureOptions = default)
    {
        return options.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, configureOptions);
    }

    public static JwtGenerationConfiguration ToJwtGenerationConfiguration(this JwtValidationConfiguration configuration) =>
        new(
            new SigningCredentials(configuration.ValidationKey, JwtConfigurationUtils.DefaultAlgorithm),
            Duration.FromDays(365),
            configuration.Issuers.FirstOption(),
            configuration.Audiences.FirstOption());
}
