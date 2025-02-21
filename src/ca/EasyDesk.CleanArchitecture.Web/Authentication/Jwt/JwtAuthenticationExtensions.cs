using EasyDesk.CleanArchitecture.Application.Authentication.DependencyInjection;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
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
        var jwtBearerOptions = new JwtBearerOptions();
        configureOptions?.Invoke(jwtBearerOptions);
        return options.AddScheme(schemeName, new JwtBearerProvider(jwtBearerOptions));
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
