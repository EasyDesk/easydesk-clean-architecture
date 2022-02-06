using EasyDesk.CleanArchitecture.Infrastructure.Configuration;
using EasyDesk.Tools.Options;
using EasyDesk.Tools.PrimitiveTypes.DateAndTime;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;

namespace EasyDesk.CleanArchitecture.Infrastructure.Jwt;

public static class JwtConfigurationUtils
{
    public const string JwtConfigurationKey = "JwtScopes";

    public static JwtValidationConfiguration GetJwtValidationConfiguration(this IConfiguration configuration, string sectionName)
    {
        var section = configuration.RequireSection(sectionName);

        JwtValidationConfiguration validation = builder => builder
            .WithSigningCredentials(GetSecretKeyFromSecction(section));

        section.GetValueAsOption<IEnumerable<string>>("ValidIssuers").IfPresent(issuers =>
        {
            validation = builder => validation(builder).WithIssuerValidation(issuers);
        });
        section.GetValueAsOption<IEnumerable<string>>("ValidAudiences").IfPresent(audiences =>
        {
            validation = builder => validation(builder).WithAudienceValidation(audiences);
        });

        return validation;
    }

    public static JwtTokenConfiguration GetJwtTokenConfiguration(IConfiguration configuration, string sectionName)
    {
        var section = configuration.RequireSection(sectionName);
        return builder => builder
            .WithSigningCredentials(GetSecretKeyFromSecction(section), SecurityAlgorithms.HmacSha256Signature)
            .WithLifetime(Duration.FromTimeSpan(section.RequireValue<TimeSpan>("Lifetime")))
            .WithIssuer(section.RequireValue<string>("Issuer"))
            .WithAudience(section.RequireValue<string>("Audience"));
    }

    private static SecurityKey GetSecretKeyFromSecction(IConfigurationSection scopeSection) =>
        KeyUtils.KeyFromString(scopeSection.RequireValue<string>("SecretKey"));
}
