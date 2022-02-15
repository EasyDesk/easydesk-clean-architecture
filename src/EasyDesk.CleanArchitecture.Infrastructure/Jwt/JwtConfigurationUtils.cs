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
    public const string DefaultConfigurationSectionName = "JwtSettings";

    public static JwtValidationConfiguration GetJwtValidationConfiguration(
        this IConfiguration configuration, string sectionName = DefaultConfigurationSectionName)
    {
        var section = configuration.RequireSection(sectionName);
        var validationSection = section.RequireSection("Validation");
        var issuers = validationSection.GetValueAsOption<IEnumerable<string>>("ValidIssuers");
        var audiences = validationSection.GetValueAsOption<IEnumerable<string>>("ValidAudiences");

        return builder =>
        {
            var finalBuilder = builder.WithSigningCredentials(GetSecretKeyFromSecction(section));
            issuers.IfPresent(issuers => finalBuilder.WithIssuerValidation(issuers));
            audiences.IfPresent(audiences => finalBuilder.WithAudienceValidation(audiences));
            return finalBuilder;
        };
    }

    public static JwtTokenConfiguration GetJwtTokenConfiguration(
        this IConfiguration configuration, string sectionName = DefaultConfigurationSectionName)
    {
        var section = configuration.RequireSection(sectionName);
        var authoritySection = section.RequireSection("Authority");
        return builder => builder
            .WithSigningCredentials(GetSecretKeyFromSecction(section), SecurityAlgorithms.HmacSha256Signature)
            .WithLifetime(Duration.FromTimeSpan(authoritySection.RequireValue<TimeSpan>("Lifetime")))
            .WithIssuer(authoritySection.RequireValue<string>("Issuer"))
            .WithAudience(authoritySection.RequireValue<string>("Audience"));
    }

    private static SecurityKey GetSecretKeyFromSecction(IConfigurationSection scopeSection) =>
        KeyUtils.KeyFromString(scopeSection.RequireValue<string>("SecretKey"));
}
